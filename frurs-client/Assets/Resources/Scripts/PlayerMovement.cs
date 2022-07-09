using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DarkRift;
using DarkRift.Client;
using DarkRift.Client.Unity;

namespace FRURS.Client
{
    public class PlayerMovement : MonoBehaviour
    {
        UnityClient client;

        private const int bufferSize = 1000;

        private float playerSpeed = 2.0f;
        private float rotateSpeed = 30.0f;
        private CharacterController controller;
        private long tick;
        private bool forwards = true;

        public QueuedMove serverAuthority = null; 
        
        struct Move
        {
            public Vector3 direction;
            public bool move;

            public Move(Vector3 direction, bool move)
            {
                this.direction = direction;
                this.move = move;
            }
        }

        private Move[] moveBuffer = new Move[bufferSize];

        // Start is called before the first frame update
        void Start()
        {
            client = GameObject.Find("Network").GetComponent<UnityClient>();
            tick = 0;
            controller = gameObject.AddComponent<CharacterController>(); 
        }

        void FixedUpdate()
        {
            Vector3 currentMove = Vector3.zero;

            if (serverAuthority != null)
            {
                transform.position = serverAuthority.position;
                //Debug.Log("current tick: " + tick + "\n" + "server tick: " + serverAuthority.tick);
                for (long i = serverAuthority.tick + 1; i < tick + 1; i++)
                {
                    Move imove = moveBuffer[i % bufferSize];
                    currentMove = currentMove + imove.direction * Time.fixedDeltaTime * Convert.ToByte(imove.move);
                }
                serverAuthority = null;
            } 
            else
            {
                Move imove = moveBuffer[tick % bufferSize];
                currentMove = currentMove + imove.direction * Time.fixedDeltaTime * Convert.ToByte(imove.move);
            }

            tick++;

            bool move = false;
            if (Input.GetKey(KeyCode.W))
            {
                if (!forwards)
                {
                    transform.Rotate(new Vector3(0, 1, 0) * 180);
                    forwards = true;
                }
                //controller.Move(currentMove + gameObject.transform.forward * Time.fixedDeltaTime * playerSpeed);
                move = true;
            }

            else if (Input.GetKey(KeyCode.S))
            {
                if (forwards)
                {
                    transform.Rotate(new Vector3(0, 1, 0) * 180);
                    forwards = false;
                }
                //controller.Move(currentMove + gameObject.transform.forward * Time.fixedDeltaTime * playerSpeed);
                move = true;
            }

            if (Input.GetKey(KeyCode.D))
            {
                //Rotate the sprite about the Y axis in the positive direction
                Vector3 vec = new Vector3(0, 1, 0);
                if (!forwards)
                    vec = new Vector3(0, -1, 0);
                transform.Rotate(vec * Time.fixedDeltaTime * rotateSpeed, Space.World);
            }

            if (Input.GetKey(KeyCode.A))
            {
                //Rotate the sprite about the Y axis in the negative direction
                Vector3 vec = new Vector3(0, -1, 0);
                if (!forwards)
                    vec = new Vector3(0, 1, 0);
                transform.Rotate(vec * Time.fixedDeltaTime * rotateSpeed, Space.World);
            }

            using (DarkRiftWriter writer = DarkRiftWriter.Create())
            {
                writer.Write((byte) 0x04);
                writer.Write((byte) 0x00);
                writer.Write(gameObject.transform.rotation.x);
                writer.Write(gameObject.transform.rotation.y);
                writer.Write(gameObject.transform.rotation.z);
                writer.Write(gameObject.transform.rotation.w);
                writer.Write(move);
                writer.Write(tick);

                using (Message message = Message.Create(0, writer))
                    client.SendMessage(message, SendMode.Unreliable);
            } 
            
            moveBuffer[tick % bufferSize] = new Move(gameObject.transform.forward * playerSpeed, move);    
        }
    }
}
