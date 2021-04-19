namespace TheRing.Common.Grpc.Server
{
    public interface IGrpcServiceRegister
    {
        void RegisterService<TService, TFactory>()
            where TService : class
            where TFactory : IGrpcServerServiceFactory<TService>, new();
    }
}