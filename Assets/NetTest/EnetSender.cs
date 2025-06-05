using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using enet;

namespace NetTest
{
    public class ENetSendRequest
    {
        public SocketEventType Type;
        public byte[] Payload;
    }

    public unsafe class ENetSender
    {
        private readonly ConcurrentQueue<ENetSendRequest> _sendQueue = new();
        private readonly ENetPeer* _server;
        private TestEnetClient _client;

        public ENetSender(ENetPeer* server, TestEnetClient client)
        {
            _client = client;
            _server = server;
            StartSenderLoop();
        }

        public void SendRequest(SocketEventType type, byte[] payload)
        {
            _sendQueue.Enqueue(new ENetSendRequest
            {
                Type = type,
                Payload = payload
            });
        }

        private void StartSenderLoop()
        {
            Task.Run(() =>
            {
                while (true)
                {
                    while (_sendQueue.TryDequeue(out var req))
                    {
                        var data = req.Payload;
                        fixed (byte* ptr = data)
                        {
                            ENetPacket* packet = enet.ENet.enet_packet_create(ptr, (nuint)(data.Length), 
                                SocketEventTypeHelper.IsReliable(req.Type) ? (uint)ENetPacketFlag.ENET_PACKET_FLAG_RELIABLE : 0u);
                            enet.ENet.enet_peer_send(_server, 0, packet);
                        }
                    }
                    Thread.Sleep(1); // or use await Task.Delay(...)
                }
            });
        }
    }

}