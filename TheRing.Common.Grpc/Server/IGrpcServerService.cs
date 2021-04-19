namespace TheRing.Common.Grpc.Server
{
    public interface IGrpcServerService<out T>
    {
        public IServerServiceDefinition ServiceDefinition { get; }
        public T Subscriber { get; }
    }
}