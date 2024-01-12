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
            log.LogInformation("Run");
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
                    JObject alertMessage = (JObject)JsonConvert.DeserializeObject(eventGridEvent.Data.ToString());
                    string deviceId = (string)alertMessage["systemProperties"]["iothub-connection-device-id"];
                    var ID = alertMessage["body"]["TurbineID"];
                    var alert = alertMessage["body"]["Alert"];
                    log.LogInformation($"Device:{deviceId} Device Id is:{ID}");
                    log.LogInformation($"Device:{deviceId} Alert Status is:{alert}");

                    var updateProperty = new JsonPatchDocument();
                    updateProperty.AppendReplace("/Alert", alert.Value<bool>());
                    updateProperty.AppendReplace("/TurbineID", ID.Value<string>());

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
                    log.LogInformation("eventGridEvent");
                    JObject deviceMessage = (JObject)JsonConvert.DeserializeObject(eventGridEvent.Data.ToString());
                    log.LogInformation("deviceMessage");

                    string deviceId = (string)deviceMessage["systemProperties"]["iothub-connection-device-id"];
                    log.LogInformation("deviceId");

                    var ID = deviceMessage["body"]["TurbineID"];
                    var TimeInterval = deviceMessage["body"]["TimeInterval"];
                    var TvStatus = deviceMessage["body"]["TvStatus"] != null ? deviceMessage["body"]["TvStatus"] : true;
                    var CurrentChannelName = deviceMessage["body"]["CurrentChannelName"] != null ? deviceMessage["body"]["CurrentChannelName"] : "";
                    var CurrentChannelNumber = deviceMessage["body"]["CurrentChannelNumber"] != null ? deviceMessage["body"]["CurrentChannelNumber"] : 0;
                    var VolumeLevel = deviceMessage["body"]["VolumeLevel"] != null ? deviceMessage["body"]["VolumeLevel"] : 0;
                    var VolumeMute = deviceMessage["body"]["VolumeMute"] != null ? deviceMessage["body"]["VolumeMute"] : false;
                    var DisplaySettingsBrightness = deviceMessage["body"]["DisplaySettingsBrightness"] != null ? deviceMessage["body"]["DisplaySettingsBrightness"] : 0;
                    var DisplaySettingsContrast = deviceMessage["body"]["DisplaySettingsContrast"] != null ? deviceMessage["body"]["DisplaySettingsContrast"] : 0;
                    var DisplaySettingsColorTemperature = deviceMessage["body"]["DisplaySettingsColorTemperature"] != null ? deviceMessage["body"]["DisplaySettingsColorTemperature"] : "";
                    var ActiveInput = deviceMessage["body"]["ActiveInput"] != null ? deviceMessage["body"]["ActiveInput"] : "";
                    var AudioOutput = deviceMessage["body"]["AudioOutput"] != null ? deviceMessage["body"]["AudioOutput"] : "";
                    var NetworkStatusConnected = deviceMessage["body"]["NetworkStatusConnected"] != null ? deviceMessage["body"]["NetworkStatusConnected"] : true;
                    var NetworkStatusWifiStrength = deviceMessage["body"]["NetworkStatusWifiStrength"] != null ? deviceMessage["body"]["NetworkStatusWifiStrength"] : 0;
                    var TvComponentsMainBoardTemperature = deviceMessage["body"]["TvComponentsMainBoardTemperature"] != null ? deviceMessage["body"]["TvComponentsMainBoardTemperature"] : 0;
                    var TvComponentsPowerSupplyVoltage = deviceMessage["body"]["TvComponentsPowerSupplyVoltage"] != null ? deviceMessage["body"]["TvComponentsPowerSupplyVoltage"] : 0;
                    var TvComponentsPowerSupplyCurrent = deviceMessage["body"]["TvComponentsPowerSupplyCurrent"] != null ? deviceMessage["body"]["TvComponentsPowerSupplyCurrent"] : 0;
                    var TvComponentsDisplayPanelResolution = deviceMessage["body"]["TvComponentsDisplayPanelResolution"] != null ? deviceMessage["body"]["TvComponentsDisplayPanelResolution"] : "";
                    var TvComponentsDisplayPanelBacklightIntensity = deviceMessage["body"]["TvComponentsDisplayPanelBacklightIntensity"] != null ? deviceMessage["body"]["TvComponentsDisplayPanelBacklightIntensity"] : 0;
                    var TvComponentsAudioSystemVolume = deviceMessage["body"]["TvComponentsAudioSystemVolume"] != null ? deviceMessage["body"]["TvComponentsAudioSystemVolume"] : 0;
                    var TvComponentsAudioSystemMute = deviceMessage["body"]["TvComponentsAudioSystemMute"] != null ? deviceMessage["body"]["TvComponentsAudioSystemMute"] : false;
                    var TvComponentsWifiModuleConnected = deviceMessage["body"]["TvComponentsWifiModuleConnected"] != null ? deviceMessage["body"]["TvComponentsWifiModuleConnected"] : true;
                    var TvComponentsWifiModuleSignalStrength = deviceMessage["body"]["TvComponentsWifiModuleSignalStrength"] != null ? deviceMessage["body"]["TvComponentsWifiModuleSignalStrength"] : 0;
                    var oxys = deviceMessage["body"]["oxys"] != null ? deviceMessage["body"]["oxys"] : 1;
                    var ats = deviceMessage["body"]["ats"] != null ? deviceMessage["body"]["ats"] : 1;
                    var pressure = deviceMessage["body"]["pressure"] != null ? deviceMessage["body"]["pressure"] : 1;
                    var cps = deviceMessage["body"]["cps"] != null ? deviceMessage["body"]["cps"] : 0;
                    var aps = deviceMessage["body"]["aps"] != null ? deviceMessage["body"]["aps"] : 0;
                    var sas = deviceMessage["body"]["sas"] != null ? deviceMessage["body"]["sas"] : 0;
                    var vss = deviceMessage["body"]["vss"] != null ? deviceMessage["body"]["vss"] : 0;
                    var iat = deviceMessage["body"]["iat"] != null ? deviceMessage["body"]["iat"] : 0;
                    var maf = deviceMessage["body"]["maf"] != null ? deviceMessage["body"]["maf"] : 0;
                    var ect = deviceMessage["body"]["ect"] != null ? deviceMessage["body"]["ect"] : 0;

                    log.LogInformation($"Device:{deviceId} Device Id is:{ID}");
                    log.LogInformation($"Device:{deviceId} Time interval is:{TimeInterval}");
                    log.LogInformation($"Device:{deviceId} TvStatus is:{TvStatus}");
                    log.LogInformation($"Device:{deviceId} AudioOutput is:{AudioOutput}");
                    log.LogInformation($"Device:{deviceId} TvComponentsAudioSystemVolumeis:{TvComponentsAudioSystemVolume}");

                    var updateProperty = new JsonPatchDocument();
                    var turbineTelemetry = new Dictionary<string, Object>()
                    {
                        ["TurbineID"] = ID,
                        ["TimeInterval"] = TimeInterval,
                        ["TvStatus"] = TvStatus,
                        ["CurrentChannelName"] = CurrentChannelName,
                        ["CurrentChannelNumber"] = CurrentChannelNumber,
                        ["VolumeLevel"] = VolumeLevel,
                        ["VolumeMute"] = VolumeMute,
                        ["DisplaySettingsBrightness"] = DisplaySettingsBrightness,
                        ["DisplaySettingsContrast"] = DisplaySettingsContrast,
                        ["DisplaySettingsColorTemperature"] = DisplaySettingsColorTemperature,
                        ["ActiveInput"] = ActiveInput,
                        ["AudioOutput"] = AudioOutput,
                        ["NetworkStatusConnected"] = NetworkStatusConnected,
                        ["NetworkStatusWifiStrength"] = NetworkStatusWifiStrength,
                        ["TvComponentsMainBoardTemperature"] = TvComponentsMainBoardTemperature,
                        ["TvComponentsPowerSupplyVoltage"] = TvComponentsPowerSupplyVoltage,
                        ["TvComponentsPowerSupplyCurrent"] = TvComponentsPowerSupplyCurrent,
                        ["TvComponentsDisplayPanelResolution"] = TvComponentsDisplayPanelResolution,
                        ["TvComponentsDisplayPanelBacklightIntensity"] = TvComponentsDisplayPanelBacklightIntensity,
                        ["TvComponentsAudioSystemVolume"] = TvComponentsAudioSystemVolume,
                        ["TvComponentsAudioSystemMute"] = TvComponentsAudioSystemMute,
                        ["TvComponentsWifiModuleConnected"] = TvComponentsWifiModuleConnected,
                        ["TvComponentsWifiModuleSignalStrength"] = TvComponentsWifiModuleSignalStrength,
                        ["oxys"] = oxys,
                        ["ats"] = ats,
                        ["pressure"] = pressure,
                        ["cps"] = cps,
                        ["aps"] = aps,
                        ["sas"] = sas,
                        ["vss"] = vss,
                        ["iat"] = iat,
                        ["maf"] = maf,
                        ["ect"] = ect
                    };

                    updateProperty.AppendAdd("/TurbineID", ID.Value<string>());
                    updateProperty.AppendAdd("/TimeInterval", TimeInterval.Value<string>());
                    updateProperty.AppendAdd("/TvStatus", TvStatus.Value<bool>());
                    updateProperty.AppendAdd("/CurrentChannelName", CurrentChannelName.Value<string>());
                    updateProperty.AppendAdd("/CurrentChannelNumber", CurrentChannelNumber.Value<int>());
                    updateProperty.AppendAdd("/VolumeLevel", VolumeLevel.Value<int>());
                    updateProperty.AppendAdd("/VolumeMute", VolumeMute.Value<bool>());
                    updateProperty.AppendAdd("/DisplaySettingsBrightness", DisplaySettingsBrightness.Value<int>());
                    updateProperty.AppendAdd("/DisplaySettingsContrast", DisplaySettingsContrast.Value<int>());
                    updateProperty.AppendAdd("/DisplaySettingsColorTemperature", DisplaySettingsColorTemperature.Value<string>());
                    updateProperty.AppendAdd("/ActiveInput", ActiveInput.Value<string>());
                    updateProperty.AppendAdd("/AudioOutput", AudioOutput.Value<string>());
                    updateProperty.AppendAdd("/NetworkStatusConnected", NetworkStatusConnected.Value<bool>());
                    updateProperty.AppendAdd("/NetworkStatusWifiStrength", NetworkStatusWifiStrength.Value<int>());
                    updateProperty.AppendAdd("/TvComponentsMainBoardTemperature", TvComponentsMainBoardTemperature.Value<double>());
                    updateProperty.AppendAdd("/TvComponentsPowerSupplyVoltage", TvComponentsPowerSupplyVoltage.Value<int>());
                    updateProperty.AppendAdd("/TvComponentsPowerSupplyCurrent", TvComponentsPowerSupplyCurrent.Value<double>());
                    updateProperty.AppendAdd("/TvComponentsDisplayPanelResolution", TvComponentsDisplayPanelResolution.Value<string>());
                    updateProperty.AppendAdd("/TvComponentsDisplayPanelBacklightIntensity", TvComponentsDisplayPanelBacklightIntensity.Value<int>());
                    updateProperty.AppendAdd("/TvComponentsAudioSystemVolume", TvComponentsAudioSystemVolume.Value<int>());
                    updateProperty.AppendAdd("/TvComponentsAudioSystemMute", TvComponentsAudioSystemMute.Value<bool>());
                    updateProperty.AppendAdd("/TvComponentsWifiModuleConnected", TvComponentsWifiModuleConnected.Value<bool>());
                    updateProperty.AppendAdd("/TvComponentsWifiModuleSignalStrength", TvComponentsWifiModuleSignalStrength.Value<int>());
                    updateProperty.AppendAdd("/oxys", oxys.Value<double>());
                    updateProperty.AppendAdd("/ats", ats.Value<double>());
                    updateProperty.AppendAdd("/pressure", pressure.Value<double>());
                    updateProperty.AppendAdd("/cps", cps.Value<double>());
                    updateProperty.AppendAdd("/aps", aps.Value<double>());
                    updateProperty.AppendAdd("/sas", sas.Value<double>());
                    updateProperty.AppendAdd("/vss", vss.Value<double>());
                    updateProperty.AppendAdd("/iat", iat.Value<double>());
                    updateProperty.AppendAdd("/maf", maf.Value<double>());
                    updateProperty.AppendAdd("/ect", ect.Value<double>());
                    log.LogInformation(updateProperty.ToString());
                    try
                    {
                        await client.PublishTelemetryAsync(deviceId, Guid.NewGuid().ToString(), JsonConvert.SerializeObject(turbineTelemetry));
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