using System.Net.Http.Headers;
using System.Net.Http.Json;
using Newtonsoft.Json;

namespace Ryan.Integration.Salesforce.Client;

public sealed record SalesforceJobsClient
{
    private readonly SalesforceCredentials _credentials;

    public SalesforceJobsClient(SalesforceCredentials credentials)
    {
        _credentials = credentials;
    }

    public async Task<JobStatus> CreateJob(
        string typeName,
        string keyField,
        string contentType = "CSV",
        string operation = "upsert",
        string lineEnding = "CRLF"
    )
    {
        var job = new
        {
            @object = typeName,
            externalIdFieldName = keyField,
            contentType,
            operation,
            lineEnding,
        };

        var uri = $"/services/data/{_credentials.ApiVersion}/jobs/ingest/";
        var httpClient = CreateHttpClient();
        
        var response = await httpClient.PostAsJsonAsync(uri, job);
        response.EnsureSuccessStatusCode();

        var responseText = await response.Content.ReadAsStringAsync();
        var status = JsonConvert.DeserializeObject<JobStatus>(responseText);

        return status;
    }

    public async Task<JobStatus> GetJobStatus(string jobId)
    {
        var uri = $"/services/data/{_credentials.ApiVersion}/jobs/ingest/{jobId}";
        var httpClient = CreateHttpClient();

        var responseText = await httpClient.GetStringAsync(uri);
        var status = JsonConvert.DeserializeObject<JobStatus>(responseText);

        return status;
    }

    public async Task<JobStatus> AbortJob(string jobId)
    {
        var uri = $"/services/data/{_credentials.ApiVersion}/jobs/ingest/{jobId}";
        var httpClient = CreateHttpClient();

        var response = await httpClient.PatchAsJsonAsync(uri, new { state = "Aborted"} );
        response.EnsureSuccessStatusCode();

        var responseText = await response.Content.ReadAsStringAsync();
        var status = JsonConvert.DeserializeObject<JobStatus>(responseText);

        return status;
    }

    public async Task DeleteJob(string jobId)
    {
        var uri = $"/services/data/{_credentials.ApiVersion}/jobs/ingest/{jobId}";
        var httpClient = CreateHttpClient();

        var response = await httpClient.DeleteAsync(uri);
        response.EnsureSuccessStatusCode();
    }

    private HttpClient CreateHttpClient()
    {
        var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _credentials.AccessToken);
        httpClient.BaseAddress = new Uri(_credentials.InstanceUrl);
        return httpClient;
    }
    
    public record JobStatus(
        [JsonProperty("id")] string JobId,
        [JsonProperty("state")] string Status,
        [JsonProperty("jobType")] string JobType,
        [JsonProperty("operation")] string Operation,
        [JsonProperty("object")] string TypeName,
        [JsonProperty("externalIdFieldName")] string KeyField,
        [JsonProperty("contentType")] string ContentType,
        [JsonProperty("columnDelimiter")] string ColumnDelimiter,
        [JsonProperty("lineEnding")] string LineEnding,
        [JsonProperty("createdDate")] DateTimeOffset CreatedDate,
        [JsonProperty("systemModstamp")] DateTimeOffset ModifiedDate);
}

public static class SalesforceJobsClientExtensions
{
    public static async Task AbortAndDeleteJob(this SalesforceJobsClient client, string jobId)
    {
        await client.AbortJob(jobId);
        await client.DeleteJob(jobId);
    }
}