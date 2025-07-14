using AspNetCore.Grpc.LocalizerStore.Rpc;
using AspNetCore.Grpc.LocalizerStore.Service;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using System.Globalization;

namespace AspNetCore.Grpc.LocalizerStore
{

    public static class StringLocalizerStoreExtensions
    {
        /// <summary>
        /// 添加本地化资源服务
        /// </summary>
        /// <param name="services"></param>
        /// <param name="setupAction"></param>
        /// <returns></returns>
        public static IServiceCollection AddLocalizerStore(this IServiceCollection services, Action<LocalizerStoreOption> setupAction)
        {
            var options = new LocalizerStoreOption();
            setupAction(options);
            if (string.IsNullOrWhiteSpace(options.Url))
            {
                return services;
            }
            services.TryAddSingleton(options);
            services.TryAddSingleton<GrpcErrorInterceptor>();
            services.TryAddSingleton<ILocalizerChannel, LocalizerChannel>();
            services.TryAddSingleton<IMemoryCache, MemoryCache>();
            services.TryAddScoped<IStringLocalizerStore, StringLocalizerStore>();
            if (options.AllowManage)
            {
                services.TryAddSingleton<ICultureLocalizerService, CultureLocalizerService>();
            }
            return services;
        }

        /// <summary>
        /// 添加本地化资源服务，以便在请求中使用
        /// </summary>
        /// <param name="app"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseRequestLocalizatioStore(this IApplicationBuilder app)
        {
            var _scopeFactory = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>();
            using var _newScope = _scopeFactory.CreateScope();
            var logger = _newScope.ServiceProvider.GetRequiredService<ILogger<StringLocalizerStore>>();
            try
            {
                var localizerStore = _newScope.ServiceProvider.GetService<IStringLocalizerStore>();
                var _option = _newScope.ServiceProvider.GetService<LocalizerStoreOption>();
                if (localizerStore != null)
                {
                    var resources = localizerStore.GetCultures().GetAwaiter().GetResult();
                    if (resources == null || resources.Length == 0)
                    {
                        logger.LogWarning("No localization resources found.");
                        return app;
                    }
                    var supportedCultures = resources.Select(s => new CultureInfo(s.Code)).ToArray();
                    var defaultCulture = _option?.DefaultCulture;
                    if (string.IsNullOrWhiteSpace(defaultCulture))
                    {
                        defaultCulture = (resources.FirstOrDefault(w => w.IsDefault) ?? resources.First()).Code;
                    }
                    app.UseRequestLocalization(new RequestLocalizationOptions
                    {
                        DefaultRequestCulture = new RequestCulture(defaultCulture),
                        SupportedCultures = supportedCultures,
                        SupportedUICultures = supportedCultures
                    });
                }
            }
            catch(Exception ex)
            {
                logger.LogWarning("Failed to load localization resources: {Message}", ex.Message);
            }

            return app;
        }
    }
}
