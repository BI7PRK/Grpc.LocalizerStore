using GoI18n;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Grpc.Net.Client;
using Grpc.Net.Client.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace AspNetCore.Grpc.LocalizerStore.Rpc
{

    public class LocalizerChannelOptions
    {
        /// <summary>
        /// 请求超时，单位秒
        /// </summary>
        public int Timeout { get; set; } = 30;
        /// <summary>
        /// 服务地址
        /// </summary>
        public string Url { get; set; } = "";

        public Dictionary<string, string> Headers { get; set; } = [];
    }

    public class LocalizerChannelBasic
    {
        public static CallInvoker GetGrpcChannel(GrpcErrorInterceptor errorInterceptor, LocalizerChannelOptions option)
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
            var grpcChannel = GrpcChannel.ForAddress(option.Url, new GrpcChannelOptions
            {
                ServiceConfig = new ServiceConfig { MethodConfigs = { defaultMethodConfig } },
                HttpClient = new HttpClient
                {
                    Timeout = TimeSpan.FromSeconds(option.Timeout),
                }
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
        public LocalizerChannel(GrpcErrorInterceptor errorInterceptor, LocalizerChannelOptions option)
        {
            var _grpcChannel = GetGrpcChannel(errorInterceptor, option);
            _channel = new I18nService.I18nServiceClient(_grpcChannel);
        }
        public I18nService.I18nServiceClient Client => _channel;
    }


    public static class LocalizerChannelExtensions
    {
        public static void AddLocalizerChannel(this IServiceCollection services, Action<LocalizerChannelOptions> setupAction)
        {
            var options = new LocalizerChannelOptions();
            setupAction(options);
            services.TryAddSingleton(options);
            services.TryAddSingleton<GrpcErrorInterceptor>();
            services.TryAddSingleton<ILocalizerChannel, LocalizerChannel>();
        }
    }
}
