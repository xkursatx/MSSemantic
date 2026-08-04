using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using MSSemantic.Data;
using MSSemantic.Plugins;

// =====================================================================
// YAPIPLANDIRMA AYARLARI - appsettings.json'dan okunuyor
// =====================================================================

var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .Build();

// AI Servis Seçimi: "LocalOllama", "CloudOllama", "OpenAI", "AzureOpenAI"
var aiProvider = configuration["AiProvider"] ?? "LocalOllama";

// Yerel Ollama Ayarları
var localOllamaEndpoint = configuration["OllamaSettings:Local:Endpoint"] ?? "http://192.168.5.200:11434";
var localOllamaModel = configuration["OllamaSettings:Local:Model"] ?? OllamaModels.Qwen35_4B;

// Cloud/Remote Ollama Ayarları
var cloudOllamaEndpoint = configuration["OllamaSettings:Cloud:Endpoint"] ?? "https://your-ollama-cloud-endpoint.com";
var cloudOllamaModel = configuration["OllamaSettings:Cloud:Model"] ?? "qwen2.5:3b";

// OpenAI Ayarları
var openAiApiKey = configuration["OpenAiSettings:ApiKey"] ?? "your-openai-api-key";
var openAiModel = configuration["OpenAiSettings:Model"] ?? "gpt-4o-mini";

// Azure OpenAI Ayarları
var azureOpenAiEndpoint = configuration["AzureOpenAiSettings:Endpoint"] ?? "https://your-resource.openai.azure.com";
var azureOpenAiApiKey = configuration["AzureOpenAiSettings:ApiKey"] ?? "your-azure-api-key";
var azureOpenAiDeployment = configuration["AzureOpenAiSettings:DeploymentName"] ?? "gpt-4";

// =====================================================================
// UYGULAMA BAŞLATMA
// =====================================================================

// ApplicationDbContext'i yapılandırıyoruz
var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
optionsBuilder.UseNpgsql(configuration.GetConnectionString("DefaultConnection"));
var dbContext = new ApplicationDbContext(optionsBuilder.Options);

// Veritabanını oluştur/güncelle
await dbContext.Database.MigrateAsync();

// Repository'yi DbContext ile oluşturuyoruz
var chatRepository = new PostgresChatHistoryRepository(dbContext);
await chatRepository.EnsureSchemaAsync();

var sessionId = await chatRepository.CreateSessionAsync();
Console.WriteLine($"Session: {sessionId}");
Console.WriteLine($"AI Provider: {aiProvider}\n");

var kernelBuilder = Kernel.CreateBuilder();

// AI Servis yapılandırması
switch (aiProvider)
{
    case "LocalOllama":
        Console.WriteLine($"Yerel Ollama kullanılıyor: {localOllamaEndpoint}");
        Console.WriteLine($"Model: {localOllamaModel}\n");
        kernelBuilder.AddOllamaChatCompletion(
            modelId: localOllamaModel,
            endpoint: new Uri(localOllamaEndpoint));
        break;

    case "CloudOllama":
        var cloudOllamaApiKey = configuration["OllamaSettings:Cloud:ApiKey"] ?? "your-ollama-cloud-api-key";

        Console.WriteLine($"Cloud Ollama kullanılıyor: {cloudOllamaEndpoint}");
        Console.WriteLine($"Model: {cloudOllamaModel}\n");

        // Header enjekte eden custom HttpClient hazırlanıyor
        var cloudHttpClient = new HttpClient();
        cloudHttpClient.BaseAddress = new Uri(cloudOllamaEndpoint);
        cloudHttpClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", cloudOllamaApiKey);

        kernelBuilder.AddOllamaChatCompletion(
            modelId: cloudOllamaModel,
            httpClient: cloudHttpClient // Custom HttpClient enjekte ediliyor
        );
        break;

    case "OpenAI":
        Console.WriteLine($"OpenAI kullanılıyor");
        Console.WriteLine($"Model: {openAiModel}\n");
        kernelBuilder.AddOpenAIChatCompletion(
            modelId: openAiModel,
            apiKey: openAiApiKey);
        break;

    case "AzureOpenAI":
        Console.WriteLine($"Azure OpenAI kullanılıyor: {azureOpenAiEndpoint}");
        Console.WriteLine($"Deployment: {azureOpenAiDeployment}\n");
        kernelBuilder.AddAzureOpenAIChatCompletion(
            deploymentName: azureOpenAiDeployment,
            endpoint: azureOpenAiEndpoint,
            apiKey: azureOpenAiApiKey);
        break;

    default:
        throw new InvalidOperationException($"Geçersiz AI provider: {aiProvider}");
}

