using Microsoft.Extensions.Configuration;
using Ryan.Integration.Salesforce.Client;

var config = new ConfigurationBuilder()
    .AddAzureAppConfiguration("Endpoint=https://dxp-stg-config.azconfig.io;Id=kSod-l5-s0:g8tLxqfLC8zR+98YXSCj;Secret=vjQUzwZbkzoC0UWjFEbwDOVNR6hHLDkjY4WnbxLxPpk=")
    .Build();
    
var salesforceConfig = new SalesforceConfigurationBuilder()
    .WithConfiguration(config)
    .Build();
    
var authenticationClient = new SalesforceAuthenticationClient(salesforceConfig);
var credentials = await authenticationClient.AuthenticateAsync();
var client = new SalesforceJobsClient(credentials);

var createStatus = await client.CreateJob("Revenue__c", "UniqueKey__c");
Console.WriteLine(createStatus);

var status = await client.GetJobStatus(createStatus.JobId);
Console.WriteLine(status);

await client.AbortAndDeleteJob(createStatus.JobId);
Console.WriteLine("job deleted");