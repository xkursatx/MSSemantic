using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using MSSemantic.Plugins;


var kernelBuilder = Kernel.CreateBuilder();

kernelBuilder.AddOllamaChatCompletion(modelId: OllamaModels.Llama31_8B, endpoint: new Uri("http://192.168.5.200:11434"));
kernelBuilder.Plugins.AddFromType<ProductsPlugin>();

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

Kurallar:
- Her zaman Türkçe cevap ver.
- Gerektiğinde plugin kullan.
- Emin olmadığın veya kaynağı olmayan bilgileri uydurma.
- kullanıcıya samimiyetle, kibarca ve sade bir şekilde cevap ver.
""");


while (true)
{
    Thread.Sleep(100);
    Console.Write("Soru:");
    chatMessages.AddUserMessage(Console.ReadLine());
    var completion = chatService.GetStreamingChatMessageContentsAsync(
        chatMessages,
        kernel: kernel,
        executionSettings: new OpenAIPromptExecutionSettings()
        {
            ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions,
            FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
        });
    string fullMessage = "";
    Console.Write("Asistan :");
    await foreach (var content in completion)
    {
        Console.Write(content.Content);
        fullMessage += content;
    }

    chatMessages.AddAssistantMessage(fullMessage);
    Console.WriteLine("\r\n");
}