// ProductsPlugin'e DbContext'i inject ediyoruz
kernelBuilder.Plugins.AddFromObject(new ProductsPlugin(dbContext));

var kernel = kernelBuilder.Build();

foreach (var plugin in kernel.Plugins)
{
    foreach (var function in plugin)
    {
        Console.WriteLine($"Plugin: {plugin.Name}, Function: {function.Name}");
    }
}

//var chatService = new ChatService(kernel);
var chatService = kernel.GetRequiredService<IChatCompletionService>();
var chatMessages = new ChatHistory(); // be able to reference past messages

chatMessages.AddSystemMessage("""
Sen UMAI Bilişim'in yapay zeka ürün asistanısın. 
Adın Cemile.
Dilbilgisi ve imla kurallarına dikkat ederek, düzgün bir lisan ile samimi ve kibar bir şekilde cevap vereceksin.
Sadece ve sadece UMAI Bilişim ürünleri hakkında bilgi verebilirsin. 
Kullanıcı konuyu değiştirse bile sadece UMAI Bilişim ürünleri hakkında cevap vereceksin.

Kurallar:
- Türkçe, İngilizce ve İspanyolca cevap verebilirsin.
- Gerektiğinde plugin kullan.
- Emin olmadığın veya kaynağı olmayan bilgileri uydurma.
- kullanıcıya samimiyetle, kibarca ve sade bir şekilde cevap ver.
""");


while (true)
{
    Console.Write("Soru: ");
    var input = Console.ReadLine();

    // FIX: Console.ReadLine() null dönebilir (örn. input stream kapanırsa,
    // Ctrl+D / EOF). Önceki versiyonda bu direkt AddUserMessage'a null
    // olarak geçip exception fırlatıyordu. Artık boş/null girişte
    // döngüyü sonlandırıyoruz.
    if (string.IsNullOrWhiteSpace(input))
    {
        Console.WriteLine("Çıkış yapılıyor...");
        break;
    }

    chatMessages.AddUserMessage(input);
    await chatRepository.SaveMessageAsync(sessionId, AuthorRole.User, input);

    var fullMessage = "";
    Console.Write("Asistan: ");

    // FIX: Ollama'ya ulaşılamazsa (servis düşer, network kopar) uygulama
    // artık tamamen çökmüyor, hatayı yazıp bir sonraki soruya geçiyor.
    try
    {
        var completion = chatService.GetStreamingChatMessageContentsAsync(
            chatMessages,
            kernel: kernel,
            executionSettings: new PromptExecutionSettings
            {
                // FIX: ToolCallBehavior kaldırıldı. Bu alan OpenAI connector'a
                // özel legacy bir API; Ollama connector'da yalnızca
                // FunctionChoiceBehavior.Auto() zaten yeterli ve doğru olan.
                FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
            });

        await foreach (var content in completion)
        {
            // FIX: "+= content" yerine açıkça ".Content" kullanıyoruz.
            // Örtük ToString() dönüşümüne güvenmek yerine, SK'nın
            // streaming içerik alanına doğrudan erişiyoruz.
            Console.Write(content.Content);
            fullMessage += content.Content;
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"\n[Hata] Modelden cevap alınamadı: {ex.Message} {ex.StackTrace}");
        continue;
    }

    chatMessages.AddAssistantMessage(fullMessage);
    await chatRepository.SaveMessageAsync(sessionId, AuthorRole.Assistant, fullMessage);

    Console.WriteLine("\r\n");
}
