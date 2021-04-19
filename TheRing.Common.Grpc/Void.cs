namespace TheRing.Common.Grpc
{
    public sealed class Void
    {
        public static readonly Void Instance = new Void();

        private Void()
        {
        }
    }
}