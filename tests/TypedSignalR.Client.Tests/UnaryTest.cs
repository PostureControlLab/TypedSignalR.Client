using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using TypedSignalR.Client.Tests.Shared;
using Xunit;

namespace TypedSignalR.Client.Tests;

// Lunch TypedSignalR.Client.Tests.Server.csproj before test!
public class UnaryTest : IAsyncLifetime
{
    private readonly HubConnection _connection;
    private readonly IUnaryHub _unaryHub;
    private readonly CancellationTokenSource _cancellationTokenSource = new();

    public UnaryTest()
    {
        _connection = new HubConnectionBuilder()
            .WithUrl("https://localhost:7105/Hubs/UnaryHub")
            .Build();

        _unaryHub = _connection.CreateHubProxy<IUnaryHub>(_cancellationTokenSource.Token);
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

    /// <summary>
    /// no parameter test
    /// </summary>
    /// <returns></returns>
    [Fact]
    public async Task Get()
    {
        var str = await _unaryHub.Get();
        Assert.True(str == "TypedSignalR.Client");
    }
    [Fact]
    public async Task Add()
    {
        var x = Random.Shared.Next();
        var y = Random.Shared.Next();

        var added = await _unaryHub.Add(x, y);

        Assert.True(added == (x + y));
    }

    [Fact]
    public async Task Cat()
    {
        var x = "revue";
        var y = "starlight";

        var cat = await _unaryHub.Cat(x, y);

        Assert.True(cat == (x + y));
    }

    /// <summary>
    /// User defined type test
    /// </summary>
    /// <returns></returns>
    [Fact]
    public async Task Echo()
    {
        var instance = new UserDefinedType()
        {
            Guid = Guid.NewGuid(),
            DateTime = DateTime.Now,
        };

        var ret = await _unaryHub.Echo(instance);

        Assert.True(ret.DateTime == instance.DateTime);
        Assert.True(ret.Guid == instance.Guid);
    }
}
