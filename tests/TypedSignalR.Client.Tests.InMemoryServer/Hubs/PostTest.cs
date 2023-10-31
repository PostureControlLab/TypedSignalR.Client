using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.SignalR.Client;
using TypedSignalR.Client.Tests.Shared;

namespace TypedSignalR.Client.Tests.InMemoryServer.Hubs;

public class PostTest : IntegrationTestBase, IAsyncLifetime
{
    private readonly HubConnection _connection;
    private readonly ISideEffectHub _sideEffectHub;
    private readonly CancellationTokenSource _cancellationTokenSource = new();

    public PostTest()
    {
        var client = CreateClient();
        var handler = CreateHandler();

        _connection = new HubConnectionBuilder()
            .WithUrl(new Uri(client.BaseAddress!, "/Hubs/SideEffectHub"), options =>
            {
                options.Transports = HttpTransportType.ServerSentEvents | HttpTransportType.LongPolling;
                options.HttpMessageHandlerFactory = _ => handler;
            })
            .WithAutomaticReconnect()
            .Build();

        _sideEffectHub = _connection.CreateHubProxy<ISideEffectHub>(_cancellationTokenSource.Token);
    }

    public async Task InitializeAsync()
    {
        await _connection.StartAsync(_cancellationTokenSource.Token);
    }

    public async Task DisposeAsync()
    {
        try
        {
            await _connection.StopAsync(_cancellationTokenSource.Token);
        }
        finally
        {
            _cancellationTokenSource.Cancel();
        }
    }
    [Fact]
    public async Task NoParameter()
    {
        await _sideEffectHub.Init(); // 0
        await _sideEffectHub.Increment(); // 1
        await _sideEffectHub.Increment(); // 2
        await _sideEffectHub.Increment(); // 3
        await _sideEffectHub.Increment(); // 4

        var result = await _sideEffectHub.Result();

        Assert.Equal(4, result);
    }

    [Fact]
    public async Task PostParameter()
    {
        List<UserDefinedType> list = new();

        for (int i = 0; i < 10; i++)
        {
            var instance = new UserDefinedType()
            {
                Guid = Guid.NewGuid(),
                DateTime = DateTime.Now,
            };
            list.Add(instance);
            await _sideEffectHub.Post(instance);
        }

        var data = await _sideEffectHub.Fetch();

        for (int i = 0; i < data.Length; i++)
        {
            Assert.Equal(data[i].DateTime, list[i].DateTime);
            Assert.Equal(data[i].Guid, list[i].Guid);
        }
    }
}
