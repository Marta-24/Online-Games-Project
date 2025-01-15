using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.IO;

namespace Scripts
{
    public enum ActionType
    {
        None = 0,
        Hello = 1,
        Position = 2,
        Create = 3,
        Damage = 4,
        ReadyToCreate = 6,
        ChangeLevel = 7,
        Confirmation = 8
    }
    struct int_
    {
        public int i;
        public int_(int i)
        {
            this.i = i;
        }
    }

    public class ParentPacket
    {
        public int id;
        public int i;
        public int netId;

        public ParentPacket()
        {       }

        public ParentPacket(int netId, int id)
        {
            this.netId = netId;
            this.id = id;
        }
    }

    public class MovementPacket : ParentPacket
    {
        public Vector2 position;
        public MovementPacket(int netId, Vector2 pos, int id)
        {
            this.netId = netId;
            this.position = pos;
            this.id = id;
        }
    }

    public class CreatePacket : ParentPacket
    {
        public Vector2 position;
        public Vector2 direction;
        public GameObjectType objType;
        public Vector3 rotation;
        public CreatePacket(int netId, Vector2 pos, Vector2 direction, Vector3 rotation, GameObjectType objType, int id)
        {
            this.netId = netId;
            this.position = pos;
            this.direction = direction;
            this.objType = objType;
            this.rotation = rotation;
            this.id = id;
        }
    }

    public class StringPacket : ParentPacket
    {
        public string str;

        public StringPacket(int netId, string str, int id)
        {
            this.netId = netId;
            this.str = str;
            this.id = id;
        }
    }

    public class IntPacket : ParentPacket
    {
        public int a;

        public IntPacket(int netId, int a, int id)
        {
            this.netId = netId;
            this.a = a;
            this.id = id;
        }
    }

    public class StartGamePacket : ParentPacket
    {
        public int a;
        public bool player;

        public StartGamePacket(int netId, int a, bool player, int id)
        {
            this.netId = netId;
            this.a = a;
            this.player = player;
            this.id = id;
        }
    }

     public class ConfirmationPacket : ParentPacket
    {
        ActionType act;
        public ConfirmationPacket(int id)
        {
            this.netId = 0;
            this.id = id;
        }
    }
    public class UserUDP
    {
        public EndPoint endPoint;
        public String name;
        public UserUDP(EndPoint endPoint, string name)
        {
            this.endPoint = endPoint;
            this.name = name;
        }
    }

    public class StringJson
    {
        public string value;
        public StringJson(string value)
        {
            this.value = value;
        }
    } 

    public class NetworkPackets : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}