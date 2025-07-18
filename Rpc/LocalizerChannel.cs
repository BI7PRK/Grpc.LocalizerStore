﻿using GoI18n;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Grpc.Net.Client;
using Grpc.Net.Client.Configuration;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Security;

namespace AspNetCore.Grpc.LocalizerStore.Rpc
{
    /// <summary>
    /// 本地化服务配置选项
    /// </summary>
    public class LocalizerStoreOption
    {
        /// <summary>
        /// 本地化服务地址
        /// </summary>
        public string Url { get; set; } = string.Empty;
        /// <summary>
        /// 超时时间
        /// </summary>
        public int Timeout { get; set; } = 30;

        /// <summary>
        /// 请求头
        /// </summary>
        public Dictionary<string, string> Headers { get; set; } = [];

        /// <summary>
        /// 跳过证书验证
        /// </summary>
        public bool SkipCertificateValidation { get; set; } = false;

        /// <summary>
        /// 是否允许管理本地化资源
        /// </summary>
        public bool AllowManage { get; set; }

        /// <summary>
        /// 默认文化
        /// </summary>
        public string DefaultCulture { get; set; } = "en-US";

        /// <summary>
        /// 是否支持未加密的 HTTP/2
        /// </summary>
        public bool Http2UnencryptedSupport { get; set; } = false;
    }
    /// <summary>
    /// 本地化服务通道基础类
    /// </summary>
    public class LocalizerChannelBasic
    {
        /// <summary>
        /// 获取 gRPC 通道
        /// </summary>
        /// <param name="errorInterceptor"></param>
        /// <param name="option"></param>
        /// <returns></returns>
        public static CallInvoker GetGrpcChannel(GrpcErrorInterceptor errorInterceptor, LocalizerStoreOption option)
        {
            var defaultMethodConfig = new MethodConfig
            {
                Names = { MethodName.Default },
                RetryPolicy = new RetryPolicy
                {
                    MaxAttempts = 2,
                    InitialBackoff = TimeSpan.FromSeconds(1),
                    MaxBackoff = TimeSpan.FromSeconds(5),
                    BackoffMultiplier = 1.5,
                    RetryableStatusCodes = { StatusCode.Unavailable }
                }
            };
            var handler = new SocketsHttpHandler
            {
                 SslOptions = new SslClientAuthenticationOptions
                 {
                      RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) =>
                      {
                          if (option.SkipCertificateValidation)
                          {
                              return true; // 跳过证书验证
                          }
                          return sslPolicyErrors == SslPolicyErrors.None; // 正常验证
                      },
                 },
                 ConnectTimeout = TimeSpan.FromSeconds(option.Timeout > 0 ? option.Timeout : 15), // 设置超时时间
                PooledConnectionIdleTimeout = TimeSpan.FromSeconds(option.Timeout > 0 ? option.Timeout : 15), // 连接池空闲超时时间
            };
            var channelOption = new GrpcChannelOptions
            {
                ServiceConfig = new ServiceConfig { MethodConfigs = { defaultMethodConfig } },
                HttpHandler = handler
            };
            if (option.Http2UnencryptedSupport)
            {
                channelOption.HttpVersion = HttpVersion.Version20; // 支持未加密的 HTTP/2
                channelOption.HttpVersionPolicy = HttpVersionPolicy.RequestVersionExact; // 强制使用 HTTP/2
            }

            var grpcChannel = GrpcChannel.ForAddress(option.Url, channelOption);
            return grpcChannel.Intercept(errorInterceptor).Intercept(met =>
            {
                foreach (var item in option.Headers)
                {
                    met.Add(item.Key, item.Value);
                }
                return met;
            });
        }
    }
    /// <summary>
    /// 本地化服务通道接口
    /// </summary>
    public interface ILocalizerChannel
    {
        /// <summary>
        /// 本地化服务客户端
        /// </summary>
        I18nService.I18nServiceClient Client { get; }
    }
    /// <summary>
    /// 本地化服务通道实现
    /// </summary>
    public class LocalizerChannel : LocalizerChannelBasic, ILocalizerChannel
    {

        private readonly I18nService.I18nServiceClient _channel;
        /// <summary>
        /// 本地化服务通道构造函数
        /// </summary>
        /// <param name="errorInterceptor"></param>
        /// <param name="option"></param>
        public LocalizerChannel(GrpcErrorInterceptor errorInterceptor, LocalizerStoreOption option)
        {
            var _grpcChannel = GetGrpcChannel(errorInterceptor, option);
            _channel = new I18nService.I18nServiceClient(_grpcChannel);
        }
        /// <summary>
        /// 本地化服务客户端
        /// </summary>
        public I18nService.I18nServiceClient Client => _channel;
    }

}
