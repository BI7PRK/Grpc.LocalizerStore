using AspNetCore.Grpc.LocalizerStore.Rpc;
using GoI18n;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using System.Reflection;
using static Grpc.Core.Metadata;

namespace AspNetCore.Grpc.LocalizerStore.Service
{
    public interface ICultureLocalizerService
    {
        /// <summary>
        /// /添加国家语言及键值
        /// </summary>
        /// <param name="key"></param>
        /// <param name="values"></param>
        /// <param name="tid"></param>
        /// <returns></returns>
        Task<CultureBaseReply> AddResourceKeyValueAsync(string key, CultureKeyValue[] values, int tid = 0);
        /// <summary>
        /// /// /// 编辑国家语言
        /// </summary>
        /// <param name="culture"></param>
        /// <returns></returns>
        Task<CulturesReply> EditCultureAsync(CultureItem culture);
        /// <summary>
        /// /// /// 编辑语言资源的键
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        Task<CultureKeysReply> EditResourceKeyAsync(CultureKeyItem data);
        /// <summary>
        /// /// /// 编辑语言资源的键值
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        Task<CultureKeyValuesReply> EditResourceKeyValueAsync(CultureKeyValueItem data);
        /// <summary>
        /// /// 编辑语言资源类别
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        Task<CulturesTypesReply> EditResourceTypeAsync(CultureTypeItem data);
        /// <summary>
        /// 获取国家语言列表
        /// </summary>
        /// <returns></returns>
        Task<CulturesReply> GetCultureListAsync();
        /// <summary>
        /// /// 获取语言资源的键列表
        /// </summary>
        /// <param name="index"></param>
        /// <param name="limit"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        Task<CultureKeysReply> GetResourceKeyPagerAsync(int index, int limit, string key);
        /// <summary>
        /// /// 获取语言资源的键值列表
        /// </summary>
        /// <param name="index"></param>
        /// <param name="limit"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        Task<CultureKeyValuesReply> GetResourceKeyValuePagerAsync(int index, int limit, string key);
        /// <summary>
        /// /// 获取语言资源的类别列表
        /// </summary>
        /// <param name="index"></param>
        /// <param name="limit"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        Task<CulturesTypesReply> GetResourceTypePagerAsync(int index, int limit, string key);
        /// <summary>
        /// /// 删除语言资源的键
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<CultureKeysReply> RemoveResourceKeyAsync(int id);
        /// <summary>
        /// /// 删除语言资源类别
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<CulturesTypesReply> RemoveResourceTypeAsync(int id);

        /// <summary>
        /// 导入本地化资源到数据库
        /// </summary>
        /// <param name="assemblyType"></param>
        /// <returns></returns>
        Task<IEnumerable<LocalizerResource>> ImportRsource(Type assemblyType);
    }

    public class CultureLocalizerService : ICultureLocalizerService
    {
        private readonly ILogger<CultureLocalizerService> _logger;
        private readonly I18nService.I18nServiceClient _channel;
        public CultureLocalizerService(ILogger<CultureLocalizerService> logger, ILocalizerChannel channel)
        {
            _logger = logger;
            _channel = channel.Client;
        }



        #region 国家语言
        public async Task<CulturesReply> EditCultureAsync(CultureItem culture)
        {

            return await _channel.CultureFeatureAsync(new CulturesRequest
            {
                Action = ActionTypes.AddOrUpdate,
                ParamData = culture
            });
        }

        public async Task<CulturesReply> GetCultureListAsync()
        {
            return await _channel.CultureFeatureAsync(new CulturesRequest
            {
                Action = ActionTypes.List,
                Index = 1,
                Size = 999999,
            });
        }
        #endregion

        #region 类别
        public async Task<CulturesTypesReply> EditResourceTypeAsync(CultureTypeItem data)
        {
            return await _channel.CulturesResourceTypeFeatureAsync(new CultureTypesRequest
            {
                Action = ActionTypes.AddOrUpdate,
                ParamData = data
            });
        }

        public async Task<CulturesTypesReply> RemoveResourceTypeAsync(int id)
        {
            return await _channel.CulturesResourceTypeFeatureAsync(new CultureTypesRequest
            {
                Action = ActionTypes.Delete,
                ParamData = new CultureTypeItem
                {
                    Id = id
                }
            });
        }

        public async Task<CulturesTypesReply> GetResourceTypePagerAsync(int index, int limit, string key)
        {
            return await _channel.CulturesResourceTypeFeatureAsync(new CultureTypesRequest
            {
                Action = ActionTypes.List,
                ParamData = new CultureTypeItem
                {
                    Name = key
                },
                Index = index,
                Size = limit
            });
        }
        #endregion

        #region 语言资源的键
        public async Task<CultureKeysReply> EditResourceKeyAsync(CultureKeyItem data)
        {
            return await _channel.CulturesResourceKeyFeatureAsync(new CultureKeysRequest
            {
                Action = ActionTypes.AddOrUpdate,
                ParamData = data
            });
        }

        public async Task<CultureKeysReply> RemoveResourceKeyAsync(int id)
        {
            return await _channel.CulturesResourceKeyFeatureAsync(new CultureKeysRequest
            {
                Action = ActionTypes.Delete,
                ParamData = new CultureKeyItem
                {
                    Id = id
                }
            });
        }

