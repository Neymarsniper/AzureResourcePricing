using azureresourcepricing.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Nancy.Json;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Net;
using Azure.Core;
using Azure.Identity;
using static System.Net.WebRequestMethods;
using System;
//using Azure.ResourceManager.Resources.Models;
//using Azure.ResourceManager.Resources;

namespace azureresourcepricing.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        
        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {

            //TokenCredential cred = new DefaultAzureCredential(true);
            //// authenticate your client
            //ArmClient client = new ArmClient(cred);

            //// this example assumes you already have this TenantResource created on azure
            //// for more information of creating TenantResource, please refer to the document of TenantResource
            //var tenant = client.GetTenants().GetAllAsync().GetAsyncEnumerator().Current;

            //// get the collection of this SubscriptionResource
            //SubscriptionCollection collection = tenant.GetSubscriptions();

            //// invoke the operation and iterate over the result
            //await foreach (SubscriptionResource item in collection.GetAllAsync())
            //{
            //    // the variable item is a resource, you could call other operations on this instance as well
            //    // but just for demo, we get its data from this resource instance
            //    SubscriptionData resourceData = item.Data;
            //    // for demo we just print out the id
            //    Console.WriteLine($"Succeeded on id: {resourceData.Id}");
            //}

            
            
            return View();
        }

       
        
        public void GetAuthorizationToken()
        {
            ClientCredential cc = new ClientCredential(AzureDetails.ClientID, AzureDetails.ClientSecret);
            var context = new AuthenticationContext("https://login.microsoftonline.com/" + AzureDetails.TenantID);
            var result = context.AcquireTokenAsync("https://management.azure.com/", cc);
            if (result == null)
            {
                throw new InvalidOperationException("Failed to obtain the Access token");
            }
            AzureDetails.AccessToken = result.Result.AccessToken;
        }

        [HttpGet("GetTenantSubscription")]
        public SubscriptionListResult GetTenantSubscription(string TenantId, string Client_Id, String Client_Secret)
        {
            GetAuthorizationToken();
            var access_token = AzureDetails.AccessToken;
            var subscriptions = GetSubscriptions(access_token);
            return subscriptions;
        }

        public SubscriptionListResult GetSubscriptions(string token)
        {
            var httpClient = new HttpClient();
            //{
            //    BaseAddress = new Uri("https://management.azure.com/subscriptions")
            //};

            string URI = "https://management.azure.com/subscriptions?api-version=2022-12-01";

            //httpClient.DefaultRequestHeaders.Remove("Authorization");
            httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
            HttpResponseMessage response = new HttpResponseMessage();
            try
            {

                response = httpClient.GetAsync(URI).Result;
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var HttpsResponse = response.Content.ReadAsStringAsync().Result;
                    var SubscriptionList = JsonConvert.DeserializeObject<SubscriptionListResult>(HttpsResponse);
                    return SubscriptionList;
                    
                }
                else
                {
                    return null;
                    //Debug.LogError($"Could not retrieve stuff {id}");
                }
               
            }
            catch(Exception ex )
            {

                var exception = ex;
            }

            //var JSONObj = new JavaScriptSerializer().Deserialize<Dictionary<string, string>>(JSONObject);
            return null;

        }


        [HttpGet("GetResourceGroups")]
        public ResourceGroupListResult GetResourceGroups(string subscriptionId)
        {
            var access_token = AzureDetails.AccessToken;
            var resourceGroups = GetResourceGroups(access_token, subscriptionId);
            return resourceGroups;
        }
        public ResourceGroupListResult GetResourceGroups(string token, string subscriptionId)
        {
            var httpClient = new HttpClient();

            string URI = "https://management.azure.com/subscriptions/"+subscriptionId+"/resourceGroups?api-version=2020-09-01";

            httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);

            HttpResponseMessage response = new HttpResponseMessage();

            try
            {
                response = httpClient.GetAsync(URI).Result;

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var responseBody = response.Content.ReadAsStringAsync().Result;
                    var resourceGroups = JsonConvert.DeserializeObject<ResourceGroupListResult>(responseBody);
                    return resourceGroups;
                }
                else
                {
                    // Handle error cases or return an empty result as needed
                    return null;
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions and errors
                return null;
            }

        }


        [HttpGet("GetVirtualMachines")]
        public VMListResult GetVirtualMachines(string resourceGroups)
        {
            var access_token = AzureDetails.AccessToken;
            var VMs = GetVirtualMachines(resourceGroups, access_token);
            return VMs;
        }
        public VMListResult GetVirtualMachines(string resourceGroups, string token)
        {
            var httpClient = new HttpClient();

            string URI = "https://management.azure.com" + resourceGroups + "/resources?$filter=resourceType eq 'Microsoft.Compute/virtualMachines'&api-version=2021-04-01";
            //took me hours to figure this out sir, that i have to delete some extra part of URL by comparing it with postman URL.
            //string URI = "https://management.azure.com/subscriptions/{{subscriptionId}}/resourceGroups/{{resourceGroups}}/resources?$filter=resourceType eq 'Microsoft.Compute/virtualMachines'&api-version=2021-04-01";

            httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);

            HttpResponseMessage response = new HttpResponseMessage();

            try
            {
                response = httpClient.GetAsync(URI).Result;

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var responseBody = response.Content.ReadAsStringAsync().Result;
                    var VMs = JsonConvert.DeserializeObject<VMListResult>(responseBody);
                    return VMs;
                }
                else
                {
                    // Handle error cases or return an empty result as needed
                    return null;
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions and errors
                return null;
            }

        }


        [HttpGet("GetAvailableVirtualMachines")]
        public AvailableVMListResult GetAvailableVirtualMachines(string subscriptionId)
        {
            var access_token = AzureDetails.AccessToken;
            var AvVMs = GetAvailableVirtualMachines(subscriptionId, access_token);
            return AvVMs;
        }
        public AvailableVMListResult GetAvailableVirtualMachines(string subscriptionId, string access_token)
        {
            var httpClient = new HttpClient();

            string URI = "https://management.azure.com/subscriptions/" + subscriptionId + "/providers/Microsoft.Compute/locations/eastus/vmSizes?api-version=2023-07-01";

            httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + access_token);

            HttpResponseMessage response = new HttpResponseMessage();

            try
            {
                response = httpClient.GetAsync(URI).Result;
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var responseBody = response.Content.ReadAsStringAsync().Result;
                    var AvVMs = JsonConvert.DeserializeObject<AvailableVMListResult>(responseBody);
                    return AvVMs;
                }
                else
                {
                    // Handle error cases or return an empty result as needed
                    return null;
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions and errors
                return null;
            }

        }



        [HttpGet("GetVirtualMachinesTableData")]
        public VMListData GetVirtualMachinesTableData(string subscriptionId, string resourceGroups, string selectedVirtualMachine)
        {
            var access_token = AzureDetails.AccessToken;
            var TableVMs = GetVirtualMachinesTableData(subscriptionId, resourceGroups, selectedVirtualMachine, access_token);
            return TableVMs;
        }
        public VMListData GetVirtualMachinesTableData(string subscriptionId, string resourceGroups, string selectedVirtualMachine, string access_token)
        {
            var httpClient = new HttpClient();

            string URI = "https://prices.azure.com/api/retail/prices?$filter=serviceName eq 'Virtual Machines' and armSkuName eq 'Standard_B1s' and armRegionName eq 'eastus'";

            httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + access_token);

            HttpResponseMessage response = new HttpResponseMessage();

            try
            {
                response = httpClient.GetAsync(URI).Result;
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var responseBody = response.Content.ReadAsStringAsync().Result;
                    var TableVMs = JsonConvert.DeserializeObject<VMListData>(responseBody);
                    return TableVMs;
                    
                }
                else
                {
                    // Handle error cases or return an empty result as needed
                    return null;
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions and errors
                return null;
            }

        }




        [HttpGet("GetAvailableVirtualMachinesTableData")]
        public VMListData GetAvailableVirtualMachinesTableData(string selectedAvailableVirtualMachine)
        {
            var access_token = AzureDetails.AccessToken;
            var TableVMs = GetAvailableVirtualMachinesTableData(selectedAvailableVirtualMachine, access_token);
            return TableVMs;
        }
        public VMListData GetAvailableVirtualMachinesTableData(string selectedAvailableVirtualMachine, string access_token)
        {
            var httpClient = new HttpClient();

            string URI = "https://prices.azure.com/api/retail/prices?$filter=serviceName eq 'Virtual Machines' and armSkuName eq '"+ selectedAvailableVirtualMachine + "' and armRegionName eq 'eastus'";

            httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + access_token);

            HttpResponseMessage response = new HttpResponseMessage();

            try
            {
                response = httpClient.GetAsync(URI).Result;
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var responseBody = response.Content.ReadAsStringAsync().Result;
                    var TableVMs = JsonConvert.DeserializeObject<VMListData>(responseBody);
                    return TableVMs;

                }
                else
                {
                    // Handle error cases or return an empty result as needed
                    return null;
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions and errors
                return null;
            }

        }


        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }


    }

    public class AzureDetails
    {
        public static string ClientID = "cc4ca5c1-7cb4-4a56-b404-a02f73f91b47";
        public static string ClientSecret = "0vu8Q~oV-hkcJaRLjj8i6Z5bz5s9siflhYoIDbQw";
        public static string TenantID = "ccfb5208-1b75-4992-bb53-30f7b9144fcd";
        public static string AccessToken { get; set; }
    }

    /////////////////////////////////////////////
    ///
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    public class Count
    {
        public string type { get; set; }
        public int value { get; set; }
    }

    public class Promotion
    {
        public string category { get; set; }
        public DateTime endDateTime { get; set; }
    }

    public class SubscriptionListResult
    {
        public List<Value> value { get; set; }
        public Count count { get; set; }
    }

    public class ResourceGroupListResult
    {
        public List<ResValue> value { get; set; }
        public Count count { get; set; }
    }

    public class VMListResult
    {
        public List<ResValue> value { get; set; }
        public Count count { get; set; }
    }

    public class VMListData
    {
        public string BillingCurrency { get; set; }
        public string CustomerEntityId { get; set; }
        public string CustomerEntityType { get; set; }  
        public List<VMData> Items { get; set; }
        public string? NextPageLink { get; set; }
        public int Count { get; set; }
    }

    public class AvailableVMListResult
    {
        public List<VMValue> value { get; set; }
        public Count count { get; set; }
    }

    public class SubscriptionPolicies
    {
        public string locationPlacementId { get; set; }
        public string quotaId { get; set; }
        public string spendingLimit { get; set; }
    }

    public class Value
    {
        public string id { get; set; }
        public string authorizationSource { get; set; }
        public List<object> managedByTenants { get; set; }
        public string subscriptionId { get; set; }
        public string tenantId { get; set; }
        public string displayName { get; set; }
        public string state { get; set; }
        public SubscriptionPolicies subscriptionPolicies { get; set; }
        public List<Promotion> promotions { get; set; }
    }

    public class ResValue
    {
        public string id { get; set; }
        public string name { get; set; }
        public string type  { get; set; }
        public string location { get; set; }
    }

    public class VMValue
    {
        public string name { get; set; }
        public int numberOfCores { get; set; }
        public int osDiskSizeInMB { get; set; }
        public int resourceDiskSizeInMB { get; set; }
        public int memoryInMB { get; set; }
        public int maxDataDiskCount { get; set; }
    }

    public class VMData
    {
        public string currencyCode { get; set; }
        public float tierMinimumUnits { get; set; }
        public string? reservationTerm { get; set; }
        public float retailPrice { get; set; }
        public float unitPrice { get; set; }
        public string armRegionName { get; set; }
        public string location { get; set; }
        public DateTime effectiveStartDate { get; set; }
        public string meterId { get; set; }
        public string meterName { get; set; }
        public string productId { get; set; }
        public string skuId { get; set; }
        public string productName { get; set; }
        public string skuName { get; set; }
        public string serviceName { get; set; }
        public string serviceId { get; set; }
        public string serviceFamily { get; set; }
        public string unitOfMeasure { get; set; }
        public string type { get; set; }
        public bool isPrimaryMeterRegion { get; set; }
        public string armSkuName { get; set; }
    }

}