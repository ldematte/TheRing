using Grpc.Core;

namespace TheRing.Common.Grpc.Server
{
    public interface IServerServiceDefinition
    {
        void Visit(ServiceBinderBase serviceBinder);
    }
}