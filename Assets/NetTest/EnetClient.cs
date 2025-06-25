using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Codes;
using Codes.InGame.Player_Ingame;
using enet;
using NetCode.ENetCode;
using Newtonsoft.Json;
using UnityEngine;

namespace NetTest
{
    public unsafe class EnetClient:MonoBehaviour
    {
        private enet.ENetHost* client;
        private enet.ENetPeer* server;
        private bool isConnected = false;
        private bool running;
        private Task eventLoopTask;
        private ConcurrentQueue<byte[]> receiveQueue = new();
        public ConcurrentQueue<byte[]> requestQueue = new();
        
        
        
        void StartSocketClient()
        {
            running = true;
            unsafe
            {
                try
                {
                    enet.ENet.enet_initialize();
                    Debug.Log("Initialized EnetClient");
                }
                catch (Exception e)
                {
                    Debug.Log(e);
                }
            
                client = enet.ENet.enet_host_create(null, 1, 2, 0, 0);
                ENetAddress address = new ENetAddress();
                if (client == null)
                {
                    Debug.LogError("client not created");
                }
                enet.ENet.enet_address_set_host_ip(&address, NetTestStatic.instance.dedicatedBaseUrl);
                address.port = Convert.ToUInt16(NetTestStatic.instance.dedicatedPort);
                Debug.LogError((*client).serviceTime);
                //Address address = new Address();
                server = enet.ENet.enet_host_connect(client,&address,1,2);
                Debug.Log("Server State:" + server->state.ToString());
                Debug.Log("try to connect on " + server->host->ToString() + ":"+address.host.ipv4->ToString() + ":" + address.port.ToString());
                eventLoopTask = Task.Run(ENetEventLoop);
            }
        }

        
        private const int DefaultPacketLength = 11;
        /// <summary>
        /// should be return to ArrayPool
        /// </summary>
        /// // Packet header format:
        /// 0~7   : Timestamp (UInt64)
        /// 8~9   : SessionKey (UInt16)
        /// 10    : SocketEventType (1 byte)
        /// <returns>should be return to ArrayPool!!should be return to ArrayPool!!should be return to ArrayPool!!</returns>
        byte[] MakeDefaultPacket(SocketEventType type,UInt16 sessionKey,UInt64 timeStamp, byte[] payload)
        {
            byte[] msg = ArrayPool<byte>.Shared.Rent(payload.Length+DefaultPacketLength);
            Buffer.BlockCopy(BitConverter.GetBytes(timeStamp), 0, msg, 0, 8);
            Buffer.BlockCopy(BitConverter.GetBytes(sessionKey), 0, msg, 8, 2);
            msg[10] = (byte)type;
            Buffer.BlockCopy(payload, 0, msg, 11, payload.Length);
            return msg;
        }
        
        public unsafe void SendAssignPacket(AssignRequestDto dto)
        {
            var json = JsonConvert.SerializeObject(dto);
            int maxBytes = Encoding.UTF8.GetMaxByteCount(json.Length);
            byte[] binary = ArrayPool<byte>.Shared.Rent(maxBytes);
            int binaryLength = Encoding.UTF8.GetBytes(json, 0, json.Length, binary, 0);
            var msg = MakeDefaultPacket(SocketEventType.Assign,0,0,binary);
            
            try
            {
                fixed (byte* ptr = msg)
                {
                    ENetPacket* packet = enet.ENet.enet_packet_create(ptr,
                        (nuint)(binaryLength + DefaultPacketLength),
                        (int)enet.ENetPacketFlag.ENET_PACKET_FLAG_RELIABLE);

                    enet.ENet.enet_peer_send(server, 0, packet);
                }
            }
            catch (Exception e)
            {
                Debug.Log(e);
                throw;
            }
            finally
            {
                //버퍼 반환
                ArrayPool<byte>.Shared.Return(binary);
                ArrayPool<byte>.Shared.Return(msg);
            }
            
        }
        
        
        private const int InputPayloadLength = 22;
        private Vector3 rotTemp;
        private Vector2 inputTemp;
        public unsafe void SendMovePacket(MoveRequestDto dto)
        {
            //보낼 정보가 이전 정보와 같으면 리턴
            if (dto.InputVector == inputTemp && dto.RotEular.Approximately(rotTemp))
                return;
            rotTemp = dto.RotEular;
            inputTemp = dto.InputVector;
            //Move 내용 byte[]로 인코딩
            byte[] binaryInput = ArrayPool<byte>.Shared.Rent(InputPayloadLength);
            EncodeInput(binaryInput, dto);

            //기본 패킷 형식+Move 내용
            byte[] msg = MakeDefaultPacket(SocketEventType.Move, dto.SessionKey, dto.Timestamp, binaryInput);
            try
            {
                fixed (byte* ptr = msg)
                {
                    ENetPacket* packet = enet.ENet.enet_packet_create(ptr,
                        (nuint)(binaryInput.Length + DefaultPacketLength),
                        (int)enet.ENetPacketFlag.ENET_PACKET_FLAG_UNSEQUENCED);

                    enet.ENet.enet_peer_send(server, 0, packet);
                }
            }
            catch (Exception e)
            {
                Debug.Log(e);
                throw;
            }
            finally
            {
                //버퍼 반환
                ArrayPool<byte>.Shared.Return(msg);
                ArrayPool<byte>.Shared.Return(binaryInput);
            }
        }
        private void EncodeInput(byte[] buf, MoveRequestDto dto)
        {
            Buffer.BlockCopy(BitConverter.GetBytes(dto.UserPrivateKey), 0, buf, 0, 8);
            
            // sbyte → byte로 저장 (역직렬화 시 sbyte로 해석)
            sbyte x = (sbyte)(dto.InputVector.x * 127f);
            sbyte y = (sbyte)(dto.InputVector.y * 127f);
            buf[8] = (byte)x;
            buf[9] = (byte)y;
            Buffer.BlockCopy(BitConverter.GetBytes(dto.RotEular.x), 0, buf, 10, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(dto.RotEular.y), 0, buf, 14, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(dto.RotEular.z), 0, buf, 18, 4);
        }
        
        

        public void Update()
        {
            while (receiveQueue.TryDequeue(out byte[] data))
            {
                
                
                //receiveQueue의 data들은 모두 Rent해온거(enet 패킷은 destroy되니까 거를 필요 있음)
                ArrayPool<byte>.Shared.Return(data);
            }
        }
        
        private void ENetEventLoop()
        {
            if (client == null) return;
            ENetEvent netEvent;
            while (running)
            {
                while (enet.ENet.enet_host_service(client, &netEvent, 1) > 0)
                {
                    switch (netEvent.type)
                    {
                        case ENetEventType.ENET_EVENT_TYPE_CONNECT:
                            isConnected = true;
                            break;

                        case ENetEventType.ENET_EVENT_TYPE_RECEIVE:

                            byte[] data = ArrayPool<byte>.Shared.Rent((int)netEvent.packet->dataLength);
                            Marshal.Copy((IntPtr)netEvent.packet->data, data, 0, data.Length);
                            receiveQueue.Enqueue(data);
                            enet.ENet.enet_packet_destroy(netEvent.packet);
                            break;

                        case ENetEventType.ENET_EVENT_TYPE_DISCONNECT:
                            isConnected = false;
                            running = false;
                            break;
                    }
                }
            }
        }

        void OnApplicationQuit()
        {
            enet.ENet.enet_host_destroy(client);
            enet.ENet.enet_deinitialize();
        }
    }
}