using System.Text.Json;
using Google.GenAI;
using Google.GenAI.Types;
using SPBackend.Requests.Queries.GetLlmChat;
using SPBackend.Services.LLM.Tools;

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
            model: "gemini-3-flash-preview",
            contents: conversation,
            config: config
        );
        return response.Candidates[0].Content;
    }
    
    private readonly Client _client;
    private readonly PowerSourceTools _powerSourceTools;
    private readonly List<Content> _conversation = new(); 
    
    public GeminiService(IConfiguration  configuration, PowerSourceTools powerSourceTools)
    {
        var apiKey = configuration["Gemini:ApiKey"]
                     ?? throw new Exception("Gemini API key not found");
        _client = new Client(apiKey: apiKey);
        _powerSourceTools = powerSourceTools;
    }
    
    public async Task<GetLlmChatResponse> GetResponse(string prompt)
    {
        var config = new GenerateContentConfig
        {
            Tools = new List<Tool>
            {
                PowerSourceTools.GetToolDefinition()
            },
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
        
        var toolResult = await _powerSourceTools.ExecuteAsync(functionCall.Name);
        var toolContent = new Content
        {
            Role = "tool",
            Parts = new List<Part>
            {
                new Part
                {
                    FunctionResponse = new FunctionResponse
                    {
                        Name = functionCall.Name,
                        Response = ToDictionary(toolResult)
                    }
                }
            }
        };
        _conversation.Add(toolContent);

        var finalResponse = await GenerateModelContentAsync(_conversation, config);

        return new GetLlmChatResponse
        {
            Answer = finalResponse.Parts[0].Text
        };
    }
}