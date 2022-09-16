using System.Collections;
using System.Globalization;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using CsvHelper;
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

    public async Task UploadJobBatch<T>(string jobId, T data)
        where T : IEnumerable
    {
        var csv = await GetCsvData(data);
        await UploadJobBatch(jobId, csv);
    }

    public async Task UploadJobBatch(string jobId, string csv)
    {
        var uri = $"/services/data/{_credentials.ApiVersion}/jobs/ingest/{jobId}/batches/";
        var content = new StringContent(csv, Encoding.Default, "text/csv");
        var httpClient = CreateHttpClient();

        var response = await httpClient.PutAsync(uri, content);
        response.EnsureSuccessStatusCode();
    }

    public async Task<JobStatus> StartJob(string jobId)
        => await SetJobState(jobId, "UploadComplete");
    
    public async Task<JobStatus> GetJobStatus(string jobId)
    {
        var uri = $"/services/data/{_credentials.ApiVersion}/jobs/ingest/{jobId}";
        var httpClient = CreateHttpClient();

        var responseText = await httpClient.GetStringAsync(uri);
        var status = JsonConvert.DeserializeObject<JobStatus>(responseText);

        return status;
    }

    public async Task<JobStatus> AbortJob(string jobId)
        => await SetJobState(jobId, "Aborted");

    public async Task DeleteJob(string jobId)
    {
        var uri = $"/services/data/{_credentials.ApiVersion}/jobs/ingest/{jobId}";
        var httpClient = CreateHttpClient();

        var response = await httpClient.DeleteAsync(uri);
        response.EnsureSuccessStatusCode();
    }

    private async Task<JobStatus> SetJobState(string jobId, string state)
    {
        var uri = $"/services/data/{_credentials.ApiVersion}/jobs/ingest/{jobId}";
        var httpClient = CreateHttpClient();

        var response = await httpClient.PatchAsJsonAsync(uri, new { state } );
        response.EnsureSuccessStatusCode();

        var responseText = await response.Content.ReadAsStringAsync();
        var status = JsonConvert.DeserializeObject<JobStatus>(responseText);

        return status;
    }

    private HttpClient CreateHttpClient()
    {
        var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _credentials.AccessToken);
        httpClient.BaseAddress = new Uri(_credentials.InstanceUrl);
        return httpClient;
    }

    private async Task<string> GetCsvData<T>(T data)
        where T : IEnumerable
    {
        var stringBuilder = new StringBuilder();
        await using var stringWriter = new StringWriter(stringBuilder);
        await using var csvWriter = new CsvWriter(stringWriter, CultureInfo.InvariantCulture);
        await csvWriter.WriteRecordsAsync(data);
        return stringBuilder.ToString();
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
    public static async Task<SalesforceJobsClient.JobStatus> RunJob(
        this SalesforceJobsClient client,
        string typeName,
        string keyField,
        T data,
        string contentType = "CSV",
        string operation = "upsert",
        string lineEnding = "CRLF"
    )
    {
        
    }
    
    public static async Task AbortAndDeleteJob(this SalesforceJobsClient client, string jobId)
    {
        await client.AbortJob(jobId);
        await client.DeleteJob(jobId);
    }
}