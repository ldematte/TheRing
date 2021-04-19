namespace TheRing.Common.Serializer
{
    public interface IParameterSerializer
    {
        void Serialize(int x);
        void Serialize(double d);
        void Serialize<T>(T payload);
    }
}