using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.EventGrid.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.EventGrid;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace SignalRFunctions
{
    public static class SignalRFunctions
    {
        public static string MachineId;
        public static string Time;
        public static bool Alert;
        public static bool tv_status;
        // public static string current_channel_name;
        // public static int current_channel_number;
        // public static int volume_level;
        // public static bool volume_mute;
        // public static int display_settings_brightness;
        // public static int display_settings_contrast;
        // public static string display_settings_color_temperature;
        // public static string active_input;
        // public static string audio_output;
        // public static bool network_status_connected;
        // public static int network_status_wifi_strength;
        // public static double tv_components_main_board_temperature;
        // public static int tv_components_power_supply_voltage;
        // public static double tv_components_power_supply_current;
        // public static string tv_components_display_panel_resolution;
        // public static int tv_components_display_panel_backlight_intensity;
        // public static int tv_components_audio_system_volume;
        // public static bool tv_components_audio_system_mute;
        // public static bool tv_components_wifi_module_connected;
        // public static int tv_components_wifi_module_signal_strength;

        [FunctionName("negotiate")]
        public static SignalRConnectionInfo GetSignalRInfo(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req,
            [SignalRConnectionInfo(HubName = "dttelemetry")] SignalRConnectionInfo connectionInfo)
        {
            return connectionInfo;
        }

        [FunctionName("broadcast")]
        public static Task SendMessage(
            [EventGridTrigger] EventGridEvent eventGridEvent,
            [SignalR(HubName = "dttelemetry")] IAsyncCollector<SignalRMessage> signalRMessages,
            ILogger log)
        {
            JObject eventGridData = (JObject)JsonConvert.DeserializeObject(eventGridEvent.Data.ToString());
            if (eventGridEvent.EventType.Contains("telemetry"))
            {
                var data = eventGridData.SelectToken("data");

                var telemetryMessage = new Dictionary<object, object>();
                foreach (JProperty property in data.Children())
                {
                    log.LogInformation(property.Name + " - " + property.Value);
                    telemetryMessage.Add(property.Name, property.Value);
                }
                return signalRMessages.AddAsync(
                new SignalRMessage
                {
                    Target = "TelemetryMessage",
                    Arguments = new[] { telemetryMessage }
                });
            }
            else
            {
                try
                {
                    MachineId = eventGridEvent.Subject;

                    var data = eventGridData.SelectToken("data");
                    var patch = data.SelectToken("patch");
                    foreach (JToken token in patch)
                    {
                        if (token["path"].ToString() == "/Alert")
                        {
                            Alert = token["value"].ToObject<bool>();
                        }
                    }

                    log.LogInformation($"setting alert to: {Alert}");
                    var property = new Dictionary<object, object>
                    {
                        {"MachineId", MachineId },
                        {"Alert", Alert }
                    };
                    return signalRMessages.AddAsync(
                        new SignalRMessage
                        {
                            Target = "PropertyMessage",
                            Arguments = new[] { property }
                        });
                }
                catch (Exception e)
                {
                    log.LogInformation(e.Message);
                    return null;
                }
            }

        }
    }
}

