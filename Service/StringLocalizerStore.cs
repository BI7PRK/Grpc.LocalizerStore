using AspNetCore.Grpc.LocalizerStore.Rpc;
using GoI18n;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using System.Globalization;

namespace AspNetCore.Grpc.LocalizerStore.Service
{

    public interface IStringLocalizerStore
    {
        /// <summary>
        /// 是否可加载
        /// </summary>
        bool IsSuccessed { get; }

        /// <summary>
        /// 获取本地化资源
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        string this[string name] { get; }
        /// <summary>
        /// 获取所有的本地化资源
        /// </summary>
        /// <returns></returns>
        ReadOnlyDictionary<string, string> GetAllStrings();

        /// <summary>
        /// 获取支持的语言
        /// </summary>
        /// <returns></returns>
        Task<CultureItem[]> GetCultures();

        /// <summary>
        /// 获取本地化资源
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        string GetString(string name);

        /// <summary>
        /// /// 重载本地化资源
        /// </summary>
        /// <returns></returns>
        Task ReloadAsync();
    }
    public class StringLocalizerStore : IStringLocalizerStore
    {
        readonly IMemoryCache _memoryCache;
        ReadOnlyDictionary<string, string> _resources;
        readonly ILocalizerChannel _i18NChannel;
        readonly ILogger<StringLocalizerStore> _logger;
        private bool _canLoad;
        public StringLocalizerStore(ILocalizerChannel i18NChannel, ILogger<StringLocalizerStore> logger, IMemoryCache memoryCache)
        {
            _i18NChannel = i18NChannel;
            _logger = logger;
            _memoryCache = memoryCache;
            LoadResource().GetAwaiter().GetResult();
        }

        private async Task LoadResource()
        {
            var code = CultureInfo.CurrentCulture.Name;
            if (_memoryCache.TryGetValue(code, out Dictionary<string, string>? data) && data != null)
            {
                _resources = new ReadOnlyDictionary<string, string>(data);
                return;
            }
            _logger.LogInformation("loading localizer resource ...{code}", code);

            var resources = new Dictionary<string, string>();
            try
            {
                var res = await _i18NChannel.Client.GetCultureResourcesAsync(new CultureCodeRequest
                {
                    Code = code
                });
                if (res.Code == ReplyCode.Success)
                {
                    foreach (var item in res.Items)
                    {
                        resources.Add(item.Key.ToUpper(), item.Text);
                    }
                    _memoryCache.Set(code, resources);
                }
                else
                {
                    _logger.LogError("GetCultureResources failed: {Message}", res.Message);
                }
                _canLoad = true;
            }
            catch
            {
                _canLoad = false;
            }
            _resources = new ReadOnlyDictionary<string, string>(resources);
        }
        /// <summary>
        /// 获取本地化资源
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public string GetString(string name)
        {
            if (_resources.TryGetValue(name.ToUpper(), out var value))
            {
                return value;
            }
            else
            {
                return name.ToUpper();
            }
        }
        /// <summary>
        /// 获取本地化资源
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public string this[string name]
        {
            get
            {
                if (_resources.TryGetValue(name.ToUpper(), out var value))
                {
                    return value;
                }
                else
                {
                    return name.ToUpper();
                }
            }
        }
        /// <summary>
        /// 获取所有的本地化资源
        /// </summary>
        /// <returns></returns>
        public ReadOnlyDictionary<string, string> GetAllStrings()
        {
            return _resources;
        }

        public async Task<CultureItem[]> GetCultures()
        {
            var res = await _i18NChannel.Client.CultureFeatureAsync(new CulturesRequest
            {
                Action = ActionTypes.List
            });
            if (res.Code == ReplyCode.Success)
            {
                return [.. res.Items];
            }
            return [];
        }

        /// <summary>
        /// 重载本地化资源
        /// </summary>
        public Task ReloadAsync()
        {
            _memoryCache.Remove(CultureInfo.CurrentCulture.Name);
            return LoadResource();
        }

        public bool IsSuccessed => _canLoad;
    }

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
            try
            {
                var _scopeFactory = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>();
                using var _newScope = _scopeFactory.CreateScope();

                var localizerStore = _newScope.ServiceProvider.GetService<IStringLocalizerStore>();
                if (localizerStore != null && localizerStore.IsSuccessed)
                {
                    var resources = localizerStore.GetCultures().GetAwaiter().GetResult();
                    if (resources == null || resources.Length == 0)
                    {
                        return app;
                    }
                    var supportedCultures = resources.Select(s => new CultureInfo(s.Code)).ToArray();
                    var defaultCulture = resources.FirstOrDefault(w => w.IsDefault)?.Code ?? CultureInfo.CurrentCulture.Name;
                    app.UseRequestLocalization(new RequestLocalizationOptions
                    {
                        DefaultRequestCulture = new RequestCulture(defaultCulture),
                        SupportedCultures = supportedCultures,
                        SupportedUICultures = supportedCultures
                    });
                }
            }
            catch (Exception ex)
            {
               
            }
           
            return app;
        }
    }

}
