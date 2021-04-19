using System;
using System.Collections.Immutable;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace TheRing.Common.Grpc
{
    public class Forwarder<T>
    {
        private ImmutableArray<ChannelWriter<T>> m_clients;

        public Forwarder()
        {
            m_clients = ImmutableArray<ChannelWriter<T>>.Empty;
        }

        public void Forward(T item)
        {
            try
            {
                var clients = m_clients;
                foreach (var client in clients)
                {
                    client.TryWrite(item);
                }
            }
            catch (OperationCanceledException) { }
        }

        public delegate Task ClientDelegate(ChannelReader<T> reader);

        public async Task Subscribe(ClientDelegate client)
        {
            // add action to the list of observers
            var clientChannel = Channel.CreateBounded<T>(new BoundedChannelOptions(100)
            {
                AllowSynchronousContinuations = true,
                FullMode = BoundedChannelFullMode.DropOldest,
                SingleReader = true,
                SingleWriter = true
            });
            var writer = clientChannel.Writer;

            m_clients = m_clients.Add(writer);

            try
            {
                await client(clientChannel.Reader);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            finally
            {
                // remove action to the list of observers
                m_clients = m_clients.Remove(writer);
            }
        }
    }
}