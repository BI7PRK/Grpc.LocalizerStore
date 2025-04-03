using Grpc.Core;
using Grpc.Core.Interceptors;
using Microsoft.Extensions.Logging;
using System.Net.Sockets;

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
            catch (SocketException ex)
            {
                throw new Exception("连接失败");
            }
            catch (RpcException ex)
            {
                logger.LogError(ex, "调用GRPC异常 Call Method:{MethodFullName} Request: {Request} HttpUrl:{MethodName}", context.Method.FullName, request, context.Method.Name);
                throw new Exception(ex.Status.Detail);
            }
            catch (Exception ex)
            {
                return Activator.CreateInstance<TResponse>();
            }
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
                var response = await responseTask;
                return response;
            }
            catch (SocketException ex)
            {
                throw new Exception("[HandleResponse] 连接失败");
            }
            catch (RpcException ex)
            {
                logger.LogError(ex, $"调用GRPC异常 Call Method:{method.FullName} Request: {request}");
                throw new Exception(ex.Status.Detail);
            }
            catch (Exception ex)
            {
                return Activator.CreateInstance<TResponse>();
            }
        }
    }
}
