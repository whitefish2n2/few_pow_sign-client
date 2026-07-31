using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Codes.InGame;
using Cysharp.Threading.Tasks;
using enet;
using NetCode.ENetCode;
using Newtonsoft.Json;
using Plugins;
using UnityEngine;

namespace NetTest{

    public struct NetworkMessage
    {
        public byte[] buffer;
        public int length;
    }
    public struct SendPacketInfo
    {
        public byte[] buffer;
        public int length;
        public ENetPacketFlag flag;
    }
    public class EnetClient:MonoSingleton<EnetClient>
    {
        private unsafe ENetHost* client;
        private unsafe ENetPeer* server;
        private volatile bool isConnected;
        private volatile bool running;
        private Task eventLoopTask;
        private ConcurrentQueue<NetworkMessage> receiveQueue = new();
        private ConcurrentQueue<SendPacketInfo> requestQueue = new();

        #if UNITY_EDITOR
        [SerializeField] private bool logRawPackets = true;
        #endif

        public async UniTask<bool> ConnectAsync(string ip, ushort port, int timeoutSeconds = 5)
        {
            var tcs = new UniTaskCompletionSource<bool>();
            var cts = new CancellationTokenSource();

            StartSocketClient(ip, port);

            async UniTask WaitConnection()
            {
                while (!isConnected && !cts.Token.IsCancellationRequested)
                {
                    await UniTask.Yield(); 
                }
                tcs.TrySetResult(isConnected);
            }

            _ = WaitConnection();

            var timeoutTask = UniTask.Delay(timeoutSeconds * 1000, cancellationToken: cts.Token);
            var finished = await UniTask.WhenAny(tcs.Task, timeoutTask);

            cts.Cancel();
            cts.Dispose();

            if (!finished.hasResultLeft)
            {
                Debug.LogError("[EnetClient] UDP 연결 타임아웃!");
                StopSocketClient();
                return false;
            }

            return await tcs.Task;
        }

        public unsafe void StartSocketClient(string ipAddress, ushort port)
        {
            if (running) return;
            running = true;
            
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
            
            // 매개변수로 받은 동적 IP와 Port 세팅
            enet.ENet.enet_address_set_host_ip(&address, ipAddress);
            address.port = port;
            
            server = enet.ENet.enet_host_connect(client, &address, 1, 2);
            Debug.Log($"try to connect on {ipAddress}:{port}");
            
            eventLoopTask = Task.Run(ENetEventLoop);
        }
        
        public unsafe void StopSocketClient()
        {
            if (!running) return;
    
            running = false; // 이벤트 루프 스레드 정지   

            if (eventLoopTask != null && !eventLoopTask.IsCompleted)
            {
                eventLoopTask.Wait(500); // 스레드 안전 종료 대기
            }

            if (client != null)
            {
                if (server != null)
                {
                    enet.ENet.enet_peer_disconnect(server, 0); 
                }
                enet.ENet.enet_host_destroy(client);
                client = null;
                server = null;
            }

            isConnected = false;
    
            ClearQueues(); 
    
            Debug.Log("EnetClient completely stopped and reset.");
        }

        // ENet이 프로토콜 레벨에서 자체 추적하는 값이라 별도 핑퐁 패킷 없이 바로 읽음
        public unsafe uint GetPingMs()
        {
            return server != null ? server->roundTripTime : 0;
        }

        public unsafe float GetPacketLossPercent()
        {
            if (server == null) return 0f;
            return server->packetLoss / (float)enet.ENet.ENET_PEER_PACKET_LOSS_SCALE * 100f;
        }

        private void ClearQueues()
        {
            // 1. 송신 큐(Request Queue) 비우기 및 배열 반납
            while (requestQueue.TryDequeue(out SendPacketInfo sendInfo))
            {
                if (sendInfo.buffer != null)
                {
                    ArrayPool<byte>.Shared.Return(sendInfo.buffer);
                }
            }
            // 2. 수신 큐(Receive Queue) 비우기 및 배열 반납
            while (receiveQueue.TryDequeue(out NetworkMessage msg))
            {
                if (msg.buffer != null)
                {
                    ArrayPool<byte>.Shared.Return(msg.buffer);
                }
            }

            Debug.Log("[EnetClient] 네트워크 큐 초기화 및 ArrayPool 반납 완료.");
        }
        
