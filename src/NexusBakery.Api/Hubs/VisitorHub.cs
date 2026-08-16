using Microsoft.AspNetCore.SignalR;
using NexusBakery.Domain.Interfaces;

namespace NexusBakery.Api.Hubs;

public interface IVisitorClient
{
    Task ReceiveVisitorUpdate(int liveVisitorCount, long lifetimeVisits);
}

public class VisitorHub : Hub<IVisitorClient>
{
    private readonly IMetricsRepository _metricsRepository;
    private static int _connectedCount = 1;
    private static readonly object _lock = new();

    public VisitorHub(IMetricsRepository metricsRepository)
    {
        _metricsRepository = metricsRepository;
    }

    public override async Task OnConnectedAsync()
    {
        lock (_lock)
        {
            _connectedCount++;
        }

        await _metricsRepository.IncrementLiveUsersAsync(1);
        await _metricsRepository.IncrementLifetimeVisitsAsync();

        var metrics = await _metricsRepository.GetGlobalMetricsAsync();
        await Clients.All.ReceiveVisitorUpdate(Math.Max(_connectedCount, metrics.ActiveLiveUsers), metrics.TotalLifetimeVisits);

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        lock (_lock)
        {
            _connectedCount = Math.Max(1, _connectedCount - 1);
        }

        await _metricsRepository.IncrementLiveUsersAsync(-1);
        var metrics = await _metricsRepository.GetGlobalMetricsAsync();
        await Clients.All.ReceiveVisitorUpdate(Math.Max(_connectedCount, metrics.ActiveLiveUsers), metrics.TotalLifetimeVisits);

        await base.OnDisconnectedAsync(exception);
    }

    public async Task GetCurrentCount()
    {
        var metrics = await _metricsRepository.GetGlobalMetricsAsync();
        await Clients.Caller.ReceiveVisitorUpdate(Math.Max(_connectedCount, metrics.ActiveLiveUsers), metrics.TotalLifetimeVisits);
    }
}
