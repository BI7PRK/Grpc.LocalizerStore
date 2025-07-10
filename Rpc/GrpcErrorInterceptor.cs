using Grpc.Core;
using Grpc.Core.Interceptors;
using Microsoft.Extensions.Logging;

namespace AspNetCore.Grpc.LocalizerStore.Rpc
{
    public class GrpcErrorInterceptor(ILogger<GrpcErrorInterceptor> logger) : Interceptor
    {
        public override TResponse BlockingUnaryCall<TRequest, TResponse>(TRequest request, ClientInterceptorContext<TRequest, TResponse> context, BlockingUnaryCallContinuation<TRequest, TResponse> continuation)
        {
            try
            {
                return continuation(request, context);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "调用GRPC异常 Call Method:{MethodFullName} Request: {Request} HttpUrl:{MethodName}", context.Method.FullName, request, context.Method.Name);
            }

            return Activator.CreateInstance<TResponse>();
        }
        public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(TRequest request, ClientInterceptorContext<TRequest, TResponse> context, AsyncUnaryCallContinuation<TRequest, TResponse> continuation)
        {
            var call = continuation(request, context);
            return new AsyncUnaryCall<TResponse>(HandleResponse(context.Method, request, call.ResponseAsync), call.ResponseHeadersAsync, call.GetStatus, call.GetTrailers, call.Dispose);
        }

        private async Task<TResponse> HandleResponse<TRequest, TResponse>(Method<TRequest, TResponse> method, TRequest request, Task<TResponse> responseTask)
        {
            try
            {
                return await responseTask;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"调用GRPC异常 Call Method:{method.FullName} Request: {request}");
            }
            return Activator.CreateInstance<TResponse>();
        }
    }
}
