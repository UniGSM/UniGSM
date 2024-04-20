using GsmCore.Models;
using GsmCore.Params;
using GsmCore.Utils;

namespace GsmCore.ApiClient;

public partial class ApiClient
{
    public async Task<HttpResponseMessage> CreateServer(Server server)
    {
        return await Put("server", server);
    }

    public async Task<HttpResponseMessage> DeleteServer(int id)
    {
        return await Delete($"server/{id}");
    }

    public async Task<HttpResponseMessage> RestartServer(int id)
    {
        return await Post<Server>($"server/{id}/restart", null!);
    }

    public async Task<HttpResponseMessage> StartServer(int id)
    {
        return await Post<Server>($"server/{id}/start", null!);
    }

    public async Task<HttpResponseMessage> StopServer(int id)
    {
        return await Post<Server>($"server/{id}/stop", null!);
    }

    public async Task<HttpResponseMessage> UpdateServer(int id)
    {
        return await Post<Server>($"server/{id}/update", null!);
    }

    public async Task<Server?> GetServer(int id)
    {
        return await Get<Server>($"server/{id}");
    }

    public async Task<PagedList<Server>?> GetServers(int page = 1)
    {
        return await GetPaginated<Server>("server", page);
    }

    public async Task<HttpResponseMessage> UpdateNetworking(int id, ServerNetworkingParams networkingParams)
    {
        return await Post($"server/{id}/networking", networkingParams);
    }

    public async Task<HttpResponseMessage> UpdateFlags(int id, ServerFlagParams serverFlagParams)
    {
        return await Post($"server/{id}/flags", serverFlagParams);
    }

    public async Task<HttpResponseMessage> UpdateGameData(int id, ServerGameDataParams serverGameDataParams)
    {
        return await Post($"server/{id}/gamedata", serverGameDataParams);
    }
}