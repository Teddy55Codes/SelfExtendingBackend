using System.Text.RegularExpressions;
using FluentResults;
using Newtonsoft.Json;
using NuGet.Versioning;
using OpenAI.Chat;

namespace SelfExtendingBackend.Generation;

public class AiConnection
{
    private const string FixInstructionPart1 = "After running the generated code, I encountered the following build error:";

    private const string FixInstructionPart2 = "Please analyze the build error and fix the code accordingly, ensuring the error is resolved while maintaining adherence to the original requirements. Return the updated C# code and JSON in the same format as specified above.";


    private const string GenerateCodeInstruction = """
                                                   Generate C# code and a separate JSON file for dependencies based on the following requirements, with a specific structure for output to allow easy parsing.

                                                   ### C# Code Requirements:
                                                   1. Use the `IEndpoint` interface in your generated class witch has the following methods, for this add the using Statement for "SelfExtendingBackend.Contract" where this Interface is originally implemented:
                                                      - `HttpContent Request(string body)` - This method should correctly handle requests based on whether a `body` is provided or not. If no `body` is expected, handle it accordingly (i.e., send an empty body or `null`).
                                                      - `string Url { get; }` - This property should simply return the URL provided by the user in the prompt input (e.g., `"/xy/"`).

                                                   2. Add the `[Export(typeof(IEndpoint))]` attribute to the class definition for exporting the implementation.

                                                   3. The code must be syntactically valid C# and ready for direct use.

                                                   ### JSON Dependencies Requirements:
                                                   1. Return a separate JSON file containing the necessary dependencies, structured as `"package_name": "version"`.
                                                   2. Use the following format for the JSON:
                                                      ```json
                                                      {
                                                        "package_name": "version"
                                                      }
                                                      ```

                                                   ### Specific Output Format:
                                                   Return the C# code and the JSON in a format that can be easily parsed programmatically in C#. Follow this structure:

                                                   1. For the C# code, output the code as follows:
                                                   
                                                      ```
                                                      ----C# CODE START----
                                                      (Place the full C# code here)
                                                      ----C# CODE END----
                                                      ```

                                                   2. For the JSON, output the dependencies as follows:
                                                   
                                                      ```
                                                      ----JSON START----
                                                      {
                                                        "package_name": "version"
                                                      }
                                                      ----JSON END----
                                                      ```

                                                   ### Example Input:
                                                   For the generated code, you will receive a URL path as input (e.g., `"/xy/"`). Additionally, you will be informed if the request requires a body or not, allowing the `Request` method to handle cases with or without the `body` parameter.

                                                   ### Example Output:
                                                   The C# code should include the following:
                                                   1. The full implementation of the class with the specified methods.
                                                   2. The `Url` property should return exactly what the user provides in the prompt.
                                                   3. The `Request` method should handle cases where a body is or is not provided, based on the input prompt.
                                                   4. Handle optional body scenarios appropriately in the `Request` method, sending `null` or an empty body when no body is required.

                                                   Ensure that the generated C# code works as described, with dependencies specified separately in a well-formatted JSON file and ensure the output follows the format for easy parsing.
                                                   """;

    private const string SecretKey =
        "";
    
    private readonly ChatCompletionOptions _options = new()
    {
    };

    private readonly List<ChatMessage> _chatMessages = [];

    private readonly ChatClient _client;

    public AiConnection(ChatClient client)
    {
        this._client = new ChatClient(
            model: "gpt-4o",
            apiKey: SecretKey);
    }

    public AiMessage GenerateCodeWithAi(string inputFromUser)
    {
        _chatMessages.Add(new UserChatMessage(GenerateCodeInstruction));
        _chatMessages.Add(new UserChatMessage(inputFromUser));

        return this.RunCodeWithAi();
    }

    public AiMessage FixCodeWithAi(string error)
    {
        _chatMessages.Add(new UserChatMessage(FixInstructionPart1 + error + FixInstructionPart2));

        return this.RunCodeWithAi();
    }

    private static AiMessage ParseAiResponse(string rawResponse)
    {
        // Use regex to extract C# code between the C# delimiters
        var csharpCodePattern = @"----C# CODE START----\s*(.*?)\s*----C# CODE END----";
        var csharpCode = Regex.Match(rawResponse, csharpCodePattern, RegexOptions.Singleline).Groups[1].Value;

        // Use regex to extract the class name from the C# code
        var classNamePattern = @"public\s+class\s+(\w+)\s*:";
        var classNameMatch = Regex.Match(csharpCode, classNamePattern);
        var className = classNameMatch.Success ? classNameMatch.Groups[1].Value : "No class found";

        // Use regex to extract JSON code between the JSON delimiters
        var jsonCodePattern = @"----JSON START----\s*(.*?)\s*----JSON END----";
        var jsonCode = Regex.Match(rawResponse, jsonCodePattern, RegexOptions.Singleline).Groups[1].Value;

        var dependencies = ParseJsonToPackageList(jsonCode);

        return new AiMessage(className, dependencies, csharpCode);
    }

    private static List<(string, NuGetVersion)> ParseJsonToPackageList(string json)
    {
        // Deserialize the JSON into a Dictionary<string, string> where key is the package name and value is the version
        var packageDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);

        // Convert the dictionary to a List<(string, NuGetVersion)>
        return (from package in packageDict
            let packageName = package.Key
            let version = NuGetVersion.Parse(package.Value)
            select (packageName, version)).ToList();
    }

    private AiMessage RunCodeWithAi()
    {
        ChatCompletion completion = _client.CompleteChat(_chatMessages, _options);
        var rawResponse = completion.Content[0].Text;

        _chatMessages.Add(rawResponse);

        return ParseAiResponse(rawResponse);
    }
}