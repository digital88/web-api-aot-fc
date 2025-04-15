using System.Net;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using ZiggyCreatures.Caching.Fusion.Backplane.StackExchangeRedis;

namespace Test.Api.Models;

public sealed class RedisCacheConfigurationOptions
{
    public string?[]? EndPoints { get; set; }
    public string? Password { get; set; }
    public bool Ssl { get; set; }

    public IList<EndPoint> GetDnsEndPoints()
    {
        return EndPoints?
            .Select(e => new HostPort(e))
            .Select(hp => new DnsEndPoint(hp.Host, hp.Port))
            .Cast<EndPoint>()
            .ToList() ?? [];
    }

    internal readonly struct HostPort
    {
        public readonly string Host { get; }
        public readonly int Port { get; }

        public HostPort(string? hostPortString)
        {
            Host = string.Empty;
            if (string.IsNullOrWhiteSpace(hostPortString)) return;
            var hostPortStringSplit = hostPortString.Split(":", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (hostPortStringSplit.Length != 2) return;
            Host = hostPortStringSplit[0];
            if (!int.TryParse(hostPortStringSplit[1], out var port)) return;
            Port = port;
        }
    }
}

public static class RedisCacheConfigurationOptionsExtensions
{
    public static RedisCacheOptions AsRedisCacheOptions(this RedisCacheConfigurationOptions opts)
    {
        return new RedisCacheOptions
        {
            ConfigurationOptions = new()
            {
                Ssl = opts.Ssl,
                Password = opts.Password,
                EndPoints = [.. opts.GetDnsEndPoints()]
            }
        };
    }

    public static RedisBackplaneOptions AsRedisBackplaneOptions(this RedisCacheConfigurationOptions opts)
    {
        return new RedisBackplaneOptions
        {
            ConfigurationOptions = new()
            {
                Ssl = opts.Ssl,
                Password = opts.Password,
                EndPoints = [.. opts.GetDnsEndPoints()]
            }
        };
    }
}