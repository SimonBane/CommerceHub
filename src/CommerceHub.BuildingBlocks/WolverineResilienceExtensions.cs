using System.Data.Common;
using System.Net.Sockets;
using JasperFx.CodeGeneration;
using Npgsql;
using Wolverine;
using Wolverine.Configuration;
using Wolverine.ErrorHandling;
using Wolverine.Runtime.Handlers;

namespace CommerceHub.BuildingBlocks;

/// <summary>
/// Wolverine resilience policies: retries with exponential backoff for transient errors.
/// </summary>
public static class WolverineResilienceExtensions
{
    /// <summary>
    /// Applies standard resilience policies to Wolverine message handlers via an IHandlerPolicy:
    /// - Retry with exponential backoff for transient DB/network errors (Npgsql, DbException, Socket, HttpRequest)
    /// - Schedule retry for timeouts
    /// </summary>
    public static WolverineOptions UseResiliencePolicies(this WolverineOptions opts)
    {
        opts.Policies.Add<ResilienceErrorHandlingPolicy>();
        return opts;
    }
}

internal sealed class ResilienceErrorHandlingPolicy : IHandlerPolicy
{
    public void Apply(IReadOnlyList<HandlerChain> chains, GenerationRules rules, JasperFx.IServiceContainer container)
    {
        foreach (var chain in chains)
        {
            chain.OnException<NpgsqlException>()
                .RetryWithCooldown(
                    TimeSpan.FromMilliseconds(50),
                    TimeSpan.FromMilliseconds(100),
                    TimeSpan.FromMilliseconds(250),
                    TimeSpan.FromMilliseconds(500));

            chain.OnException<DbException>()
                .RetryWithCooldown(
                    TimeSpan.FromMilliseconds(50),
                    TimeSpan.FromMilliseconds(100),
                    TimeSpan.FromMilliseconds(250));

            chain.OnException<SocketException>()
                .ScheduleRetry(TimeSpan.FromSeconds(5));

            chain.OnException<HttpRequestException>()
                .ScheduleRetry(TimeSpan.FromSeconds(5));

            chain.OnException<TimeoutException>()
                .ScheduleRetry(TimeSpan.FromSeconds(5));
        }
    }
}
