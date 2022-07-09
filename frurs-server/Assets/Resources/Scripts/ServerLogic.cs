using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DarkRift;
using DarkRift.Server;
using DarkRift.Server.Unity;

namespace FRURS.Server
{
    [RequireComponent(typeof(XmlUnityServer))]
    [RequireComponent(typeof(PlayerManager))]
    public class ServerLogic : MonoBehaviour
    {
        XmlUnityServer xmlServer;
        DarkRiftServer server;
        PlayerManager playerManager;

        void Start()
        {
            xmlServer = GetComponent<XmlUnityServer>();
            server = xmlServer.Server;
            playerManager = GetComponent<PlayerManager>();
        }
    }
}
