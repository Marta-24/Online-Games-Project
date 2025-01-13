using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Scripts
{

    public class ChangeLevel : MonoBehaviour
    {
        public int nextlevel;
        public SceneLoader lvlManager;
        // Start is called before the first frame update
        void Start()
        {
            FindLevelManager();
        }

        // Update is called once per frame
        void Update()
        {
            if (lvlManager == null)
            {
                FindLevelManager();
            }
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            Debug.Log("is working");
            if (other.CompareTag("Player1") || other.CompareTag("Player2"))
            {
                Debug.Log("CHANGING SCENE");
                lvlManager.ChangeToLevel(nextlevel, true);
            }

        }

        void FindLevelManager()
        {
            GameObject obj = GameObject.Find("SceneLoader");
            lvlManager = obj.GetComponent<SceneLoader>();
        }
    }
}