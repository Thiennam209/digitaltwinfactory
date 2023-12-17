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
        public static int Part;
        public static string Station;
        public static int Serial;
        public static string AdjJudge;
        public static double Pressure;
        public static double IP1;
        public static string CrimpJudge;
        public static string PerformJudge;
        public static double I1;
        public static double I2;
        public static double I3;
        public static double I4;
        public static double I1I15;
        public static double I2I14;
        public static double I3I13;
        public static double Stick1;
        public static double Stick3;
        public static double Flow;
        public static double Resp1_P5;
        public static double Resp1_P6;
        public static double Resp2_P5;
        public static double Resp2_P6;
        public static bool Alert;

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

                    log.LogInformation($"setting Alert to: {Alert}");
                    var property = new Dictionary<object, object>
                    {
                        {"MachineId", MachineId },
                        {"Alert",Alert}
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