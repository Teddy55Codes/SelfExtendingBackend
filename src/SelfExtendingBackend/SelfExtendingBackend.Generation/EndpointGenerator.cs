using FluentResults;
using SelfExtendingBackend.Contract;

namespace SelfExtendingBackend.Generation;

public class EndpointGenerator
{
    public Result<(IEndpoint, AiMessage)> GenerateEndpoint(string prompt)
    {
        var aiConnection = new AiConnection();
        AiMessage aiMessage = aiConnection.GenerateCodeWithAi(prompt);
        
        var isSuccess = false;
        var retries = 0;
        while (!isSuccess)
        {
            var result = new LibraryBuilder(aiMessage).BuildProject();
            if (result.IsFailed)
            {
                aiMessage = aiConnection.FixCodeWithAi(result.Errors[0].Message);
                if (retries > 5) return Result.Fail("Failed to Generate Endpoint");
                retries++;
            }
            else
            {
                isSuccess = true;
            }
        }

        var libraryLoader = new LibraryLoader(aiMessage.Name, aiMessage.Name);

        return Result.Ok((libraryLoader.LoadLibrary(), aiMessage));
    }
}