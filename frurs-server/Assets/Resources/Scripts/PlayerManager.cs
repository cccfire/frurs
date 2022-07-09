using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DarkRift;
using DarkRift.Server;
using DarkRift.Server.Unity;

namespace FRURS.Server
{
    [RequireComponent(typeof(XmlUnityServer))]
    public class PlayerManager : MonoBehaviour
    {

        const float WIDTH = 10;
        DarkRiftServer server;
        Dictionary<IClient, Player> players = new Dictionary<IClient, Player>();        

        void Start()
        {
            this.server = GetComponent<XmlUnityServer>().Server; 
            server.ClientManager.ClientConnected += OnClientConnect;
            server.ClientManager.ClientDisconnected += OnClientDisconnect;
        }

        void OnClientConnect(object sender, ClientConnectedEventArgs e) 
        {
            Debug.Log("Client connected");

            System.Random r = new System.Random();
            float rx = (float) r.NextDouble() * WIDTH;
            float ry = 0.0f;
            float rz = (float) r.NextDouble() * WIDTH;
            Player newlyConnected = new Player(
                Instantiate(Resources.Load("Prefabs/NetworkPlayer"), new Vector3(rx, ry, rz), Quaternion.identity) as GameObject,
                Guid.NewGuid(),
                e.Client.ID
            );

            // Send Spawn Self packet
            using (DarkRiftWriter spawnSelfWriter = DarkRiftWriter.Create())
            {
                spawnSelfWriter.Write((byte) 0x03);
                spawnSelfWriter.Write((byte) 0x00);
                spawnSelfWriter.Write(newlyConnected.ID.ToByteArray());
                spawnSelfWriter.Write(newlyConnected.obj.transform.position.x);
                spawnSelfWriter.Write(newlyConnected.obj.transform.position.y);
                spawnSelfWriter.Write(newlyConnected.obj.transform.position.z);

                using (Message playerMessage = Message.Create(0, spawnSelfWriter))
                    e.Client.SendMessage(playerMessage, SendMode.Reliable);
            }
            
            // Send Spawn Entity packet
            using (DarkRiftWriter spawnPlayerWriter = DarkRiftWriter.Create())
            {
                spawnPlayerWriter.Write((byte) 0x03);
                spawnPlayerWriter.Write((byte) 0x01);
                spawnPlayerWriter.Write(newlyConnected.ID.ToByteArray());
                spawnPlayerWriter.Write(newlyConnected.obj.transform.position.x);
                spawnPlayerWriter.Write(newlyConnected.obj.transform.position.y);
                spawnPlayerWriter.Write(newlyConnected.obj.transform.position.z);
                using (Message spawnPlayerMessage = Message.Create(0, spawnPlayerWriter))
                {
                    foreach (IClient client in server.ClientManager.GetAllClients().Where(x => x != e.Client))
                        client.SendMessage(spawnPlayerMessage, SendMode.Reliable);
                }
            }

            players.Add(e.Client, newlyConnected);

            // Send Spawn Entities packet
            using (DarkRiftWriter playerWriter = DarkRiftWriter.Create())
            {
                playerWriter.Write((byte) 0x03);
                playerWriter.Write((byte) 0x02);
                foreach (KeyValuePair<IClient, Player> entry in players)
                {
                    Player player = entry.Value;
                    if (entry.Key != e.Client)
                    {
                        playerWriter.Write(player.ID.ToByteArray());
                        playerWriter.Write(player.obj.transform.position.x);
                        playerWriter.Write(player.obj.transform.position.y);
                        playerWriter.Write(player.obj.transform.position.z);
                    }
                }

                using (Message playerMessage = Message.Create(0, playerWriter))
                    e.Client.SendMessage(playerMessage, SendMode.Reliable);
            }

            e.Client.MessageReceived += HandlePackets;
        }