        public void SendPacket(ref byte[] payload, int payloadLength, enet.ENetPacketFlag flag)
        {
            if (!isConnected) return;
            requestQueue.Enqueue(new SendPacketInfo { buffer = payload, length = payloadLength, flag = flag });
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
        private byte[] MakeDefaultPacket(SocketEventType type, UInt16 sessionKey, UInt64 timeStamp, byte[] payload, int payloadLength)
        {
            byte[] msg = System.Buffers.ArrayPool<byte>.Shared.Rent(payloadLength + DefaultPacketLength);
    
            Buffer.BlockCopy(BitConverter.GetBytes(timeStamp), 0, msg, 0, 8);
            Buffer.BlockCopy(BitConverter.GetBytes(sessionKey), 0, msg, 8, 2);
            msg[10] = (byte)type;
    
            Buffer.BlockCopy(payload, 0, msg, 11, payloadLength);
    
            return msg;
        }
        
        public unsafe void SendAssignPacket(AssignRequestDto dto)
        {
            
            var json = JsonConvert.SerializeObject(dto);
//            Debug.Log("AssignPacket Send, 内容：" + json);
            int maxBytes = Encoding.UTF8.GetMaxByteCount(json.Length);
            byte[] binary = ArrayPool<byte>.Shared.Rent(maxBytes);
            int binaryLength = Encoding.UTF8.GetBytes(json, 0, json.Length, binary, 0);
            var msg = MakeDefaultPacket(SocketEventType.Assign, MatchMakeStatic.Instance.dedicatedServerIndex, 0, binary, binaryLength);
            try
            {
                SendPacket(ref msg,(binaryLength + DefaultPacketLength),enet.ENetPacketFlag.ENET_PACKET_FLAG_RELIABLE);
            }
            catch (Exception e)
            {
                Debug.Log(e);
                throw;
            }
        }
        
        
        
        private float pitchTemp;
        private float yawTemp;
        private Vector2 inputTemp;
        public unsafe void SendMovePacket(MoveRequestDto dto)
        {
            //보낼 정보가 이전 정보와 같으면 리턴
            if (dto.InputVector == inputTemp && dto.inputPitch.Equals(pitchTemp) && dto.inputYaw.Equals(yawTemp))
                return;
            pitchTemp = dto.inputPitch;
            yawTemp = dto.inputYaw;
            inputTemp = dto.InputVector;
            //Move 내용 byte[]로 인코딩
            byte[] binaryInput = ArrayPool<byte>.Shared.Rent(MoveRequestDto.InputPayloadLength);
            dto.Encode(binaryInput);

            //기본 패킷 형식+Move 내용
            byte[] msg = MakeDefaultPacket(SocketEventType.Move, dto.SessionKey, dto.Timestamp, binaryInput,(MoveRequestDto.InputPayloadLength + DefaultPacketLength));
            try
            {
                SendPacket(ref msg,(MoveRequestDto.InputPayloadLength + DefaultPacketLength),enet.ENetPacketFlag.ENET_PACKET_FLAG_RELIABLE);
            }
            catch (Exception e)
            {
                Debug.Log(e);
                throw;
            }
        }
        
        public unsafe void SendProgressPacket(byte percent)
        {
            int payloadLength = 1; // [Percent (1바이트)]
            byte[] payload = System.Buffers.ArrayPool<byte>.Shared.Rent(payloadLength);
    
            try
            {
                payload[0] = percent;

                
                byte[] msg = MakeDefaultPacket(SocketEventType.Progress, MatchMakeStatic.Instance.dedicatedServerIndex, 0, payload, payloadLength);

                SendPacket(ref msg, payloadLength + DefaultPacketLength, enet.ENetPacketFlag.ENET_PACKET_FLAG_RELIABLE);
            }
            catch (Exception e)
            {
                Debug.LogError($"[EnetClient] Progress Packet 전송 실패: {e.Message}");
            }
            finally
            {
                System.Buffers.ArrayPool<byte>.Shared.Return(payload);
            }
        }
        public void SendInteractPacket()
        {
            int payloadLength = 0; // 페이로드 없음 (서버가 조준 레이캐스트로 권위 판정)
            byte[] payload = System.Buffers.ArrayPool<byte>.Shared.Rent(1);

            try
            {
                byte[] msg = MakeDefaultPacket(SocketEventType.Interact, MatchMakeStatic.Instance.dedicatedServerIndex, 0, payload, payloadLength);

                SendPacket(ref msg, payloadLength + DefaultPacketLength, enet.ENetPacketFlag.ENET_PACKET_FLAG_RELIABLE);
            }
            catch (Exception e)
            {
                Debug.LogError($"[EnetClient] Interact Packet 전송 실패: {e.Message}");
            }
            finally
            {
                System.Buffers.ArrayPool<byte>.Shared.Return(payload);
            }
        }

        public void SendDropWeaponPacket()
        {
            int payloadLength = 0; // 페이로드 없음 (현재 장착무기 드롭)
            byte[] payload = System.Buffers.ArrayPool<byte>.Shared.Rent(1);

            try
            {
                byte[] msg = MakeDefaultPacket(SocketEventType.DropWeapon, MatchMakeStatic.Instance.dedicatedServerIndex, 0, payload, payloadLength);

                SendPacket(ref msg, payloadLength + DefaultPacketLength, enet.ENetPacketFlag.ENET_PACKET_FLAG_RELIABLE);
            }
            catch (Exception e)
            {
                Debug.LogError($"[EnetClient] DropWeapon Packet 전송 실패: {e.Message}");
            }
            finally
            {
                System.Buffers.ArrayPool<byte>.Shared.Return(payload);
            }
        }

        public void SendSwapWeaponPacket(bool up)
        {
            int payloadLength = 1; // [Dir (1바이트)] 1=위, 0=아래
            byte[] payload = System.Buffers.ArrayPool<byte>.Shared.Rent(payloadLength);

            try
            {
                payload[0] = (byte)(up ? 1 : 0);
                byte[] msg = MakeDefaultPacket(SocketEventType.SwapWeapon, MatchMakeStatic.Instance.dedicatedServerIndex, 0, payload, payloadLength);

                SendPacket(ref msg, payloadLength + DefaultPacketLength, enet.ENetPacketFlag.ENET_PACKET_FLAG_RELIABLE);
            }
            catch (Exception e)
            {
                Debug.LogError($"[EnetClient] SwapWeapon Packet 전송 실패: {e.Message}");
            }
            finally
            {
                System.Buffers.ArrayPool<byte>.Shared.Return(payload);
            }
        }

        public void SendReloadPacket()
        {
            int payloadLength = 0; // 페이로드 없음 (현재 장착무기 리로드)
            byte[] payload = System.Buffers.ArrayPool<byte>.Shared.Rent(1);

            try
            {
                byte[] msg = MakeDefaultPacket(SocketEventType.Reload, MatchMakeStatic.Instance.dedicatedServerIndex, 0, payload, payloadLength);

                SendPacket(ref msg, payloadLength + DefaultPacketLength, enet.ENetPacketFlag.ENET_PACKET_FLAG_RELIABLE);
            }
            catch (Exception e)
            {
                Debug.LogError($"[EnetClient] Reload Packet 전송 실패: {e.Message}");
            }
            finally
            {
                System.Buffers.ArrayPool<byte>.Shared.Return(payload);
            }
        }
        public void SendJumpPacket()
        {
            int payloadLength = 0; 
            byte[] payload = System.Buffers.ArrayPool<byte>.Shared.Rent(1);

            try
            {
                byte[] msg = MakeDefaultPacket(SocketEventType.Jump, MatchMakeStatic.Instance.dedicatedServerIndex, 0, payload, payloadLength);

                SendPacket(ref msg, payloadLength + DefaultPacketLength, enet.ENetPacketFlag.ENET_PACKET_FLAG_RELIABLE);
            }
            catch (Exception e)
            {
                Debug.LogError($"[EnetClient] Jump Packet 전송 실패: {e.Message}");
            }
            finally
            {
                System.Buffers.ArrayPool<byte>.Shared.Return(payload);
            }
        }
        public void SendShotPacket()
        {
            int payloadLength = 0; // 페이로드 없음 (서버가 연사율/탄약 게이트 후 자체 origin/dir 구성)
            byte[] payload = System.Buffers.ArrayPool<byte>.Shared.Rent(1);

            try
            {
                byte[] msg = MakeDefaultPacket(SocketEventType.Shot, MatchMakeStatic.Instance.dedicatedServerIndex, 0, payload, payloadLength);

                SendPacket(ref msg, payloadLength + DefaultPacketLength, enet.ENetPacketFlag.ENET_PACKET_FLAG_RELIABLE);
            }
            catch (Exception e)
            {
                Debug.LogError($"[EnetClient] Shot Packet 전송 실패: {e.Message}");
            }
            finally
            {
                System.Buffers.ArrayPool<byte>.Shared.Return(payload);
            }
        }
        public void SendHitThisPacket(byte targetPublicKey, Vector3 origin, Vector3 dir)
        {
            int payloadLength = 25; // targetPublicKey(1)+origin(12)+dir(12)
            byte[] payload = System.Buffers.ArrayPool<byte>.Shared.Rent(payloadLength);

            try
            {
                payload[0] = targetPublicKey;
                BitConverter.GetBytes(origin.x).CopyTo(payload, 1);
                BitConverter.GetBytes(origin.y).CopyTo(payload, 5);
                BitConverter.GetBytes(origin.z).CopyTo(payload, 9);
                BitConverter.GetBytes(dir.x).CopyTo(payload, 13);
                BitConverter.GetBytes(dir.y).CopyTo(payload, 17);
                BitConverter.GetBytes(dir.z).CopyTo(payload, 21);

                byte[] msg = MakeDefaultPacket(SocketEventType.HitThis, MatchMakeStatic.Instance.dedicatedServerIndex, 0, payload, payloadLength);

                SendPacket(ref msg, payloadLength + DefaultPacketLength, enet.ENetPacketFlag.ENET_PACKET_FLAG_RELIABLE);
            }
            catch (Exception e)
            {
                Debug.LogError($"[EnetClient] HitThis Packet 전송 실패: {e.Message}");
            }
            finally
            {
                System.Buffers.ArrayPool<byte>.Shared.Return(payload);
            }
        }
        private unsafe void ENetEventLoop()
        {
            if (client == null) return;
            enet.ENetEvent netEvent;

            while (running)
            {
                // 1. 송신 큐(requestQueue)에 쌓인 걸 이 스레드에서 ENet으로 쏩니다.
                while (requestQueue.TryDequeue(out SendPacketInfo sendInfo))
                {
                    fixed (byte* ptr = sendInfo.buffer)
                    {
                        enet.ENetPacket* packet = enet.ENet.enet_packet_create(ptr, (nuint)sendInfo.length, (uint)sendInfo.flag);
                        enet.ENet.enet_peer_send(server, 0, packet);
                    }
                    ArrayPool<byte>.Shared.Return(sendInfo.buffer); // 쏘고 나서 반납
                }

                // 2. 수신 처리
                while (enet.ENet.enet_host_service(client, &netEvent, 1) > 0) // timeout을 1ms로 줘서 CPU 낭비 방지
                {
                    switch (netEvent.type)
                    {
                        case enet.ENetEventType.ENET_EVENT_TYPE_CONNECT:
                            isConnected = true;
                            Debug.Log("Connected to Server!");
                            break;

                        case enet.ENetEventType.ENET_EVENT_TYPE_RECEIVE:
                            int actualLength = (int)netEvent.packet->dataLength;
                            byte[] data = ArrayPool<byte>.Shared.Rent(actualLength);
                            Marshal.Copy((IntPtr)netEvent.packet->data, data, 0, actualLength);
                            
                            receiveQueue.Enqueue(new NetworkMessage { buffer = data, length = actualLength });
                            enet.ENet.enet_packet_destroy(netEvent.packet);
                            break;

                        case enet.ENetEventType.ENET_EVENT_TYPE_DISCONNECT:
                            isConnected = false;
                            Debug.Log("Disconnected from Server.");
                            break;
                    }
                }
            }
        }

        private void Update()
        {
            while (receiveQueue.TryDequeue(out NetworkMessage msg))
            {
                try
                {
                    ReadOnlySpan<byte> dataSpan = new ReadOnlySpan<byte>(msg.buffer, 0, msg.length);
#if  UNITY_EDITOR
                    if (logRawPackets)
                    {
                        var sb = new StringBuilder();
                        sb.Append($"[EnetClient] RECV ({msg.length}B) Type:{(SocketEventType)dataSpan[0]} | ");
                        for (int i = 0; i < msg.length; i++)
                            sb.Append($"{dataSpan[i]:X2} ");
                        Debug.Log(sb.ToString());
                    }    
#endif
                    
                    ProcessPacket(dataSpan);
                }
                catch (Exception e)
                {
                    Debug.LogError($"Parse Error: {e}"); 
                }
                finally
                {
                    ArrayPool<byte>.Shared.Return(msg.buffer); // 처리 끝난 후 반납
                }
            }
        }

        unsafe void OnApplicationQuit()
        {
            running = false;
    
            if (eventLoopTask != null && !eventLoopTask.IsCompleted)
            {
                eventLoopTask.Wait(500);
            }

            if (client != null)
            {
                enet.ENet.enet_host_destroy(client);
            }
            enet.ENet.enet_deinitialize();
        }
        private void ProcessPacket(ReadOnlySpan<byte> span)
        {
            if (span.Length < 1) return;

            SocketEventType eventType = (SocketEventType)span[0];
            int offset = 1;

            switch (eventType)
            {
                case SocketEventType.AssignResponse:
                    ParseAssignResponse(span.Slice(offset));
                    break;
            
                case SocketEventType.ProgressNotify:
                    ProcessProgressNofity(span.Slice(offset));
                    break;
            
                case SocketEventType.MapInit: // 혹은 DynamicInit
                    ParseMapInit(span.Slice(offset));
                    break;
                case SocketEventType.GeneratePlayer:
                    ParseGeneratePlayer(span.Slice(offset));
                    break;
                case SocketEventType.PlayerMove:
                    ParsePlayerMove(span.Slice(offset));
                    break;
                case SocketEventType.GetWeaponNotify:
                    ParseGetWeaponNotify(span.Slice(offset));
                    break;
                case SocketEventType.DropWeaponNotify:
                    ParseDropWeaponNotify(span.Slice(offset));
                    break;
                case SocketEventType.SwapWeaponNotify:
                    ParseSwapWeaponNotify(span.Slice(offset));
                    break;
                case SocketEventType.ReloadNotify:
                    ParseReloadNotify(span.Slice(offset));
                    break;
                case SocketEventType.ShotNotify:
                    ParseShotNotify(span.Slice(offset));
                    break;
                case SocketEventType.HitNotify:
                    ParseHitNotify(span.Slice(offset));
                    break;
                case SocketEventType.RespawnPlayer:
                    ParseRespawn(span.Slice(offset));
                    break;
                case SocketEventType.Death:
                    ParseDeath(span.Slice(offset));
                    break;
                case SocketEventType.ObjectMove:
                    ParseObjectMove(span.Slice(offset));
                    break;
                case SocketEventType.GenerateObject:
                    ParseGenerateObject(span.Slice(offset));
                    break;
                case SocketEventType.PhaseChangeNotify:
                    ParsePhaseChangeNotify(span.Slice(offset));
                    break;
                case SocketEventType.GameEndNotify:
                    ParseGameEndNotify(span.Slice(offset));
                    break;
                case SocketEventType.RoundEndNotify:
                    ParseRoundEndNotify(span.Slice(offset));
                    break;
                default:
                    Debug.LogWarning($"Unhandled Event Type: {eventType}");
                    break;
            }
        }
        
        public event Action<ProgressNotifyEventDto> OnPlayerProgressUpdated;
        private void ProcessProgressNofity(ReadOnlySpan<byte> span)
        {
            if (span.Length < 2) return;

            ProgressNotifyEventDto eventDto = new ProgressNotifyEventDto
            {
                publicKey = span[0],
                progressPercent = span[1]
            };

            OnPlayerProgressUpdated?.Invoke(eventDto);
        }
        public event Action<AssignResponseDto> OnAssignSuccess;

        private void ParseAssignResponse(ReadOnlySpan<byte> span)
        {
            int offset = 0;

            // 1. 내 Public Key (1바이트)
            byte myPublicKey = span[offset++];

            // 2. 방에 있는 다른 플레이어 수 (1바이트)
            byte otherPlayersCount = span[offset++];

            // DTO 인스턴스화
            AssignResponseDto dto = new AssignResponseDto(myPublicKey);

            // 3. 다른 플레이어 정보 파싱 루프
            for (int i = 0; i < otherPlayersCount; i++)
            {
                byte otherPublicKey = span[offset++];
        
                // 문자열 길이 (2바이트, Little Endian)
                ushort strLen = BitConverter.ToUInt16(span.Slice(offset, 2));
                offset += 2;

                string userId = string.Empty;
                if (strLen > 0)
                {
                    // Span을 이용해 메모리 복사 없이 문자열 디코딩
                    userId = Encoding.UTF8.GetString(span.Slice(offset, strLen));
                    offset += strLen;
                }

                dto.otherPlayers.Add(otherPublicKey, userId);
            }

            Debug.Log($"[Assign] 접속 성공! 내 키: {dto.myPublicKey}, 다른 유저 수: {otherPlayersCount}명");
    
            OnAssignSuccess?.Invoke(dto);
        }
        public event Action<MapInitDto> OnMapInitReceived;

        private void ParseMapInit(ReadOnlySpan<byte> span)
        {
            int offset = 0;
            MapInitDto dto = new MapInitDto();

            // 1. 오브젝트 총 개수 (2바이트)
            ushort objCount = BitConverter.ToUInt16(span.Slice(offset, 2)); 
            offset += 2;

            // 2. 오브젝트 매핑 정보 루프
            for (int i = 0; i < objCount; i++)
            {
                uint targetId = BitConverter.ToUInt32(span.Slice(offset, 4)); 
                offset += 4;
        
                ushort nameLen = BitConverter.ToUInt16(span.Slice(offset, 2)); 
                offset += 2;

                string name = string.Empty;
                if (nameLen > 0)
                {
                    name = Encoding.UTF8.GetString(span.Slice(offset, nameLen));
                    offset += nameLen;
                }

                dto.objectNameMappings.Add(targetId, name);
            }

            Debug.Log($"[MapInit] 맵 동기화 데이터 수신! 동적 오브젝트 {objCount}개 ID 매핑 완료.");
    
            // 이벤트 투척
            OnMapInitReceived?.Invoke(dto);
        }
        
        public event Action<GeneratePlayerDto> OnGeneratePlayerReceived;

        private void ParseGeneratePlayer(ReadOnlySpan<byte> span)
        {
            if (span.Length < 1) return;

            int offset = 0;
            byte playerCount = span[offset++];

            GeneratePlayerDto dto = new GeneratePlayerDto();

            for (int i = 0; i < playerCount; i++)
            {
                byte publicKey = span[offset++];
                byte team      = span[offset++];
                byte charId    = span[offset++];
                

                // spawnPos (float LE × 3)
                float posX = BitConverter.ToSingle(span.Slice(offset, 4)); offset += 4;
                float posY = BitConverter.ToSingle(span.Slice(offset, 4)); offset += 4;
                float posZ = BitConverter.ToSingle(span.Slice(offset, 4)); offset += 4;

                dto.players.Add(new GeneratePlayerEntry
                {
                    publicKey = publicKey,
                    team      = team,
                    charId    = charId,
                    spawnPos  = new Vector3(posX, posY, posZ)
                });
            }

            Debug.Log($"[GeneratePlayer] 플레이어 생성 데이터 수신! {playerCount}명");

            OnGeneratePlayerReceived?.Invoke(dto);
        }

        public event Action<RespawnPlayerDto> OnRespawnReceived;

        private void ParseRespawn(ReadOnlySpan<byte> span)
        {
            if (span.Length < 1) return;

            int offset = 0;
            byte playerCount = span[offset++];

            RespawnPlayerDto dto = new RespawnPlayerDto();

            for (int i = 0; i < playerCount; i++)
            {
                byte publicKey = span[offset++];

                float posX = BitConverter.ToSingle(span.Slice(offset, 4)); offset += 4;
                float posY = BitConverter.ToSingle(span.Slice(offset, 4)); offset += 4;
                float posZ = BitConverter.ToSingle(span.Slice(offset, 4)); offset += 4;

                dto.players.Add(new RespawnPlayerEntry
                {
                    publicKey = publicKey,
                    position  = new Vector3(posX, posY, posZ)
                });
            }

            OnRespawnReceived?.Invoke(dto);
        }

        public event Action<DeathDto> OnDeathReceived;

        private void ParseDeath(ReadOnlySpan<byte> span)
        {
            if (span.Length < 2) return;
            OnDeathReceived?.Invoke(new DeathDto
            {
                victimKey = span[0],
                killerKey = span[1],
            });
        }
        
        public event Action<byte, Vector3, Vector3, Vector3> OnPlayerMoveReceived;
        private void ParsePlayerMove(ReadOnlySpan<byte> span)
        {
            if (span.Length < 1) return;

            int offset = 0;
            byte playerCount = span[offset++];

            for (int i = 0; i < playerCount; i++)
            {
                byte publicKey = span[offset++];

                float posX = BitConverter.ToSingle(span.Slice(offset, 4)); offset += 4;
                float posY = BitConverter.ToSingle(span.Slice(offset, 4)); offset += 4;
                float posZ = BitConverter.ToSingle(span.Slice(offset, 4)); offset += 4;

                float pitch = BitConverter.ToSingle(span.Slice(offset, 4)); offset += 4; // rot.x
                float yaw   = BitConverter.ToSingle(span.Slice(offset, 4)); offset += 4; // rot.y
                offset += 4;                                                             // rot.z(=0)

                float velX = BitConverter.ToSingle(span.Slice(offset, 4)); offset += 4;
                float velY = BitConverter.ToSingle(span.Slice(offset, 4)); offset += 4;
                float velZ = BitConverter.ToSingle(span.Slice(offset, 4)); offset += 4;

                if (!InGameLogicStatic.IsInitialized) return;
                var player = InGameLogicStatic.Instance.GetPlayerByKey(publicKey);
                if (player == null) continue;
                OnPlayerMoveReceived?.Invoke(
                    publicKey,
                    new Vector3(posX, posY, posZ),
                    new Vector3(pitch, yaw, 0f),
                    new Vector3(velX, velY, velZ));
            }
        }
        public event Action<GetWeaponNotifyDto> OnWeaponPickup;
        private void ParseGetWeaponNotify(ReadOnlySpan<byte> span)
        {
            if (span.Length < 7) return;
            OnWeaponPickup?.Invoke(new GetWeaponNotifyDto
            {
                pickerKey      = span[0],
                weaponTargetId = BitConverter.ToUInt32(span.Slice(1, 4)),
                slot           = span[5],
                holdingSlot    = span[6],
            });
        }

        public event Action<DropWeaponNotifyDto> OnWeaponDrop;
        private void ParseDropWeaponNotify(ReadOnlySpan<byte> span)
        {
            if (span.Length < 18) return;
            OnWeaponDrop?.Invoke(new DropWeaponNotifyDto
            {
                dropperKey     = span[0],
                weaponTargetId = BitConverter.ToUInt32(span.Slice(1, 4)),
                position       = new Vector3(BitConverter.ToSingle(span.Slice(5, 4)),
                    BitConverter.ToSingle(span.Slice(9, 4)),
                    BitConverter.ToSingle(span.Slice(13, 4))),
                holdingSlot    = span[17],
            });
        }

        public event Action<SwapWeaponNotifyDto> OnWeaponSwap;
        private void ParseSwapWeaponNotify(ReadOnlySpan<byte> span)
        {
            if (span.Length < 2) return;
            OnWeaponSwap?.Invoke(new SwapWeaponNotifyDto { playerKey = span[0], holdingSlot = span[1] });
        }

        public event Action<ReloadNotifyDto> OnWeaponReload;
        private void ParseReloadNotify(ReadOnlySpan<byte> span)
        {
            if (span.Length < 4) return;
            OnWeaponReload?.Invoke(new ReloadNotifyDto
            {
                playerKey   = span[0],
                slot        = span[1],
                currentAmmo = BitConverter.ToUInt16(span.Slice(2, 2)),
            });
        }

        public event Action<ShotNotifyDto> OnShotFired;
        private void ParseShotNotify(ReadOnlySpan<byte> span)
        {
            if (span.Length < 25) return;
            OnShotFired?.Invoke(new ShotNotifyDto
            {
                playerKey = span[0],
                origin = new Vector3(BitConverter.ToSingle(span.Slice(1, 4)),
                    BitConverter.ToSingle(span.Slice(5, 4)),
                    BitConverter.ToSingle(span.Slice(9, 4))),
                dir = new Vector3(BitConverter.ToSingle(span.Slice(13, 4)),
                    BitConverter.ToSingle(span.Slice(17, 4)),
                    BitConverter.ToSingle(span.Slice(21, 4))),
            });
        }

        public event Action<HitDto> OnHit;
        private void ParseHitNotify(ReadOnlySpan<byte> span)
        {
            if (span.Length < 17) return;
            OnHit?.Invoke(new HitDto
            {
                victimKey   = span[0],
                attackerKey = span[1],
                hitPart     = span[2],
                remainingHp = BitConverter.ToUInt16(span.Slice(3, 2)),
                hitPosition = new Vector3(BitConverter.ToSingle(span.Slice(5, 4)),
                    BitConverter.ToSingle(span.Slice(9, 4)),
                    BitConverter.ToSingle(span.Slice(13, 4))),
            });
        }
        
        public event Action<uint, Vector3, Vector3> OnObjectMoveReceived;
        private void ParseObjectMove(ReadOnlySpan<byte> span)
        {
            if (span.Length < 1) return;

            int offset = 0;
            byte objectCount = span[offset++];
            if (span.Length < 1 + objectCount * 28) return;

            for (int i = 0; i < objectCount; i++)
            {
                uint targetId = BitConverter.ToUInt32(span.Slice(offset, 4)); offset += 4;

                float posX = BitConverter.ToSingle(span.Slice(offset, 4)); offset += 4;
                float posY = BitConverter.ToSingle(span.Slice(offset, 4)); offset += 4;
                float posZ = BitConverter.ToSingle(span.Slice(offset, 4)); offset += 4;

                float rotX = BitConverter.ToSingle(span.Slice(offset, 4)); offset += 4;
                float rotY = BitConverter.ToSingle(span.Slice(offset, 4)); offset += 4;
                float rotZ = BitConverter.ToSingle(span.Slice(offset, 4)); offset += 4;

                
                OnObjectMoveReceived?.Invoke(
                    targetId,
                    new Vector3(posX, posY, posZ),
                    new Vector3(rotX, rotY, rotZ));
            }
        }
        
        public event Action<uint, byte, Vector3> OnGenerateObjectReceived;

        private void ParseGenerateObject(ReadOnlySpan<byte> span)
        {
            if (span.Length < 17) return;
            uint targetId = BitConverter.ToUInt32(span.Slice(0, 4));
            byte prefabId = span[4];
            float x = BitConverter.ToSingle(span.Slice(5, 4));
            float y = BitConverter.ToSingle(span.Slice(9, 4));
            float z = BitConverter.ToSingle(span.Slice(13, 4));
            OnGenerateObjectReceived?.Invoke(targetId, prefabId, new Vector3(x, y, z));
        }

        public event Action<PhaseChangeNotifyDto> OnPhaseChanged;
        private void ParsePhaseChangeNotify(ReadOnlySpan<byte> span)
        {
            if (span.Length < 5) return;
            OnPhaseChanged?.Invoke(new PhaseChangeNotifyDto
            {
                phase    = span[0],
                duration = BitConverter.ToSingle(span.Slice(1, 4)),
            });
        }

        public event Action<GameEndNotifyDto> OnGameEnded;
        private void ParseGameEndNotify(ReadOnlySpan<byte> span)
        {
            if (span.Length < 1) return;
            OnGameEnded?.Invoke(new GameEndNotifyDto
            {
                winningTeam = span[0],
            });
        }

        public event Action<RoundEndNotifyDto> OnRoundEnded;
        private void ParseRoundEndNotify(ReadOnlySpan<byte> span)
        {
            if (span.Length < 2) return;
            OnRoundEnded?.Invoke(new RoundEndNotifyDto
            {
                winningTeam      = span[0],
                winningTeamScore = span[1],
            });
        }

        protected override void Initialize()
        {
        }
    }
    
    
}