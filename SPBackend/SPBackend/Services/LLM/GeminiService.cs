using System.Text.Json;
using Google.GenAI;
using Google.GenAI.Types;
using SPBackend.Requests.Queries.GetLlmChat;

namespace SPBackend.Services.LLM;

public class GeminiService
{
    private static Dictionary<string, object?> ToDictionary(object value)
    {
        var json = JsonSerializer.Serialize(value);
        return JsonSerializer.Deserialize<Dictionary<string, object?>>(json)!;
    }
    
    private async Task<Content> GenerateModelContentAsync(
        List<Content> conversation,
        GenerateContentConfig config
    )
    {
        var response = await _client.Models.GenerateContentAsync(
            model: "gemini-2.5-flash", // models: gemini-3-flash-preview, gemini-2.5-flash, gemini-2.5-flash-lite
            contents: conversation,
            config: config
        );
        return response.Candidates[0].Content;
    }
    
    private readonly Client _client;
    private readonly LlmFunctionRouter _functionRouter;
    private readonly List<Content> _conversation = new(); 
    
    public GeminiService(IConfiguration  configuration, LlmFunctionRouter functionRouter)
    {
        var apiKey = configuration["Gemini:ApiKey"]
                     ?? throw new Exception("Gemini API key not found");
        _client = new Client(apiKey: apiKey);
        _functionRouter = functionRouter;
    }
    
    public async Task<GetLlmChatResponse> GetResponse(string prompt)
    {
        var config = new GenerateContentConfig
        {
            Tools = LlmToolDefinitions.AllTools,
            ToolConfig = new ToolConfig
            {
                FunctionCallingConfig = new FunctionCallingConfig
                {
                    Mode = FunctionCallingConfigMode.Auto
                }
            }
        };
        
        // Create and add the user content to history (just made of the user prompt)
        var userContent = new Content
        {
            Role = "user",
            Parts = new List<Part>
            {
                new Part { Text = prompt }
            }
        };
        _conversation.Add(userContent);

        // Get the prompt response
        var firstModelContent = await GenerateModelContentAsync(_conversation, config);
        _conversation.Add(firstModelContent);
        
        // Check if any function call is necessary
        var functionCall = firstModelContent.Parts!.FirstOrDefault(p => p.FunctionCall != null)?.FunctionCall;
        if (functionCall is null)
        {
            return new GetLlmChatResponse
            {
                Answer = firstModelContent.Parts![0].Text
            };
        }
        
        // Execute the tool and add it to the conversation (referred to in the previous function call)
        var toolResult = await _functionRouter.ExecuteFunction(functionCall.Name, functionCall.Args);
        var toolContent = new Content
        {
            Role = "tool",
            Parts = new List<Part>
            {
                new Part
                {
                    FunctionResponse = new FunctionResponse
                    {
                        Id = functionCall.Id, 
                        Name = functionCall.Name,
                        Response = ToDictionary(new
                        {
                            message = toolResult.DirectMessage,
                            data = toolResult.State
                        })
                    }
                }
            }
        };
        _conversation.Add(toolContent);

        if (!toolResult.RequiresModel)
        {
            return new GetLlmChatResponse
            {
                Answer = toolResult.DirectMessage
            };
        }

        var finalResponse = await GenerateModelContentAsync(_conversation, config);
        _conversation.Add(finalResponse);

        return new GetLlmChatResponse
        {
            Answer = finalResponse.Parts[0].Text
        };
    }
}