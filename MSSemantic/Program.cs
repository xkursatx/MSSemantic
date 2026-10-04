using Microsoft.EntityFrameworkCore;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using MSSemantic.Data;
using MSSemantic.Plugins;
using MSSemantic.Services;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

var connectionString = configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("DefaultConnection bulunamadı.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));
builder.Services.AddScoped<IChatHistoryRepository, PostgresChatHistoryRepository>();
builder.Services.AddHttpClient<OllamaModelCatalog>();

builder.Services.AddScoped<Kernel>(serviceProvider =>
{
    var kernelBuilder = Kernel.CreateBuilder();
    var provider = configuration["AiProvider"] ?? "LocalOllama";

    if (provider == "LocalOllama")
    {
        kernelBuilder.AddOllamaChatCompletion(
            configuration["OllamaSettings:Local:Model"] ?? OllamaModels.Qwen25_14B,
            new Uri(configuration["OllamaSettings:Local:Endpoint"] ?? "http://localhost:11434"));
    }
    else if (provider == "OpenAI")
    {
        kernelBuilder.AddOpenAIChatCompletion(
            configuration["OpenAiSettings:Model"] ?? "gpt-4o-mini",
            configuration["OpenAiSettings:ApiKey"]
                ?? throw new InvalidOperationException("OpenAI API key bulunamadı."));
    }
    else
    {
        throw new InvalidOperationException($"Desteklenmeyen AI provider: {provider}");
    }

    var kernel = kernelBuilder.Build();
    kernel.Plugins.AddFromType<ProductsPlugin>(serviceProvider: serviceProvider);
    return kernel;
});

builder.Services.AddScoped<IChatCompletionService>(serviceProvider =>
    serviceProvider.GetRequiredService<Kernel>().GetRequiredService<IChatCompletionService>());
builder.Services.AddScoped<ChatService>();
builder.Services.AddControllersWithViews();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    await scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().Database.MigrateAsync();
}

app.UseStaticFiles();
app.UseRouting();
app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");

app.Run();
