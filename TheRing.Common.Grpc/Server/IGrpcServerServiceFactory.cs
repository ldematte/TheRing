namespace TheRing.Common.Grpc.Server
{
    public interface IGrpcServerServiceFactory<out T>
    {
        IGrpcServerService<T> Create();
    }
}