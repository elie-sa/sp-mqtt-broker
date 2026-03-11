using Google.GenAI.Types;

namespace SPBackend.Services.LLM;

public static class LlmToolDefinitions
{
    public static readonly Tool PowerTool = new Tool
    {
        FunctionDeclarations = new List<FunctionDeclaration>
        {
            new() { Name = "get_power_source", Description = "Get the current household power source" },
            new() { Name = "get_all_power_sources", Description = "Get all available power sources" },
            new() { Name = "get_per_day_room_consumption", Description = "Get today's room consumption" },
            new() { Name = "get_grouped_per_day_room_consumption", Description = "Get today's consumption grouped by room type" }
        }
    };
    
    public static readonly Tool PlugTool = new Tool
    {
        FunctionDeclarations = new List<FunctionDeclaration>
        {
            new() { Name = "get_all_plugs", Description = "Get all plugs in the household" },
            new() {
                Name = "get_plug_details",
                Description = "Returns detailed information about a plug by its ID. The returned plug may have a null timeout, which means no timeout is configured.",
                Parameters = new Schema
                {
                    Type = "OBJECT",
                    Properties = new Dictionary<string, Schema>
                    {
                        ["PlugId"] = new Schema { Type = "INTEGER", Description = "The plug id" }
                    },
                    Required = new List<string> { "PlugId" }
                }
            }, // TODO: find a way for the model to know the id of the plug automatically from the name
            new() {
                Name = "set_plug_state",
                Description = "Turn a plug on or off",
                Parameters = new Schema
                {
                    Type = "OBJECT",
                    Properties = new Dictionary<string, Schema>
                    {
                        ["PlugId"] = new Schema { Type = "INTEGER", Description = "The plug id to turn on/off" },
                        ["SwitchOn"] = new Schema { Type = "BOOLEAN", Description = "The new on/off state of the plug"}
                    },
                    Required = new List<string> { "PlugId", "SwitchOn" } // TODO: let the model figure out whether to turn on/off?
                }
            },
            new() {
                Name = "set_plug_name",
                Description = "Rename a plug",
                Parameters = new Schema
                {
                    Type = "OBJECT",
                    Properties = new Dictionary<string, Schema>
                    {
                        ["PlugId"] = new Schema { Type = "INTEGER", Description = "The plug id to edit the name of" },
                        ["Name"] = new Schema { Type = "STRING", Description = "The new name of the plug"}
                    },
                    Required = new List<string> { "PlugId", "Name" }
                }
            }, 
            new() {
                Name = "add_plug_timeout",
                Description = "Add a timeout to a plug",
                Parameters = new Schema
                {
                    Type = "OBJECT",
                    Properties = new Dictionary<string, Schema>
                    {
                        ["PlugId"] = new Schema { Type = "INTEGER", Description = "The plug id" },
                        ["Timeout"] = new Schema { Type = "STRING", Description = "The timeout of the plug in the d.hh:mm:ss format"}
                    },
                    Required = new List<string> { "PlugId", "Timeout" } 
                }
            },
            new() {
                Name = "delete_plug_timeout",
                Description = "Remove a plug timeout",
                Parameters = new Schema
                {
                    Type = "OBJECT",
                    Properties = new Dictionary<string, Schema>
                    {
                        ["PlugId"] = new Schema { Type = "INTEGER", Description = "The plug id" } 
                    },
                    Required = new List<string> { "PlugId" }
                }
            },
            new()
            {
                Name = "get_all_policies",
                Description =
                    "Returns automation policies. Each policy may include a power source condition and/or a temperature condition. " +
                    "Power source is represented by powerSourceId and powerSourceName. " +
                    "Temperature conditions are represented by nullable tempGreaterThan and tempLessThan fields. " +
                    "If both temperature fields are null, the policy has no temperature condition.",
                Parameters = new Schema
                {
                    Type = "OBJECT",
                    Properties = new Dictionary<string, Schema>
                    {
                        ["PowerSourceOnly"] = new Schema
                        {
                            Type = "BOOLEAN",
                            Description =
                                "If true, returns only policies that have a power source condition and no temperature condition."
                        },
                        ["TempOnly"] = new Schema
                        {
                            Type = "BOOLEAN",
                            Description =
                                "If true, returns only policies that have a temperature condition (tempGreaterThan or tempLessThan) and no power source condition."
                        }
                    }
                }
            },
            new() {
                Name = "get_policy_details",
                Description = "Get details of a specific policy",
                Parameters = new Schema
                {
                    Type = "OBJECT",
                    Properties = new Dictionary<string, Schema>
                    {
                        ["PolicyId"] = new Schema { Type = "INTEGER", Description = "The policy id" } 
                    },
                    Required = new List<string> { "PolicyId" }
                }
            },
            new() {
                Name = "get_all_scheduled_days",
                Description = "Get the list of days where we have a schedule",
                Parameters = new Schema
                {
                    Type = "OBJECT",
                    Properties = new Dictionary<string, Schema>
                    {
                        ["PlugIds"] = new Schema { Type = "ARRAY", Items = new Schema { Type = "INTEGER" }, Description = "The plug ids we want to get the schedules of" } 
                    }
                }
            }, 
            new() {
                Name = "get_next_schedules",
                Description = "Get upcoming schedules for the next two days"
            },
            new()
            {
                Name = "remove_plug_from_schedule",
                Description = "Remove a specific plug from a schedule",
                Parameters = new Schema
                {
                    Type = "OBJECT",
                    Properties = new Dictionary<string, Schema>
                    {
                        ["PlugId"] = new Schema { Type = "INTEGER", Description = "The plug id to delete from the schedule" },
                        ["ScheduleId"] = new Schema { Type = "INTEGER", Description = "The targeted schedule id" } 
                    },
                    Required = new List<string> { "PlugId", "ScheduleId" }
                }
            },
            new()
            {
                Name = "get_schedules_by_day",
                Description = "Get the schedules of a specific date",
                Parameters = new Schema
                {
                    Type = "OBJECT",
                    Properties = new Dictionary<string, Schema>
                    {
                        ["Date"] = new Schema { Type = "STRING", Description = "The day to get the schedules in the format YYYY-MM-DD. If the user doesn't provide it in this format, convert it yourself" },  
                        ["PlugIds"] = new Schema { Type = "ARRAY", Items = new Schema { Type = "INTEGER" }, Description = "The list of plug ids to filter the schedules" } 
                    },
                    Required = new List<string> { "Date" }
                }
            }, 
            new()
            {
                Name = "get_schedule_details",
                Description = "Get all the details of a specific schedule",
                Parameters = new Schema
                {
                    Type = "OBJECT",
                    Properties = new Dictionary<string, Schema>
                    {
                        ["ScheduleId"] = new Schema { Type = "INTEGER", Description = "The schedule id" } 
                    },
                    Required = new List<string> { "ScheduleId" }
                }
            },
            new()
            {
                Name = "add_schedule",
                Description = "Add a new schedule",
                Parameters = new Schema
                {
                    Type = "OBJECT",
                    Properties = new Dictionary<string, Schema>
                    {
                        ["Name"] = new Schema { Type = "STRING", Description = "The schedule name" }, 
                        ["Time"] = new Schema { Type = "STRING", Description = "The scheduled time in the ISO8601 format YYYY-MM-DDTHH:mm:ss.SSSZ. If the user doesn't provide it in this format, convert it yourself" },  // TODO: CHECK THE DATE FORMAT
                        ["OnPlugIds"] = new Schema { Type = "ARRAY", Items = new Schema { Type = "INTEGER" }, Description = "The plug ids to turn on in this schedule" }, 
                        ["OffPlugIds"] = new Schema { Type = "ARRAY", Items = new Schema { Type = "INTEGER" }, Description = "The plug ids to turn off in this schedule" }, 
                        ["IsActive"] = new Schema { Type = "BOOLEAN", Description = "Specifies whether to activate a schedule or not. When creating, it should be True." }
                    },
                    Required = new List<string> { "Name", "Time", "OnPlugIds", "OffPlugIds" }
                }
            },
            new()
            {
                Name = "delete_schedule",
                Description = "Delete a schedule",
                Parameters = new Schema
                {
                    Type = "OBJECT",
                    Properties = new Dictionary<string, Schema>
                    {
                        ["ScheduleId"] = new Schema { Type = "INTEGER", Description = "The schedule id" } 
                    },
                    Required = new List<string> { "ScheduleId" }
                }
            },
            new()
            {
                Name = "toggle_schedule",
                Description = "Toggle the state of a schedule (to make it active or not)",
                Parameters = new Schema
                {
                    Type = "OBJECT",
                    Properties = new Dictionary<string, Schema>
                    {
                        ["ScheduleId"] = new Schema { Type = "INTEGER", Description = "The schedule id" },
                        ["Enable"] = new Schema { Type = "BOOLEAN", Description = "The new state of the schedule" } 
                    },
                    Required = new List<string> { "ScheduleId", "Enable" }
                }
            },
            new()
            {
                Name = "add_policy",
                Description = "Create a new policy",
                Parameters = new Schema
                {
                    Type = "OBJECT",
                    Properties = new Dictionary<string, Schema>
                    {
                        ["Name"] = new Schema { Type = "STRING", Description = "The policy name" }, 
                        ["PowerSourceId"] = new Schema { Type = "INTEGER", Description = "The power source id that would trigger this policy. If null, the power source doesn't affect this policy" }, 
                        ["TempGreaterThan"] = new Schema { Type = "NUMBER", Description = "The temperature greater than condition. If null, no condition is set on the temperature greater than" }, 
                        ["TempLessThan"] = new Schema { Type = "NUMBER", Description = "The temperature less than condition. If null, no condition is set on the temperature less than" }, 
                        ["IsActive"] = new Schema { Type = "BOOLEAN", Description = "Whether the policy is active or not" }, 
                        ["OnPlugIds"] = new Schema { Type = "ARRAY", Items = new Schema { Type = "INTEGER" }, Description = "The plug ids to turn on in this schedule" }, 
                        ["OffPlugIds"] = new Schema { Type = "ARRAY", Items = new Schema { Type = "INTEGER" }, Description = "The plug ids to turn off in this schedule" }
                    },
                    Required = new List<string> { "Name", "PowerSourceId", "TempGreaterThan", "TempLessThan", "IsActive", "OnPlugIds", "OffPlugIds" }
                }
            },
            new()
            {
                Name = "delete_policy",
                Description = "Delete a policy",
                Parameters = new Schema
                {
                    Type = "OBJECT",
                    Properties = new Dictionary<string, Schema>
                    {
                        ["PolicyId"] = new Schema { Type = "INTEGER", Description = "The policy id" } 
                    },
                    Required = new List<string> { "PolicyId" }
                }
            },
            new()
            {
                Name = "toggle_policy",
                Description = "Toggle the state of a policy (to make it active or not)",
                Parameters = new Schema
                {
                    Type = "OBJECT",
                    Properties = new Dictionary<string, Schema>
                    {
                        ["PolicyId"] = new Schema { Type = "INTEGER", Description = "The policy id" },
                        ["Enable"] = new Schema { Type = "BOOLEAN", Description = "The new state of the schedule" } 
                    },
                    Required = new List<string> { "PolicyId", "Enable" }
                }
            },
        }
        // TODO: to implement:
        // EditPolicy
        // EditSchedule
    };

    public static readonly Tool RoomTool = new Tool
    {
        FunctionDeclarations = new List<FunctionDeclaration>
        {
            new() { Name = "get_all_rooms", Description = "Get all rooms in the house" },
            new()
            {
                Name = "get_plugs_per_room",
                Description = "Get the plugs of a specific room",
                Parameters = new Schema
                {
                    Type = "OBJECT",
                    Properties = new Dictionary<string, Schema>
                    {
                        ["RoomId"] = new Schema { Type = "INTEGER", Description = "The room id" }
                    },
                    Required = new List<string> { "RoomId" }
                }
            },
        }
    };

    public static readonly List<Tool> AllTools = new List<Tool>
    {
        PowerTool,
        PlugTool,
        RoomTool
    };
}