using System.Text.Json;
using Google.GenAI;
using Google.GenAI.Types;
using SPBackend.Requests.Commands.SendLlmChat;

namespace SPBackend.Services.LLM;

public class GeminiService
{
    private static Dictionary<string, object?> ToDictionary(object value)
    {
        var json = JsonSerializer.Serialize(value);
        return JsonSerializer.Deserialize<Dictionary<string, object?>>(json)!;
    }
    
    private async Task<Content> GenerateModelContentAsync(
        GenerateContentConfig config,
        string sessionId
    )
    {
        var response = await _client.Models.GenerateContentAsync(
            model: "gemini-2.5-flash", // models: gemini-3-flash-preview, gemini-2.5-flash, gemini-2.5-flash-lite
            contents: _conversation[sessionId],
            config: config
        );
        return response.Candidates[0].Content;
    }
    
    private readonly Client _client;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly Dictionary<string, List<Content>> _conversation = new(); 
    
    public GeminiService(IConfiguration  configuration, IServiceScopeFactory scopeFactory)
    {
        var apiKey = configuration["Gemini:ApiKey"]
                     ?? throw new Exception("Gemini API key not found");
        _client = new Client(apiKey: apiKey);
        _scopeFactory = scopeFactory;
    }
    
    public async Task<SendLlmChatResponse> GetResponse(SendLlmChatRequest request)
    {
        var prompt = request.Prompt;
        var sessionId = request.SessionId;
        
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
        
        if (!_conversation.ContainsKey(sessionId))
        {
            _conversation[sessionId] = new List<Content>();
        }
        
        _conversation[sessionId].Add(userContent);

        // Get the prompt response
        var firstModelContent = await GenerateModelContentAsync(config,  sessionId);
        _conversation[sessionId].Add(firstModelContent);
        
        // Check if any function call is necessary
        var functionCall = firstModelContent.Parts!.FirstOrDefault(p => p.FunctionCall != null)?.FunctionCall;
        if (functionCall is null)
        {
            return new SendLlmChatResponse
            {
                Answer = firstModelContent.Parts![0].Text
            };
        }
        
        // Execute the tool and add it to the conversation (referred to in the previous function call)
        using var scope = _scopeFactory.CreateScope();
        var functionRouter = scope.ServiceProvider.GetRequiredService<LlmFunctionRouter>();
        
        var toolResult = await functionRouter.ExecuteFunction(functionCall.Name, functionCall.Args);
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
        _conversation[sessionId].Add(toolContent);

        if (!toolResult.RequiresModel)
        {
            return new SendLlmChatResponse
            {
                Answer = toolResult.DirectMessage
            };
        }

        var finalResponse = await GenerateModelContentAsync(config, sessionId);
        _conversation[sessionId].Add(finalResponse);

        return new SendLlmChatResponse
        {
            Answer = finalResponse.Parts[0].Text
        };
    }
}