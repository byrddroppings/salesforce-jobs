using System.Net.Http.Json;
using Newtonsoft.Json;

namespace Ryan.Integration.Salesforce.Client;

public static class HttpClientExtensions
{
    public static async Task<HttpResponseMessage> PatchAsJsonAsync<T>(this HttpClient httpClient, string uri, T data)
    {
        //var serialized = JsonConvert.SerializeObject(data);
        var content = JsonContent.Create(data);
        return await httpClient.PatchAsync(uri, content);
    }
}