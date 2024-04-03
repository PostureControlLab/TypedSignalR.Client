using System;
using System.Collections.Generic;
using System.Text;
using TypedSignalR.Client.CodeAnalysis;

namespace TypedSignalR.Client.Templates;

public sealed class HubConnectionExtensionsHubInvokerTemplate
{
    private readonly IReadOnlyList<TypeMetadata> _hubTypes;
    private readonly SpecialSymbols _specialSymbols;

    public HubConnectionExtensionsHubInvokerTemplate(IReadOnlyList<TypeMetadata> hubTypes, SpecialSymbols specialSymbols)
    {
        _hubTypes = hubTypes;
        _specialSymbols = specialSymbols;
    }

    public string TransformText()
    {
        var sb = new StringBuilder();

        sb.AppendLine(""""
// <auto-generated>
// THIS (.cs) FILE IS GENERATED BY TypedSignalR.Client
// </auto-generated>
#nullable enable
#pragma warning disable CS1591
#pragma warning disable CS8767
#pragma warning disable CS8613
namespace TypedSignalR.Client
{
    internal static partial class HubConnectionExtensions
    {
"""");

        foreach (var hubType in _hubTypes)
        {
            sb.AppendLine($$"""
        private sealed class HubInvokerFor_{{hubType.CollisionFreeName}} : {{hubType.FullyQualifiedInterfaceName}}, IHubInvoker
        {
            private readonly global::Microsoft.AspNetCore.SignalR.Client.HubConnection _connection;
            private readonly global::System.Threading.CancellationToken _cancellationToken;

            public HubInvokerFor_{{hubType.CollisionFreeName}}(global::Microsoft.AspNetCore.SignalR.Client.HubConnection connection, global::System.Threading.CancellationToken cancellationToken)
            {
                _connection = connection;
                _cancellationToken = cancellationToken;
            }
{{CreateMethodsString(hubType)}}
        }

        private sealed class HubInvokerFactoryFor_{{hubType.CollisionFreeName}} : IHubInvokerFactory<{{hubType.FullyQualifiedInterfaceName}}>
        {
            public {{hubType.FullyQualifiedInterfaceName}} CreateHubInvoker(global::Microsoft.AspNetCore.SignalR.Client.HubConnection connection, global::System.Threading.CancellationToken cancellationToken)
            {
                return new HubInvokerFor_{{hubType.CollisionFreeName}}(connection, cancellationToken);
            }
        }

""");
        }

        sb.AppendLine("""
        private static partial global::System.Collections.Generic.Dictionary<global::System.Type, IHubInvokerFactory> CreateFactories()
        {
            var factories = new global::System.Collections.Generic.Dictionary<global::System.Type, IHubInvokerFactory>();

""");


        foreach (var hubType in _hubTypes)
        {
            sb.AppendLine($$"""
            factories.Add(typeof({{hubType.FullyQualifiedInterfaceName}}), new HubInvokerFactoryFor_{{hubType.CollisionFreeName}}());
""");
        }

        sb.AppendLine("""

            return factories;
        }
    }
}
#pragma warning restore CS8613
#pragma warning restore CS8767
#pragma warning restore CS1591
""");

        return sb.ToString();
    }

    private string CreateMethodsString(TypeMetadata hubType)
    {
        if (hubType.Methods.Count == 0)
        {
            return string.Empty;
        }

        if (hubType.Methods.Count == 1)
        {
            return $"\n{hubType.Methods[0].CreateMethodString(_specialSymbols)}";
        }

        var sb = new StringBuilder();

        for (int i = 0; i < hubType.Methods.Count - 1; i++)
        {
            sb.AppendLine();
            sb.AppendLine(hubType.Methods[i].CreateMethodString(_specialSymbols));
        }

        sb.AppendLine();
        sb.Append(hubType.Methods[hubType.Methods.Count - 1].CreateMethodString(_specialSymbols));

        return sb.ToString();
    }
}
