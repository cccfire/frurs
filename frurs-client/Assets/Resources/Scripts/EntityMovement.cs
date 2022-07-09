using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FRURS.Client
{
    public class EntityMovement : MonoBehaviour
    {
        ClientLogic logic;

        void Start()
        {
            logic = gameObject.GetComponent<ClientLogic>();
        }

        void FixedUpdate()
        {
            foreach (Entity e in logic.entities.Values)
            {
                e.obj.GetComponent<CharacterController>()
                    .Move((Vector3.MoveTowards(e.obj.transform.position, e.currentGoal, 1) - e.obj.transform.position) * e.speed * Time.fixedDeltaTime);
            }
        }
    }
}
