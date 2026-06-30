using Microsoft.SemanticKernel;

namespace MSSemantic.Services
{
    public class ChatService
    {
        private readonly Kernel _kernel;

        public ChatService(Kernel kernel)
        {
            _kernel = kernel;
        }

        public async Task<string> AskAsync(string question)
        {
            var result = await _kernel.InvokePromptAsync(question);
            return result.ToString();
        }
    }
}
