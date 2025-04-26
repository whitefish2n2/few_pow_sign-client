using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Codes.InGame.Player_Ingame;
using enet;

public unsafe class EnetClient : MonoBehaviour
{
    private enet.ENetHost* client;
    private enet.ENetPeer* server;
    private bool isConnected = false;
    private Coroutine currentRoutine;

    void Start()
    {
        unsafe
        {
            try
            {
                enet.ENet.enet_initialize();
                Debug.Log("Initialized EnetClient");
            }
            catch (Exception e)
            {
                Debug.Log(ENet.Library.version);
                Debug.Log(e);
            }
            
            client = enet.ENet.enet_host_create(null, 1, 2, 0, 0);
            ENetAddress address = new ENetAddress();
            if (client == null)
            {
                Debug.LogError("client not created");
            }
            enet.ENet.enet_address_set_host_ip(&address, "127.0.0.1");
            address.port = 7777;
            Debug.LogError((*client).serviceTime);
            //Address address = new Address();  
            
            server = enet.ENet.enet_host_connect(client,&address,1,2);
            
            Debug.Log("Server State:" + server->state.ToString());
            currentRoutine = StartCoroutine(Tick());
            Debug.Log("try to connect on " + server->host->ToString() + ":"+address.host.ipv4->ToString() + ":" + address.port.ToString());
        }
    }

    void Update()
    {
        
        Debug.Log(server->state.ToString());
        if (client == null) return;
        ENetEvent netEvent;
        while (enet.ENet.enet_host_service(client,&netEvent,1000)  > 0)
        {
            Debug.Log($"[ENet] Event: {netEvent.type}, State: {server->state}");
            switch (netEvent.type)
            {
                case ENetEventType.ENET_EVENT_TYPE_CONNECT:
                    Debug.Log("ENet Connected");
                    isConnected = true;
                    break;

                case ENetEventType.ENET_EVENT_TYPE_RECEIVE:
                    byte[] data = new byte[netEvent.packet->dataLength];
                    Debug.Log("ENet Received: " + data);
                    enet.ENet.enet_host_flush(client);
                    break;
                case ENetEventType.ENET_EVENT_TYPE_DISCONNECT:
                    Debug.Log(("DisConnected"));
                    isConnected = false;
                    break;
            }
        }
    }
    

    IEnumerator Tick()
    {
        WaitForSeconds wait = new WaitForSeconds(0.03f);

        while (true)
        {
            yield return wait;
            //Debug.Log("tick");
            if (!isConnected) continue;
            _SendUnsafePacket();
        }
    }

    private unsafe void _SendUnsafePacket()
    {
        byte[] msg = StaticInput.instance.GetInputBinary();
        fixed (byte* ptr = msg)
        {
            ENetPacket* packet =  enet.ENet.enet_packet_create(ptr,(nuint)(msg.Length+1),(int)enet.ENetPacketFlag.ENET_PACKET_FLAG_UNSEQUENCED);
            
            enet.ENet.enet_peer_send(server,0, packet);
        }
        
    }

    void OnApplicationQuit()
    {
        enet.ENet.enet_host_destroy(client);
        enet.ENet.enet_deinitialize();
    }
}

