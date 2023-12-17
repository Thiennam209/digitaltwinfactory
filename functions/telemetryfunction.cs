using Azure;
using Azure.Core.Pipeline;
using Azure.DigitalTwins.Core;
using Azure.Identity;
using Microsoft.Azure.EventGrid.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.EventGrid;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Collections.Generic;

namespace My.Function
{
    // This class processes telemetry events from IoT Hub, reads temperature of a device
    // and sets the "Temperature" property of the device with the value of the telemetry.
    public class telemetryfunction
    {
        private static readonly HttpClient httpClient = new HttpClient();
        private static string adtServiceUrl = Environment.GetEnvironmentVariable("ADT_SERVICE_URL");

        [FunctionName("telemetryfunction")]
        public async void Run([EventGridTrigger] EventGridEvent eventGridEvent, ILogger log)
        {
            try
            {
                // After this is deployed, you need to turn the Managed Identity Status to "On",
                // Grab Object Id of the function and assigned "Azure Digital Twins Owner (Preview)" role
                // to this function identity in order for this function to be authorized on ADT APIs.
                //Authenticate with Digital Twins
                var credentials = new DefaultAzureCredential();
                log.LogInformation(credentials.ToString());
                DigitalTwinsClient client = new DigitalTwinsClient(
                    new Uri(adtServiceUrl), credentials, new DigitalTwinsClientOptions
                    { Transport = new HttpClientTransport(httpClient) });
                log.LogInformation($"ADT service client connection created.");
                if (eventGridEvent.Data.ToString().Contains("Alert"))
                {
                    JObject AlertMessage = (JObject)JsonConvert.DeserializeObject(eventGridEvent.Data.ToString());
                    string deviceId = (string)AlertMessage["systemProperties"]["iothub-connection-device-id"];

                    var ID = AlertMessage["body"]["MachineId"];
                    var Alert = AlertMessage["body"]["Alert"];

                    log.LogInformation($"Device:{deviceId} Device Id is:{ID}");
                    log.LogInformation($"Device:{deviceId} Alert Status is:{Alert}");

                    var updateProperty = new JsonPatchDocument();

                    updateProperty.AppendReplace("/Alert", Alert.Value<bool>());
                    updateProperty.AppendReplace("/MachineId", ID.Value<string>());

                    log.LogInformation(updateProperty.ToString());

                    try
                    {
                        await client.UpdateDigitalTwinAsync(deviceId, updateProperty);
                    }
                    catch (Exception e)
                    {
                        log.LogInformation(e.Message);
                    }
                }
                else if (eventGridEvent != null && eventGridEvent.Data != null)
                {

                    JObject deviceMessage = (JObject)JsonConvert.DeserializeObject(eventGridEvent.Data.ToString());
                    string deviceId = (string)deviceMessage["systemProperties"]["iothub-connection-device-id"];

                    var ID = deviceMessage["body"]["MachineId"];
                    var Time = deviceMessage["body"]["Time"];
                    var tv_status = deviceMessage["body"]["tv_status"];
                    var current_channel_name = deviceMessage["body"]["current_channel_name"];
                    var current_channel_number = deviceMessage["body"]["current_channel_number"];
                    var volume_level = deviceMessage["body"]["volume_level"];
                    var volume_mute = deviceMessage["body"]["volume_mute"];
                    var display_settings_brightness = deviceMessage["body"]["display_settings_brightness"];
                    var display_settings_contrast = deviceMessage["body"]["display_settings_contrast"];
                    var display_settings_color_temperature = deviceMessage["body"]["display_settings_color_temperature"];
                    var active_input = deviceMessage["body"]["active_input"];
                    var audio_output = deviceMessage["body"]["audio_output"];
                    var network_status_connected = deviceMessage["body"]["network_status_connected"];
                    var network_status_wifi_strength = deviceMessage["body"]["network_status_wifi_strength"];
                    var tv_components_main_board_temperature = deviceMessage["body"]["tv_components_main_board_temperature"];
                    var tv_components_power_supply_voltage = deviceMessage["body"]["tv_components_power_supply_voltage"];
                    var tv_components_power_supply_current = deviceMessage["body"]["tv_components_power_supply_current"];
                    var tv_components_display_panel_resolution = deviceMessage["body"]["tv_components_display_panel_resolution"];
                    var tv_components_display_panel_backlight_intensity = deviceMessage["body"]["tv_components_display_panel_backlight_intensity"];
                    var tv_components_audio_system_volume = deviceMessage["body"]["tv_components_audio_system_volume"];
                    var tv_components_audio_system_mute = deviceMessage["body"]["tv_components_audio_system_mute"];
                    var tv_components_wifi_module_connected = deviceMessage["body"]["tv_components_wifi_module_connected"];
                    var tv_components_wifi_module_signal_strength = deviceMessage["body"]["tv_components_wifi_module_signal_strength"];

                    var updateProperty = new JsonPatchDocument();

                    var machineTelemetry = new Dictionary<string, Object>()
                    {
                        ["MachineId"] = ID,
                        ["Time"] = Time,
                        ["tv_status"] = tv_status,
                        ["current_channel_name"] = current_channel_name,
                        ["current_channel_number"] = current_channel_number,
                        ["volume_level"] = volume_level,
                        ["volume_mute"] = volume_mute,
                        ["display_settings_brightness"] = display_settings_brightness,
                        ["display_settings_contrast"] = display_settings_contrast,
                        ["display_settings_color_temperature"] = display_settings_color_temperature,
                        ["active_input"] = active_input,
                        ["audio_output"] = audio_output,
                        ["network_status_connected"] = network_status_connected,
                        ["network_status_wifi_strength"] = network_status_wifi_strength,
                        ["tv_components_main_board_temperature"] = tv_components_main_board_temperature,
                        ["tv_components_power_supply_voltage"] = tv_components_power_supply_voltage,
                        ["tv_components_power_supply_current"] = tv_components_power_supply_current,
                        ["tv_components_display_panel_resolution"] = tv_components_display_panel_resolution,
                        ["tv_components_display_panel_backlight_intensity"] = tv_components_display_panel_backlight_intensity,
                        ["tv_components_audio_system_volume"] = tv_components_audio_system_volume,
                        ["tv_components_audio_system_mute"] = tv_components_audio_system_mute,
                        ["tv_components_wifi_module_connected"] = tv_components_wifi_module_connected,
                        ["tv_components_wifi_module_signal_strength"] = tv_components_wifi_module_signal_strength,
                    };

                    updateProperty.AppendAdd("/MachineId", ID.Value<string>());
                    updateProperty.AppendAdd("/Time", Time.Value<string>());
                    updateProperty.AppendAdd("/tv_status", tv_status.Value<bool>());
                    updateProperty.AppendAdd("/current_channel_name", current_channel_name.Value<string>());
                    updateProperty.AppendAdd("/current_channel_number", current_channel_number.Value<int>());
                    updateProperty.AppendAdd("/volume_level", volume_level.Value<int>());
                    updateProperty.AppendAdd("/volume_mute", volume_mute.Value<bool>());
                    updateProperty.AppendAdd("/display_settings_brightness", display_settings_brightness.Value<int>());
                    updateProperty.AppendAdd("/display_settings_contrast", display_settings_contrast.Value<int>());
                    updateProperty.AppendAdd("/display_settings_color_temperature", display_settings_color_temperature.Value<string>());
                    updateProperty.AppendAdd("/active_input", active_input.Value<string>());
                    updateProperty.AppendAdd("/audio_output", audio_output.Value<string>());
                    updateProperty.AppendAdd("/network_status_connected", network_status_connected.Value<bool>());
                    updateProperty.AppendAdd("/network_status_wifi_strength", network_status_wifi_strength.Value<int>());
                    updateProperty.AppendAdd("/tv_components_main_board_temperature", tv_components_main_board_temperature.Value<double>());
                    updateProperty.AppendAdd("/tv_components_power_supply_voltage", tv_components_power_supply_voltage.Value<int>());
                    updateProperty.AppendAdd("/tv_components_power_supply_current", tv_components_power_supply_current.Value<double>());
                    updateProperty.AppendAdd("/tv_components_display_panel_resolution", tv_components_display_panel_resolution.Value<string>());
                    updateProperty.AppendAdd("/tv_components_display_panel_backlight_intensity", tv_components_display_panel_backlight_intensity.Value<int>());
                    updateProperty.AppendAdd("/tv_components_audio_system_volume", tv_components_audio_system_volume.Value<int>());
                    updateProperty.AppendAdd("/tv_components_audio_system_mute", tv_components_audio_system_mute.Value<bool>());
                    updateProperty.AppendAdd("/tv_components_wifi_module_connected", tv_components_wifi_module_connected.Value<bool>());
                    updateProperty.AppendAdd("/tv_components_wifi_module_signal_strength", tv_components_wifi_module_signal_strength.Value<int>());

                    log.LogInformation(updateProperty.ToString());
                    try
                    {
                        await client.PublishTelemetryAsync(deviceId, Guid.NewGuid().ToString(), JsonConvert.SerializeObject(machineTelemetry));
                    }
                    catch (Exception e)
                    {
                        log.LogInformation(e.Message);
                    }
                }
            }
            catch (Exception e)
            {
                log.LogInformation(e.Message);
            }
        }
    }
}