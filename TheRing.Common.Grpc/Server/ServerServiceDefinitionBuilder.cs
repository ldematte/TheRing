using System;
using System.Collections.Generic;
using Grpc.Core;

namespace TheRing.Common.Grpc.Server
{
    public class ServerServiceDefinitionBuilder
    {
        private readonly List<Action<ServiceBinderBase>> m_addMethodActions = new();

        public void AddMethod<TRequest, TResponse>(
            Method<TRequest, TResponse> method,
            ServerStreamingServerMethod<TRequest, TResponse> handler)
            where TRequest : class
            where TResponse : class
        {

            m_addMethodActions.Add(serviceBinder => serviceBinder.AddMethod(method, handler));
        }

        public IServerServiceDefinition Build()
        {
            return new ServerServiceDefinition(m_addMethodActions);
        }

        private class ServerServiceDefinition : IServerServiceDefinition
        {
            private readonly IReadOnlyList<Action<ServiceBinderBase>> m_addMethodActions;

            public ServerServiceDefinition(IReadOnlyList<Action<ServiceBinderBase>> addMethodActions)
            {
                m_addMethodActions = addMethodActions;
            }

            public void Visit(ServiceBinderBase serviceBinder)
            {
                foreach (var addMethodAction in m_addMethodActions)
                {
                    addMethodAction(serviceBinder);
                }
            }
        }
    }
}