using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Scripts
{
    public class Instanciator : MonoBehaviour
    {
        public GameObject playerPrefab;
        public GameObject camera;
        // Start is called before the first frame update
        void Start()
        {
        }

        // Update is called once per frame
        void Update()
        {

        }

        public GameObject InstancePlayerOne()
        {
            /*GameObject player = new GameObject();
            player.name = "Player1_";
            
            SpriteRenderer renderer = player.GetComponent<SpriteRenderer>();
            renderer.
            player.GetComponent<PlayerMovement>();
            player.GetComponent<Rigidbody2D>();
            player.GetComponent<BoxCollider2D>();
            player.GetComponent<Animator>();
            return player;*/
            Instantiate(playerPrefab, new Vector3(0, 0, 0), Quaternion.identity);
            camera.GetComponent<CameraFollow>().ChangeTarget(playerPrefab.transform);
            return playerPrefab;
        }

        public GameObject InstancePlayerTwo()
        {
            GameObject instance = new GameObject();
            return instance;
        }
    }
}