        void OnClientDisconnect(object sender, ClientDisconnectedEventArgs e)
        {
            Player disconnectedPlayer = players[e.Client];
            Destroy(disconnectedPlayer.obj);

            // Send Despawn Entity packet
            using (DarkRiftWriter disconnectWriter = DarkRiftWriter.Create())
            {
                disconnectWriter.Write((byte) 0x03);
                disconnectWriter.Write((byte) 0x03);
                disconnectWriter.Write(disconnectedPlayer.ID.ToByteArray());
                using (Message despawnPlayerMessage = Message.Create(0, disconnectWriter))
                {
                    foreach (IClient client in server.ClientManager.GetAllClients().Where(x => x != e.Client))
                        client.SendMessage(despawnPlayerMessage, SendMode.Reliable);
                }
            }

            players.Remove(e.Client);
        }
        
        // Handles packets
        void HandlePackets(object sender, MessageReceivedEventArgs e)
        {
            using (Message message = e.GetMessage())
            using (DarkRiftReader reader = message.GetReader())
            {
                byte packetType = reader.ReadByte();
                byte packetID = reader.ReadByte();

                // Debug.Log(packetType.ToString("x"));
                // Debug.Log(packetID.ToString("x"));

                switch (packetType)
                {
                    case 0x00:
                        break;
                    case 0x01:
                        break;
                    case 0x02:
                        break;
                    case 0x03:
                        break;
                    case 0x04:
                        switch (packetID)
                        {
                            case 0x00:
                                PlayerMovement(e, reader);
                                break;
                        }
                        break;
                    case 0x05:
                        break;
                    default:
                        break;
                }
            }
        }
        
        // Handles player movement packets
        void PlayerMovement(MessageReceivedEventArgs e, DarkRiftReader reader)
        {
            Player eventPlayer = players[e.Client];
            eventPlayer.obj.transform.rotation = new Quaternion(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
            if (reader.ReadBoolean()) 
            {
                eventPlayer.controller.Move(eventPlayer.obj.transform.forward * Time.fixedDeltaTime * eventPlayer.speed);
            }

            eventPlayer.tick = Math.Max(eventPlayer.tick, reader.ReadInt64());
        }

        void FixedUpdate()
        {

            // For every connected client, update their position and the position of other entities.
            foreach (KeyValuePair<IClient, Player> entry in players)
            {
                Player player = entry.Value;

                // Send Player Position packet
                using (DarkRiftWriter playerPositionWriter = DarkRiftWriter.Create())
                {
                    playerPositionWriter.Write((byte) 0x04);
                    playerPositionWriter.Write((byte) 0x00);
                    playerPositionWriter.Write(player.obj.transform.position.x);
                    playerPositionWriter.Write(player.obj.transform.position.y);
                    playerPositionWriter.Write(player.obj.transform.position.z);
                    playerPositionWriter.Write(player.obj.transform.rotation.x);
                    playerPositionWriter.Write(player.obj.transform.rotation.y);
                    playerPositionWriter.Write(player.obj.transform.rotation.z);
                    playerPositionWriter.Write(player.obj.transform.rotation.w);
                    playerPositionWriter.Write(player.tick);

                    using (Message playerMessage = Message.Create(0, playerPositionWriter))
                        entry.Key.SendMessage(playerMessage, SendMode.Unreliable);
                }
                
                // Send Entity Position packet
                using (DarkRiftWriter entityPositionWriter = DarkRiftWriter.Create())
                {
                    entityPositionWriter.Write((byte) 0x04);
                    entityPositionWriter.Write((byte) 0x01);
                    entityPositionWriter.Write(player.ID.ToByteArray());
                    entityPositionWriter.Write(player.obj.transform.position.x);
                    entityPositionWriter.Write(player.obj.transform.position.y);
                    entityPositionWriter.Write(player.obj.transform.position.z);
                    entityPositionWriter.Write(player.obj.transform.rotation.x);
                    entityPositionWriter.Write(player.obj.transform.rotation.y);
                    entityPositionWriter.Write(player.obj.transform.rotation.z);
                    entityPositionWriter.Write(player.obj.transform.rotation.w);
                    using (Message entityPositionMessage = Message.Create(0, entityPositionWriter))
                    {
                        foreach (IClient client in server.ClientManager.GetAllClients().Where(x => x != entry.Key))
                            client.SendMessage(entityPositionMessage, SendMode.Unreliable);
                    }
                }
            }
        }
    }
}
