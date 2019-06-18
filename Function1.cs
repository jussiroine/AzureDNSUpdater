using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Rest.Azure.Authentication;
using Microsoft.Azure.Management.Dns;
using Microsoft.Azure.Management.Dns.Models;
using System.Collections.Generic;

namespace DNSUpdater
{
    public static class DNSUpdater
    {
        [FunctionName("DNSUpdater")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("IP Update requested");

            string remoteip = req.Query["remoteip"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            remoteip = remoteip ?? data?.remoteip;

            string dnsZone = "domain.com";
            string hostname = "vpn";
            string resourceGroupName = "DNS";

            string appID = "appid";
            string appSecret = "appsecret";
            string subscriptionID = "subscriptionid";
            string tenantID = "tenantid";

            // Build the service credentials and DNS management client
            Microsoft.Rest.ServiceClientCredentials serviceCreds = await ApplicationTokenProvider.LoginSilentAsync(tenantID, appID, appSecret);
            DnsManagementClient dnsClient = new DnsManagementClient(serviceCreds)
            {
                SubscriptionId = subscriptionID
            };

            var recordSet = dnsClient.RecordSets.Get(resourceGroupName, dnsZone, hostname, RecordType.A);

            // clean up older records
            if (recordSet.ARecords.Count > 0)
            {
                recordSet.ARecords.Clear(); 
            }
            
            recordSet.ARecords.Add(new ARecord(remoteip));

            // Update the record set in Azure DNS
            // Note: ETAG check specified, update will be rejected if the record set has changed in the meantime
            recordSet = await dnsClient.RecordSets.CreateOrUpdateAsync(resourceGroupName, dnsZone, hostname, RecordType.A, recordSet, recordSet.Etag);

            // update TXT 
            recordSet = dnsClient.RecordSets.Get(resourceGroupName, dnsZone, hostname, RecordType.TXT);
                        
            recordSet.TxtRecords[0].Value.Clear(); 
            recordSet.TxtRecords[0].Value.Add(DateTime.Now.ToString());

            // Update the record set in Azure DNS
            recordSet = await dnsClient.RecordSets.CreateOrUpdateAsync(resourceGroupName, dnsZone, hostname, RecordType.TXT, recordSet, recordSet.Etag);
            
            return remoteip != null
                ? (ActionResult)new OkObjectResult($"OK")
                : new BadRequestObjectResult("Unable to update IP");
        }
    }
}
