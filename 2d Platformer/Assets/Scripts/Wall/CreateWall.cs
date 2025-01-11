using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Scripts
{
    public class CreateWall : MonoBehaviour
    {
        Instanciator instanciator_;
        NetIdManager manager;
        int Cooldown = 0;
        public bool isHorizontal;
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            if (instanciator_ == null) FindInstanciator();


            if (Cooldown == 0)
            {
                if (Input.GetButtonDown("k"))
                {
                    SpawnWall();
                    Cooldown = 24 * 5;
                }
            }
            else if (Cooldown > 0)
            {
                Cooldown--;
            }
            else if (Cooldown < 0)
            {
                Cooldown = 0;
            }
        }

        void SpawnWall()
        {
            Vector3 vec = new Vector3(0.0f, 0.0f, 0.0f);
            if (isHorizontal)
            {
                vec.z = 90.0f;
            }
            
            Vector3 vec3 = gameObject.transform.position;
            GameObject obj = instanciator_.InstanceWall(new Vector2(vec3.x + 2.0f, vec3.y), vec);
            NetId id = manager.CreateNetId(obj, GameObjectType.wall);
            manager.SendObject(id);
        }

        void FindInstanciator()
        {
            GameObject obj = GameObject.Find("NetIdManager");
            manager = obj.GetComponent<NetIdManager>();
            GameObject objOnline = GameObject.Find("ClientManager");
            if (objOnline == null) objOnline = GameObject.Find("ServerManager");
            instanciator_ = objOnline.GetComponent<Instanciator>();
        }
    }
}