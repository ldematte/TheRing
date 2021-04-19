using System;
using System.IO;
using Grpc.Core;

namespace TheRing.Common.Grpc.Server
{
    public static class GrpcServerHelpers
    {
        // TODO: extract serialize/deserialize and generalize (loose dependency on ProtoBuf)
        public static Method<Void, T> CreateServerStreamingMethod<T>(string serviceName, string methodName) =>
            new Method<Void, T>(
                type: MethodType.ServerStreaming,
                serviceName: serviceName,
                name: methodName,
                requestMarshaller: Marshallers.Create(
                    serializer: request =>
                    {
                        var x = new MemoryStream();
                        ProtoBuf.Serializer.Serialize(x, request);
                        return x.GetBuffer();
                    },
                    deserializer: _ => Void.Instance),
                responseMarshaller: Marshallers.Create(
                    serializer: request =>
                    {
                        var x = new MemoryStream();
                        ProtoBuf.Serializer.Serialize<T>(x, request);
                        return x.GetBuffer();
                    },
                    deserializer: bytes => ProtoBuf.Serializer.Deserialize<T>(new ReadOnlySpan<byte>(bytes))));

        public static void AddServerStreamingMethod<T>(ServerServiceDefinition.Builder builder, Method<Void, T> method, Forwarder<T> forwarder) where T : class
        {
            builder.AddMethod(method, async (Void _, IServerStreamWriter<T> responseStream, ServerCallContext context) =>
            {
                await forwarder.Subscribe(async reader =>
                {
                    while (context.CancellationToken.IsCancellationRequested == false)
                    {
                        var success = false;
                        try
                        {
                            var x = await reader.ReadAsync();
                            await responseStream.WriteAsync(x);
                            success = true;
                        }
                        catch (OperationCanceledException)
                        {
                        }

                        if (success == false)
                            break;
                    }
                });
            });
        }

        

        public static void AddServerStreamingMethod<T>(ServerServiceDefinitionBuilder builder, Method<Void, T> method, Forwarder<T> forwarder) where T : class
        {
            builder.AddMethod(method, async (Void _, IServerStreamWriter<T> responseStream, ServerCallContext context) =>
            {
                await forwarder.Subscribe(async reader =>
                {
                    while (context.CancellationToken.IsCancellationRequested == false)
                    {
                        var success = false;
                        try
                        {
                            var x = await reader.ReadAsync();
                            await responseStream.WriteAsync(x);
                            success = true;
                        }
                        catch (OperationCanceledException)
                        {
                        }

                        if (success == false)
                            break;
                    }
                });
            });
        }
    }
}
