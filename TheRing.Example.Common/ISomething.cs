using System;
using System.Collections.Generic;
using TheRing.Common;
using TheRing.Common.Serializer;

namespace TheRing.Example.Common
{
    public interface IBase<T>
    {
        void Foo(T x, double y);
    }

    [RingProtocol]
    public interface ISomething<T>: IBase<int> where T: IComparable
    //public interface ISomething: IBase<int> 
    {
        void Bar(T x);
        void Baz(ReadOnlyMemory<byte> payload);
    }


    



    class GeneratorSerializer: ICallSerializer
    {
        private readonly GeneratorParameterSerializer m_parameterSerializer;
        private readonly IReadOnlyDictionary<string, long> m_methodIdentifiers;

        private static readonly IDictionary<Type, IElementSerializer> m_customSerializers =
            new Dictionary<Type, IElementSerializer>();

        public GeneratorSerializer(byte[] buffer)
        {
            m_parameterSerializer = new GeneratorParameterSerializer(buffer);
        }

        public void SerializeCall(string serviceName, string methodName, Action<IParameterSerializer> serializeParameters)
        {
            var methodFullName = $"{serviceName}.{methodName}";
            if (m_methodIdentifiers.TryGetValue(methodFullName, out var id))
            {
                m_parameterSerializer.Serialize(id);
            }
            serializeParameters(m_parameterSerializer);

        }

        public void SerializeCall(short serviceNumber, short methodNumber, Action<IParameterSerializer> serializeParameters)
        {
            m_parameterSerializer.Serialize(serviceNumber);
            m_parameterSerializer.Serialize(methodNumber);
            serializeParameters(m_parameterSerializer);
        }

        private class GeneratorParameterSerializer : IParameterSerializer
        {
            private byte[] m_buffer;
            private int m_cursor;

           

            public GeneratorParameterSerializer(byte[] buffer)
            {
                m_buffer = buffer;
            }

            public void Serialize(int x)
            {
            }

            public void Serialize(long l)
            {
            }

            public void Serialize(short s)
            {
            }

            public void Serialize(double d)
            {
            }

            public void Serialize<T>(T payload)
            {
                if (m_customSerializers.TryGetValue(typeof(T), out var serializer))
                {
                    var s = (IElementSerializer<T>) serializer;
                    var bytesWritten = s.Serialize(payload, new Span<byte>(m_buffer, m_cursor, m_buffer.Length - m_cursor));
                    m_cursor += bytesWritten;
                }
                else
                {
                    throw new InvalidOperationException($"No serializer for {typeof(T).Name} has been registered");
                }
            }
        }


        public static void AddSerializer<T>(IElementSerializer<T> serializer)
        {
            m_customSerializers[typeof(T)] = serializer;
        }
    }

    class SerializingSomething<T> : ISomething<T> where T : IComparable
    {
        private const short ClassId = 1;
        private readonly ICallSerializer m_serializer;

        public SerializingSomething(ICallSerializer serializer)
        {
            m_serializer = serializer;
        }

        public void Foo(int x, double y)
        {
            m_serializer.SerializeCall(nameof(SerializingSomething<T>), nameof(Foo), s =>
            {
                s.Serialize(x);
                s.Serialize(y);
            });
            
        }

        public void Bar(T x)
        {
            m_serializer.SerializeCall(ClassId, 2, serializer =>
            {

            });
        }

        public void Baz(ReadOnlyMemory<byte> payload)
        {
            m_serializer.SerializeCall(nameof(SerializingSomething<T>), nameof(Foo), s =>
            {
                s.Serialize(payload);
            });
        }
    }

}
