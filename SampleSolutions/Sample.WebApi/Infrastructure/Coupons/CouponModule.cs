using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Sample.WebApi.Application.Coupons;
using StackExchange.Redis;

namespace Sample.WebApi.Infrastructure.Coupons;

public static class CouponModule
{
    public static IServiceCollection AddCouponModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<CouponOptions>(configuration.GetSection("Coupons"));

        services.AddSingleton<IConnectionMultiplexer>(provider =>
        {
            var connectionString = configuration.GetConnectionString("Redis");
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException("Redis 연결 문자열이 설정되지 않았습니다. appsettings.json 파일을 확인하세요.");
            }

            var options = ConfigurationOptions.Parse(connectionString, true);
            options.AbortOnConnectFail = false;

            return ConnectionMultiplexer.Connect(options);
        });

        services.AddSingleton<ICouponIssuanceService, RedisCouponIssuanceService>();
        services.AddHostedService<CouponStockInitializer>();

        return services;
    }
}
