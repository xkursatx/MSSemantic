using Microsoft.SemanticKernel;
using OllamaSharp;


var kernelBuilder = Kernel.CreateBuilder();

kernelBuilder.AddOllamaChatClient(modelId: "qwen2.5:14b", endpoint: new Uri("http://192.168.5.200:11434"));

var kernel = kernelBuilder.Build();

var result = await kernel.InvokePromptAsync("Serinlemenin en kolay yolu nedir?");

Console.WriteLine(result);