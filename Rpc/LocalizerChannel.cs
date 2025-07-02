using GoI18n;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Grpc.Net.Client;
using Grpc.Net.Client.Configuration;
using System.Net.Security;

namespace AspNetCore.Grpc.LocalizerStore.Rpc
{

    public class LocalizerStoreOption
    {
        /// <summary>
        /// 本地化服务地址
        /// </summary>
        public string Url { get; set; }
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

    public class LocalizerChannelBasic
    {
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
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) =>
                {
                    if (option.SkipCertificateValidation)
                    {
                        return true; // 跳过证书验证
                    }
                    return errors == SslPolicyErrors.None; // 默认验证
                }
            };
            var httpClient = new HttpClient(handler)
            {
                Timeout = TimeSpan.FromSeconds(option.Timeout > 0 ? option.Timeout : 15) // 设置超时时间
            };
            var grpcChannel = GrpcChannel.ForAddress(option.Url, new GrpcChannelOptions
            {
                ServiceConfig = new ServiceConfig { MethodConfigs = { defaultMethodConfig } },
                HttpClient = httpClient
            });
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

    public interface ILocalizerChannel
    {
        I18nService.I18nServiceClient Client { get; }
    }

    public class LocalizerChannel : LocalizerChannelBasic, ILocalizerChannel
    {

        private readonly I18nService.I18nServiceClient _channel;
        public LocalizerChannel(GrpcErrorInterceptor errorInterceptor, LocalizerStoreOption option)
        {
            var _grpcChannel = GetGrpcChannel(errorInterceptor, option);
            _channel = new I18nService.I18nServiceClient(_grpcChannel);
        }
        public I18nService.I18nServiceClient Client => _channel;
    }

}
