using AspNetCore.Grpc.LocalizerStore.Rpc;
using GoI18n;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;

namespace AspNetCore.Grpc.LocalizerStore.Service
{

    public interface IStringLocalizerStore
    {
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
        Task<CultureItem[]> GetCultures();

        /// <summary>
        /// 获取本地化资源
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        string GetString(string name);
        void Reload(string culture);
        void ReloadAll();
    }
    public class StringLocalizerStore : IStringLocalizerStore
    {
        readonly Dictionary<string, ReadOnlyDictionary<string, string>> _localizerCache = new Dictionary<string, ReadOnlyDictionary<string, string>>();
        readonly ReadOnlyDictionary<string, string> _resources;
        readonly ILocalizerChannel _i18NChannel;
        public StringLocalizerStore(ILocalizerChannel i18NChannel, ILogger<StringLocalizerStore> logger)
        {
            _i18NChannel = i18NChannel;
            var code = CultureInfo.CurrentCulture.Name;
            logger.LogInformation("code: {0}", code);
            if (_localizerCache.TryGetValue(code, out var localizer))
            {
                _resources = localizer;
                return;
            }
            var resources = new Dictionary<string, string>();
            try
            {
                var res = i18NChannel.Client.GetCultureResources(new CultureCodeRequest
                {
                    Code = code
                });
                if (res.Code == ReplyCode.Success)
                {
                    foreach (var item in res.Items)
                    {
                        resources.Add(item.Key, item.Text);
                    }
                    _localizerCache[code] = new ReadOnlyDictionary<string, string>(resources);
                }
                else
                {
                    logger.LogError("GetCultureResources failed: {0}", res.Message);
                }
            }
            catch { }
            _resources = new ReadOnlyDictionary<string, string>(resources); ;
        }
        /// <summary>
        /// 获取本地化资源
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public string GetString(string name)
        {
            if (_resources.TryGetValue(name, out var value))
            {
                return value;
            }
            else
            {
                return name;
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
                if (_resources.TryGetValue(name, out var value))
                {
                    return value;
                }
                else
                {
                    return name;
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
            try
            {
                var res = await _i18NChannel.Client.CultureFeatureAsync(new CulturesRequest
                {
                    Action = ActionTypes.List
                });
                if (res.Code == ReplyCode.Success)
                {
                  return res.Items.ToArray();
                }
            }
            catch (Exception)
            {

                throw;
            }
            return Array.Empty<CultureItem>();
        }

        /// <summary>
        /// 重载本地化资源
        /// </summary>
        public void ReloadAll() => _localizerCache.Clear();

        /// <summary>
        /// /// 重载本地化资源
        /// </summary>
        /// <param name="culture"></param>
        public void Reload(string culture)
        {
            if (_localizerCache.ContainsKey(culture))
            {
                _localizerCache.Remove(culture);
            }
        }

    }

    public static class StringLocalizerStoreExtensions
    {
        /// <summary>
        /// /// 添加本地化资源服务
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddStringLocalizerStore(this IServiceCollection services)
        {
            services.TryAddScoped<IStringLocalizerStore, StringLocalizerStore>();
            return services;
        }

        /// <summary>
        /// /// 添加本地化资源服务
        /// </summary>
        /// <param name="app"></param>
        /// <returns></returns>
        public static IApplicationBuilder UsRequestLocalizatioStore(this IApplicationBuilder app)
        {
            var localizerStore = app.ApplicationServices.GetService<IStringLocalizerStore>();
            if (localizerStore != null)
            {
                var culture = CultureInfo.CurrentCulture.Name;
                var resources = localizerStore.GetCultures().GetAwaiter().GetResult();

                var supportedCultures = resources.Select(s => new CultureInfo(s.Code)).ToArray();
                var defaultCulture = resources.FirstOrDefault(w=>w.IsDefault)?.Code ?? CultureInfo.CurrentCulture.Name;
                app.UseRequestLocalization(new RequestLocalizationOptions
                {
                    DefaultRequestCulture = new RequestCulture(defaultCulture),
                    SupportedCultures = supportedCultures,
                    SupportedUICultures = supportedCultures
                });
            }
            return app;
        }
    }

}
