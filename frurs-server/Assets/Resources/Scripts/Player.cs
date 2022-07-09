using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FRURS.Server
{
    public class Player
    {
        public ushort clientID { get; set; }
        public Guid ID { get; set; }
        public float speed { get; set; }
        public long tick { get; set; }
        public GameObject obj;
        public CharacterController controller;

        public Player(GameObject obj, Guid id, ushort clientID)
        {
            this.speed = 2.0f;
            this.obj = obj;
            this.ID = id;
            this.tick = 0;
            this.clientID = clientID;
            this.controller = obj.GetComponent<CharacterController>();
        }
    } 
}
