using Newtonsoft.Json;
using SPBackend.Requests.Commands.AddPolicy;
using SPBackend.Requests.Commands.AddSchedule;
using SPBackend.Requests.Queries.GetAllPlugs;
using SPBackend.Requests.Queries.GetAllSources;
using SPBackend.Requests.Queries.GetPlugDetails;
using SPBackend.Requests.Commands.AddTimeout;
using SPBackend.Requests.Commands.DeletePolicy;
using SPBackend.Requests.Commands.DeleteSchedule;
using SPBackend.Requests.Commands.DeleteTimeout;
using SPBackend.Requests.Commands.RemovePlugFromSchedule;
using SPBackend.Requests.Commands.SetPlug;
using SPBackend.Requests.Commands.SetPlugName;
using SPBackend.Requests.Commands.TogglePolicy;
using SPBackend.Requests.Commands.ToggleSchedule;
using SPBackend.Requests.Queries.GetAllPolicies;
using SPBackend.Requests.Queries.GetPlugsPerRoom;
using SPBackend.Requests.Queries.GetPolicy;
using SPBackend.Requests.Queries.GetScheduleDetails;
using SPBackend.Requests.Queries.GetSchedules;
using SPBackend.Requests.Queries.GetSchedulesByDay;
using SPBackend.Requests.Queries.GetSchedulesNextDays;
using SPBackend.Services.Mains;
using SPBackend.Services.Plugs;
using SPBackend.Services.Rooms;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace SPBackend.Services.LLM;

public class LlmFunctionRouter
{
    private readonly Dictionary<string, Func<object?, Task<object>>> _handlers;