        public async Task<CultureKeysReply> GetResourceKeyPagerAsync(int index, int limit, string key)
        {
            return await _channel.CulturesResourceKeyFeatureAsync(new CultureKeysRequest
            {
                Action = ActionTypes.List,
                ParamData = new CultureKeyItem
                {
                    Name = key
                },
                Index = index,
                Size = limit
            });
        }
        #endregion

        public async Task<CultureKeyValuesReply> EditResourceKeyValueAsync(CultureKeyValueItem data)
        {
            return await _channel.CulturesResourceKeyValueFeatureAsync(new CultureKeyValuesRequest
            {
                Action = ActionTypes.AddOrUpdate,
                ParamData = data
            });
        }

        public async Task<CultureKeyValuesReply> GetResourceKeyValuePagerAsync(int index, int limit, string key)
        {
            return await _channel.CulturesResourceKeyValueFeatureAsync(new CultureKeyValuesRequest
            {
                Action = ActionTypes.List,
                Index = index,
                Size = limit,
                SearchKey = key
            });
        }

        public async Task<CultureBaseReply> AddResourceKeyValueAsync(string key, CultureKeyValue[] values, int tid = 0)
        {
            return await _channel.AddResourceKeyValueAsync(new AddCultureKeyValueRequest
            {
                Key = key,
                Values = { values },
                TypeId = tid,
            });
        }


        #region 工具方法       
        public async Task<IEnumerable<LocalizerResource>> ImportRsource(Type assemblyType)
        {
            var dataSource = FindAllResources(assemblyType);
            if (!dataSource.Any()) return dataSource;
            var c = await _channel.CultureFeatureAsync(new CulturesRequest
            {
                Action = ActionTypes.List,

            });
            var cultures = c.Items.ToList();
            if (!cultures.Any())
            {
                _logger.LogWarning("No cultures found in the database, please add cultures first.");
                return dataSource;
            }
            // Get the resource categorys
            var types = await _channel.CulturesResourceTypeFeatureAsync(new CultureTypesRequest
            {
                Action = ActionTypes.List,
                Index = 1,
                Size = 999999,
            });
            foreach (var item in dataSource)
            {
                try
                {
                    if (item.Tid == 0)
                    {
                        var type = types.Items.FirstOrDefault(f => f.Name == item.Category);
                        if (type == null)
                        {
                            // If the type does not exist, create a new one
                            var typeReply = await _channel.CulturesResourceTypeFeatureAsync(new CultureTypesRequest
                            {
                                Action = ActionTypes.AddOrUpdate,
                                ParamData = new CultureTypeItem { Name = item.Category }
                            });
                            item.Tid = typeReply.Items.FirstOrDefault()?.Id ?? 0;
                        }
                        else
                        {
                            item.Tid = type.Id;
                        }
                    }
                    var cultureId = cultures.FirstOrDefault(f => f.Code == item.Code)?.Id ?? 0;
                    var res = await _channel.AddResourceKeyValueAsync(new AddCultureKeyValueRequest
                    {
                        Key = item.Key,
                        Values = { new CultureKeyValue { CultureId = cultureId, Text = item.Value } },
                        TypeId = item.Tid,
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "AddResourceKeyValueAsync failed: {0}", ex.Message);
                    break;
                }
               
            }
            return dataSource;
        }
        /// <summary>
        /// 获取项目内的所有国际化资源
        /// </summary>
        /// <param name="AssemblyType"></param>
        /// <returns></returns>
        private IEnumerable<LocalizerResource> FindAllResources(Type AssemblyType)
        {
            var result = new List<LocalizerResource>();
            var filelocal = AssemblyType.Assembly.Location;
            var dir = Path.GetDirectoryName(filelocal);
            ArgumentNullException.ThrowIfNull(nameof(dir));

            var MayaAssemblys = new List<string>();
            foreach (var item in Directory.GetFiles(dir!, "*.dll"))
            {
                var source = GetResource(Path.GetFileName(item).Replace(".dll", ""));
                if (source.Any()) result.AddRange(source);
            }
            return result.DistinctBy(k => k.Key);
        }
        private List<LocalizerResource> GetResource(string name)
        {
            var result = new List<LocalizerResource>();
            var subClass = typeof(ILocalizerResourceKeys);
            var Resource = Assembly.Load(name).GetTypes().Where(w => w.IsInterface == false && subClass.IsAssignableFrom(w));
            foreach (var item in Resource)
            {
                foreach (var field in item.GetFields(BindingFlags.Static | BindingFlags.Public))
                {
                    var attr = field.GetCustomAttribute<LocalizerDefaultAttribute>();
                    if (attr == null) continue;
                    var key = field.GetValue(item)?.ToString();
                    result.Add(new LocalizerResource
                    {
                        Code = attr.Code ?? "zh-CN",
                        Value = attr.Value ?? "",
                        Key = (key ?? field.Name).ToUpper(),
                        Tid = attr.Tid,
                    });
                }

            }
            return result;
        } 
        #endregion
    }

    public class LocalizerResource
    {
        public string Key { get; set; }
        public string Value { get; set; }
        public string Code { get; set; }

        public int Tid { get; set; }

        public string? Category { get; set; }
    }

    public static class CultureLocalizerServiceExtensions
    {
        /// <summary>
        /// 添加本地化资源管理服务
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddCultureLocalizerService(this IServiceCollection services)
        {
            services.TryAddSingleton<ICultureLocalizerService, CultureLocalizerService>();
            return services;
        }
    }
}