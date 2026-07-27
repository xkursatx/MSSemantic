using Microsoft.SemanticKernel;

namespace MSSemantic.Services
{
    public class MyChatService
    {
        private readonly Kernel _kernel;

        public MyChatService(Kernel kernel)
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