    public LlmFunctionRouter(PowerSourceService powerService, PlugsService plugsService, RoomsService roomsService)
    {
        _handlers = new()
        {
            // Power source services
            ["get_power_source"] = async (_) =>
                await powerService.GetPowerSource(),

            ["get_all_power_sources"] = async (_) =>
                await powerService.GetAllSources(new GetAllSourcesRequest(), new CancellationToken()), // TODO: cancellation token

            ["get_per_day_room_consumption"] = async (_) =>
                await powerService.GetPerDayRoomConsumption(),

            ["get_grouped_per_day_room_consumption"] = async (_) =>
                await powerService.GetGroupedPerDayRoomConsumption(),
            
            // Plugs Service
            ["get_all_plugs"] = async (_) => 
                await plugsService.GetAllPlugs(new GetAllPlugsRequest(), new CancellationToken()),

            ["get_plug_details"] = async (args) =>
            {
                var json = JsonSerializer.Serialize(args);
                var request = JsonSerializer.Deserialize<GetPlugDetailsRequest>(json);
                return await plugsService.GetPlugDetails(request!.PlugId, new CancellationToken());
            },
            
            ["set_plug_state"] = async (args) =>
            {
                var json = JsonSerializer.Serialize(args);
                var request = JsonSerializer.Deserialize<SetPlugRequest>(json)!;
                return await plugsService.SetPlug(request, new CancellationToken());
            },
            
            ["set_plug_name"] = async (args) =>
            {
                var json = JsonSerializer.Serialize(args);
                var request = JsonSerializer.Deserialize<SetPlugNameRequest>(json)!;
                return await plugsService.SetPlugName(request, new CancellationToken());
            },
            
            ["add_plug_timeout"] = async (args) =>
            {
                var json = JsonSerializer.Serialize(args);
                var request = JsonSerializer.Deserialize<AddTimeoutRequest>(json)!;
                return await plugsService.AddTimeout(request, new CancellationToken());
            },
            
            ["delete_plug_timeout"] = async (args) =>
            {
                var json = JsonSerializer.Serialize(args);
                var request = JsonSerializer.Deserialize<DeleteTimeoutRequest>(json)!;
                return await plugsService.DeleteTimeout(request.PlugId, new CancellationToken());
            },
            
            ["get_all_policies"] = async (args) =>
            {
                var request = new GetAllPoliciesRequest();

                if (args is not null)
                {
                    var json = JsonSerializer.Serialize(args);
                    request = JsonSerializer.Deserialize<GetAllPoliciesRequest>(json)!;
                }
                
                return await plugsService.GetAllPolicies(request, new CancellationToken());
            },
            
            ["get_policy_details"] = async (args) =>
            {
                var json = JsonSerializer.Serialize(args);
                var request = JsonSerializer.Deserialize<GetPolicyRequest>(json)!;
                return await plugsService.GetPolicy(request.PolicyId, new CancellationToken());
            },
            
            ["get_all_scheduled_days"] = async (args) =>
            {
                var request = new GetSchedulesRequest();

                if (args is not null)
                {
                    var json = JsonSerializer.Serialize(args);
                    request = JsonSerializer.Deserialize<GetSchedulesRequest>(json)!;
                }
                
                return await plugsService.GetSchedules(request, new CancellationToken());
            },
            
            ["get_next_schedules"] = async (_) => 
                await plugsService.GetSchedulesNextDays(new GetSchedulesNextDaysRequest(), new CancellationToken()),
            
            ["remove_plug_from_schedule"] = async (args) =>
            {
                var json = JsonSerializer.Serialize(args);
                var request = JsonSerializer.Deserialize<RemovePlugFromScheduleRequest>(json)!;
                return await plugsService.RemovePlugFromSchedule(request, new CancellationToken());
            },
            
            ["get_schedules_by_day"] = async (args) =>
            {
                var json = JsonSerializer.Serialize(args);
                var request = JsonSerializer.Deserialize<GetSchedulesByDayRequest>(json)!;
                return await plugsService.GetSchedulesByDay(request, new CancellationToken());
            },
            
            ["get_schedule_details"] = async (args) =>
            {
                var json = JsonSerializer.Serialize(args);
                var request = JsonSerializer.Deserialize<GetScheduleDetailsRequest>(json)!;
                return await plugsService.GetScheduleDetails(request, new CancellationToken());
            },
            
            ["add_schedule"] = async (args) =>
            {
                var json = JsonSerializer.Serialize(args);
                var request = JsonSerializer.Deserialize<AddScheduleRequest>(json)!;
                return await plugsService.AddSchedule(request, new CancellationToken());
            },
            
            ["delete_schedule"] = async (args) =>
            {
                var json = JsonSerializer.Serialize(args);
                var request = JsonSerializer.Deserialize<DeleteScheduleRequest>(json)!;
                return await plugsService.DeleteSchedule(request.ScheduleId, new CancellationToken());
            },
            
            ["toggle_schedule"] = async (args) =>
            {
                var json = JsonSerializer.Serialize(args);
                var request = JsonSerializer.Deserialize<ToggleScheduleRequest>(json)!;
                return await plugsService.ToggleSchedule(request, new CancellationToken());
            },
            
            ["add_policy"] = async (args) =>
            {
                var json = JsonSerializer.Serialize(args);
                var request = JsonSerializer.Deserialize<AddPolicyRequest>(json)!;
                return await plugsService.AddPolicy(request, new CancellationToken());
            },
            
            ["delete_policy"] = async (args) =>
            {
                var json = JsonSerializer.Serialize(args);
                var request = JsonSerializer.Deserialize<DeletePolicyRequest>(json)!;
                return await plugsService.DeletePolicy(request.PolicyId, new CancellationToken());
            },
            
            ["toggle_policy"] = async (args) =>
            {
                var json = JsonSerializer.Serialize(args);
                var request = JsonSerializer.Deserialize<TogglePolicyRequest>(json)!;
                return await plugsService.TogglePolicy(request, new CancellationToken());
            },
            
            // Rooms Service
            ["get_all_rooms"] = async (_) =>
                await roomsService.GetAllRooms(new CancellationToken()),

            ["get_plugs_per_room"] = async (args) =>
            {
                var json = JsonSerializer.Serialize(args);
                var request = JsonSerializer.Deserialize<GetPlugsPerRoomRequest>(json)!;
                return await roomsService.GetPlugsPerRoom(request.RoomId, new CancellationToken());
            },
        };
    }

    public async Task<ToolExecutionResult> ExecuteFunction(string functionName, object? arguments) // TODO: might need to add cancellation token
    {
        if (!_handlers.TryGetValue(functionName, out var handler))
            throw new InvalidOperationException($"Unknown function {functionName}");

        var response = await handler(arguments);
        
        if (response.GetType().GetProperty("Message") != null)
        {
            return  new ToolExecutionResult
            {
                RequiresModel = false,
                DirectMessage = (string) response.GetType().GetProperty("Message")!.GetValue(response)!,
                State = null
            };
        }
        else
        {
            return new ToolExecutionResult
            {
                RequiresModel = true,
                State = response
            };
        }
    }
}