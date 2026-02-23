using Google.GenAI.Types;
using SPBackend.Requests.Queries.GetPowerSource;
using SPBackend.Services.Mains;

namespace SPBackend.Services.LLM.Tools;

public class PowerSourceTools
{
    private readonly PowerSourceService _powerSourceService;

    public PowerSourceTools(PowerSourceService powerSourceService)
    {
        _powerSourceService = powerSourceService;
    }

    // TODO: turn into a variable
    public static Tool GetToolDefinition()
    {
        return new Tool
        {
            FunctionDeclarations = new List<FunctionDeclaration>
            {
                new FunctionDeclaration
                {
                    Name = "get_power_source",
                    Description = "Get the current household power source"
                }
            }
        };
    }

    public async Task<object> ExecuteAsync(string functionName)
    {
        if (functionName != "get_power_source")
            throw new InvalidOperationException($"Unknown function: {functionName}");

        var powerSourceInfo = await _powerSourceService.GetPowerSource();
        return powerSourceInfo;
    }
}