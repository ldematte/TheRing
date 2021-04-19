using System;
using System.Collections.Generic;
using Grpc.AspNetCore.Server;
using Grpc.AspNetCore.Server.Model;
using Grpc.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace TheRing.Common.Grpc.Server
{
    public static class GrpcServiceCollectionExtensions
    {
        public static IServiceCollection AddRingGrpc(this IServiceCollection services,
            Action<GrpcServiceOptions> configureOptions, Action<IGrpcServiceRegister> registerServices)
        {
            services.AddGrpc(configureOptions);
            services.TryAddEnumerable(ServiceDescriptor.Singleton(typeof(IServiceMethodProvider<>),
                typeof(CodeFirstServiceMethodProvider<>)));

            registerServices(new AdapterFactory(services));

            return services;
        }


        private interface IAdapterFactory
        {
            bool TryGetDefinition<TService>(out IServerServiceDefinition definition);
        }

        private class AdapterFactory : IAdapterFactory, IGrpcServiceRegister
        {
            private readonly IServiceCollection m_services;
            private readonly Dictionary<Type, IServerServiceDefinition> m_definitions = new();

            public AdapterFactory(IServiceCollection services)
            {
                m_services = services;
            }

            public void RegisterService<TService, TFactory>()
                where TService : class
                where TFactory : IGrpcServerServiceFactory<TService>, new()
            {
                var factory = new TFactory();
                var service = factory.Create();
                m_services.AddSingleton<TService>(service.Subscriber);
                m_definitions.Add(typeof(TService), service.ServiceDefinition);
            }

            public bool TryGetDefinition<TService>(out IServerServiceDefinition definition)
            {
                return m_definitions.TryGetValue(typeof(TService), out definition);
            }
        }

        private sealed class CodeFirstServiceMethodProvider<TService> : IServiceMethodProvider<TService>
            where TService : class
        {
            private readonly IAdapterFactory m_adapterFactory;
            private readonly ILogger<CodeFirstServiceMethodProvider<TService>> m_logger;

            public CodeFirstServiceMethodProvider(ILoggerFactory loggerFactory, IAdapterFactory adapterFactory)
            {
                m_adapterFactory = adapterFactory;
                m_logger = loggerFactory.CreateLogger<CodeFirstServiceMethodProvider<TService>>();
            }

            void IServiceMethodProvider<TService>.OnServiceMethodDiscovery(
                ServiceMethodProviderContext<TService> context)
            {
                if (m_adapterFactory.TryGetDefinition<TService>(out var definition))
                {
                    definition.Visit(new Adapter(m_logger, context));
                }
            }

            private class Adapter : ServiceBinderBase
            {
                private readonly ILogger<CodeFirstServiceMethodProvider<TService>> m_logger;
                private readonly ServiceMethodProviderContext<TService> m_context;

                public Adapter(ILogger<CodeFirstServiceMethodProvider<TService>> logger,
                    ServiceMethodProviderContext<TService> context)
                {
                    m_logger = logger;
                    m_context = context;
                }

                public override void AddMethod<TRequest, TResponse>(Method<TRequest, TResponse> method,
                    ServerStreamingServerMethod<TRequest, TResponse> handler)
                {
                    m_context.AddServerStreamingMethod(method, Array.Empty<object>(),
                        (service, request, stream, context) => handler(request, stream, context));
                    m_logger.Log(LogLevel.Information, "RPC service being provided by {0}: {1}", typeof(TService),
                        method.Name);
                }
            }
        }
    }
}