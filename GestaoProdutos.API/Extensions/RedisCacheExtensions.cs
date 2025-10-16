using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.DependencyInjection;

namespace GestaoProdutos.API.Extensions
{
    /// <summary>
    /// Extensões para configuração de Redis Cache
    /// </summary>
    public static class RedisCacheExtensions
    {
        /// <summary>
        /// Configura Redis Cache com StackExchange.Redis
        /// </summary>
        public static IServiceCollection AddRedisCache(
            this IServiceCollection services, 
            string connectionString, 
            string instanceName = "GestaoProdutos:")
        {
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = connectionString;
                options.InstanceName = instanceName;
            });
            
            return services;
        }
    }
}
