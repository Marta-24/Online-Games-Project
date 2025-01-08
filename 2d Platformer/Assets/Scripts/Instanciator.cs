using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace Scripts
{
    public class FutureInstance
    {
        public GameObject obj;
        public GameObject parent;
        public Vector3 pos;
        public string user;

        public FutureInstance()
        { }
        public FutureInstance(GameObject obj, Vector3 pos)
        {
            this.obj = obj;
            this.parent = null;
            this.pos = pos;
        }

        public FutureInstance(GameObject obj, GameObject parent, Vector3 pos)
        {
            this.obj = obj;
            this.parent = parent;
            this.pos = pos;
        }
    }

    public class FuturePanelUser : FutureInstance
    {


        public FuturePanelUser(GameObject obj, GameObject parent, string user, Vector3 pos)
        {
            this.obj = obj;
            this.parent = parent;
            this.user = user;
            this.pos = pos;
        }
    }

    public class Instanciator : MonoBehaviour
    {
        public GameObject player1Prefab;
        public GameObject player2Prefab;
        public GameObject enemyPrefab;
        public GameObject enemyFlyPrefab;
        public GameObject UserPrefab;
        public List<FutureInstance> instances;
        // Start is called before the first frame update
        void Start()
        {
            instances = new List<FutureInstance>();
        }

        // Update is called once per frame
        void Update()
        {
            if (instances.Count != 0)
            {
                foreach (FutureInstance instance in instances)
                {
                    CreateInstance(instance);
                }
                instances.Clear();
            }
        }


        public void CreateInstance(FutureInstance instance)
        {
            var obj = Instantiate(instance.obj, instance.pos, Quaternion.identity);
            if (instance.parent != null)
            {
                obj.transform.SetParent(instance.parent.transform);
                var children = obj.GetComponentsInChildren<Transform>();
                foreach (var child in children)
                {
                    if (child.name == "Text (TMP)")
                    {
                        TMP_Text text = child.GetComponent<TMP_Text>();
                        text.SetText(instance.user);
                    }
                }
            }
        }

        public GameObject InstancePlayerOne()
        {
            GameObject obj = Instantiate(player1Prefab, new Vector3(0, 0, 0), Quaternion.identity);
            obj.name = "Player1";
            return obj;
        }

        public GameObject InstancePlayerTwo()
        {
            GameObject obj = Instantiate(player2Prefab, new Vector3(0, 0, 0), Quaternion.identity);
            obj.name = "Player2";
            return obj;
        }

        public GameObject InstanceEnemyPrefab(Vector2 pos)
        {
            return Instantiate(enemyPrefab, new Vector3(pos.x, pos.y, 0.0f), Quaternion.identity);
        }

        public GameObject InstanceEnemyFlyPrefab(Vector2 pos)
        {
            return Instantiate(enemyFlyPrefab, new Vector3(pos.x, pos.y, 0.0f), Quaternion.identity);
        }

        public GameObject IntanceUserPrefab(GameObject parent, string name)
        {
            Debug.Log("adding new user instanciator!!!");
            FuturePanelUser ins = new FuturePanelUser(UserPrefab, parent, name, new Vector3(0, 0, 0));
            instances.Add(ins);
            return null;
        }
    }
}