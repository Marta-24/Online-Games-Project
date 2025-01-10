using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class wallTime : MonoBehaviour
{
    public int lifeTime;
    // Start is called before the first frame update
    void Start()
    {
        lifeTime = 24 * 3 * 5; // 3 seconds?
    }

    // Update is called once per frame
    void Update()
    {
        lifeTime--;

        if (lifeTime <= 0)
        {
            Destroy(gameObject);
        }
    }
}
