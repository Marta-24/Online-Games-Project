using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanelOrder : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        float pos = 0.0f;
        foreach (Transform child in transform)
        {
            
            child.gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(120.0f, 200.0f + pos);
            pos -= 100.0f;
        }
        //foreach (GameObject obj in gameObject)
        //{
        //    obj.transform.position = transform.position + new Vector2(3.0f, 3.0f, 0, 0f);
        //}
    }
}
