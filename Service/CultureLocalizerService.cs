﻿using AspNetCore.Grpc.LocalizerStore.Rpc;
using GoI18n;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

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

    }

    public static class CultureLocalizerServiceExtensions
    {
        /// <summary>
        /// /// 添加本地化资源管理服务
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