using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FRURS.Client
{
    public class QueuedMove
    {
        public Vector3 position;
        public Quaternion quat;
        public long tick;

        public QueuedMove(Vector3 position, Quaternion quat, long tick)
        {
            this.position = position;
            this.quat = quat;
            this.tick = tick;
        }
    }
}
