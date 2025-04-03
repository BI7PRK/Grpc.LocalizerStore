using AspNetCore.Grpc.LocalizerStore.Rpc;
using GoI18n;
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
        /// 获取本地化资源
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        string GetString(string name);
    }
    public class StringLocalizerStore : IStringLocalizerStore
    {
        readonly ReadOnlyDictionary<string, string> _resources;
        public StringLocalizerStore(ILocalizerChannel i18NChannel, ILogger<StringLocalizerStore> logger)
        {
            logger.LogInformation("code: {0}", CultureInfo.CurrentCulture.Name);
            var resources = new Dictionary<string, string>();
            var res = i18NChannel.Client.GetCultureResources(new CultureCodeRequest
            {
                Code = CultureInfo.CurrentCulture.Name
            });
            if (res.Code == ReplyCode.Success)
            {
                foreach (var item in res.Items)
                {
                    resources.Add(item.Key, item.Text);
                }
                _resources = new ReadOnlyDictionary<string, string>(resources); ;
            }
            else
            {
                logger.LogError("GetCultureResources failed: {0}", res.Message);
            }
        }

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

        public ReadOnlyDictionary<string, string> GetAllStrings()
        {
            return _resources;
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
            services.TryAddSingleton<IStringLocalizerStore, StringLocalizerStore>();
            return services;
        }
    }
}
