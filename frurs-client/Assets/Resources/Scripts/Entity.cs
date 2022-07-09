using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DarkRift.Client.Unity;
using DarkRift;
using DarkRift.Client;

namespace FRURS.Client
{
    public class Entity
    {
        public GameObject obj;
        public Guid ID { get; }
        public Vector3 currentGoal { get; set; }
        public float speed { get; set; }
        public bool move { get; set; }

        public Entity(GameObject obj, Guid id) {
            this.obj = obj;
            this.ID = id;
            this.currentGoal = obj.transform.position;
            this.speed = 2.0f;
            this.move = false;
        }
    }
}
