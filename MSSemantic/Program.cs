using Microsoft.SemanticKernel;
using MSSemantic.Services;


var kernelBuilder = Kernel.CreateBuilder();

kernelBuilder.AddOllamaChatClient(modelId: "qwen2.5:14b", endpoint: new Uri("http://192.168.5.200:11434"));

var kernel = kernelBuilder.Build();

var chatService = new ChatService(kernel);

var result = await chatService.AskAsync("Türkiye'nin başkenti neresidir?");

Console.WriteLine(result);