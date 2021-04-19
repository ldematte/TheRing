using System;

namespace TheRing.Common.Serializer
{
    public interface ICallSerializer
    {
        void SerializeCall(string serviceName, string methodName, Action<IParameterSerializer> serializeParams);
        void SerializeCall(short serviceNumber, short methodNumber, Action<IParameterSerializer> serializeParams);
    }
}