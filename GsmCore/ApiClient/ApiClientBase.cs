using System.Net.Http.Json;
using GsmCore.Util;

namespace GsmCore.ApiClient;

public class ApiClientBase(string baseUrl, string userName, string password)
{
    private TokenResponse? _token;

    public async Task<PagedList<T>?> GetPaginated<T>(string url, int pageNumber = 1)
    {
        using var client = await SetTokenAndGetClient();
        return await client.GetFromJsonAsync<PagedList<T>>($"{baseUrl}/{url}?PageNumber={pageNumber}");
    }

    public async Task<HttpResponseMessage> Delete(string url)
    {
        using var client = await SetTokenAndGetClient();
        return await client.DeleteAsync($"{baseUrl}/{url}");
    }

    public async Task<HttpResponseMessage> Put<T>(string url, T data)
    {
        using var client = await SetTokenAndGetClient();
        return await client.PutAsJsonAsync($"{baseUrl}/{url}", data);
    }

    public async Task<HttpResponseMessage> Post<T>(string url, T data)
    {
        using var client = await SetTokenAndGetClient();
        return await client.PostAsJsonAsync($"{baseUrl}/{url}", data);
    }

    public async Task<T?> Get<T>(string url)
    {
        using var client = await SetTokenAndGetClient();
        return await client.GetFromJsonAsync<T>($"{baseUrl}/{url}");
    }

    private async Task<HttpClient> SetTokenAndGetClient()
    {
        var client = new HttpClient();
        if (_token != null && _token.Expiration > DateTime.Now)
        {
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_token.Token}");
            return client;
        }

        var response = await client.PostAsJsonAsync($"{baseUrl}/api/v1/token", new TokenModel
        {
            UserName = userName,
            Password = password
        });

        _token = await response.Content.ReadFromJsonAsync<TokenResponse>() ??
                 throw new FileNotFoundException("Failed to get token");

        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_token.Token}");
        return client;
    }
}