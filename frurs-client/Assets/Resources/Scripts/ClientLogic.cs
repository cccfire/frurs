using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DarkRift.Client.Unity;
using DarkRift;
using DarkRift.Client;

namespace FRURS.Client
{
    public class ClientLogic : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("The DarkRift client to communicate on.")]
        UnityClient client;

        [SerializeField]
        [Tooltip("The controllable player prefab.")]
        GameObject controllablePrefab;

        [SerializeField]
        [Tooltip("The network controllable player prefab.")]
        GameObject networkPrefab;
        
        private GameObject self;
        private Guid selfID;

        public Dictionary<Guid, Entity> entities = new Dictionary<Guid, Entity>();

        void Awake()
        {
            if (client == null)
            {
                Debug.LogError("Client unassigned in PlayerSpawner.");
                Application.Quit();
            }

            if (controllablePrefab == null)
            {
                Debug.LogError("Controllable Prefab unassigned in PlayerSpawner.");
                Application.Quit();
            }

            if (networkPrefab == null)
            {
                Debug.LogError("Network Prefab unassigned in PlayerSpawner.");
                Application.Quit();
            }

            client.MessageReceived += HandlePackets;
        }

        void HandlePackets(object sender, MessageReceivedEventArgs e)
        {
            //Debug.Log("Packet Received");

            using (Message message = e.GetMessage())
            using (DarkRiftReader reader = message.GetReader())
            {
                byte packetType = reader.ReadByte();
                byte packetID = reader.ReadByte();

                //Debug.Log(packetType.ToString("x"));
                //Debug.Log(packetID.ToString("x"));
                switch(packetType) 
                {
                    case 0x00:
                        break;
                    case 0x01:
                        break;
                    case 0x02:
                        break;
                    case 0x03:
                        switch(packetID)
                        {
                            case 0x00:
                                SpawnSelf(reader);
                                break;

                            case 0x01:
                                SpawnEntity(reader);
                                break;

                            case 0x02:
                                SpawnEntities(reader);
                                break;

                            case 0x03:
                                DespawnEntity(reader);
                                break;

                            default:
                                break;
                        }
                        break;
                    case 0x04:
                        switch(packetID)
                        {
                            case 0x00:
                                PlayerPosition(reader);
                                break;
                            
                            case 0x01:
                                EntityPosition(reader);
                                break;

                            default:
                                break;
                        }
                        break;
                    default:
                        break;
                }
            }
        }

        /**
        * Handles packet for spawning self.
        * @param reader the reader handling the packet message. 
        */
        void SpawnSelf(DarkRiftReader reader) {
            Debug.Log("Spawning Self");
            Guid id = new Guid(reader.ReadBytes());
            Vector3 position = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());

            GameObject obj;
            obj = Instantiate(controllablePrefab, position, Quaternion.identity) as GameObject;

            selfID = id;
            self = obj;
        }

        /**
        * Handles packet for spawning an entity.
        * @param reader the reader handling the packet message. 
        */
        void SpawnEntity(DarkRiftReader reader) {
            Debug.Log("Spawning Entity");
            Guid id = new Guid(reader.ReadBytes());
            Vector3 position = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());

            GameObject obj;
            obj = Instantiate(networkPrefab, position, Quaternion.identity) as GameObject;
            Entity entity = new Entity(obj, id);
            entities.Add(id, entity);
        }

        /**
        * Handles packet for spawning multiple other entities.
        * @param reader the reader handling the packet message. 
        */
        void SpawnEntities(DarkRiftReader reader) {
            Debug.Log("Spawning Entities");
            while (reader.Position < reader.Length)
            {
                Guid id = new Guid(reader.ReadBytes());
                Vector3 position = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
    
                GameObject obj;
                obj = Instantiate(networkPrefab, position, Quaternion.identity) as GameObject;
                Entity entity = new Entity(obj, id);
                entities.Add(id, entity);
            }
        }

        /**
        * Handles packet for spawning an entity.
        * @param reader the reader handling the packet message. 
        */
        void DespawnEntity(DarkRiftReader reader) {
            Debug.Log("Despawning Entity");
            Guid id = new Guid(reader.ReadBytes());

            Destroy(entities[id].obj);
            entities.Remove(id);
        }
        
        /**
        * Handles packet for updating an entity's position.
        * @param reader the reader handling the packet message.
        */
        void EntityPosition(DarkRiftReader reader) {
            Guid id = new Guid(reader.ReadBytes());
            if (!entities.ContainsKey(id))
                return;
            Vector3 position = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
            Quaternion quat = new Quaternion(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
            entities[id].currentGoal = position;
            entities[id].obj.transform.rotation = quat;
            
        }

        /**
        * Handles packet for updating this player's position.
        * @param reader the reader handling the packet message.
        */
        void PlayerPosition(DarkRiftReader reader) {
            Vector3 position = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
            Quaternion quat = new Quaternion(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
            QueuedMove qmove = new QueuedMove(position, quat, reader.ReadInt64());
            PlayerMovement movement = self.GetComponent<PlayerMovement>();
            QueuedMove currentq = movement.serverAuthority;
            if (currentq == null || currentq.tick < qmove.tick)
            {
                movement.serverAuthority = qmove;
            }
        }
    }
}
