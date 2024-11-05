using UnityEngine;

public class CloudMove : MonoBehaviour
{
    public float speed = 1f;
    public float resetPosition = -10f;
    public float startPosition = 10f;

    void Update()
    {
        transform.Translate(Vector2.left * speed * Time.deltaTime);

        if (transform.position.x <= resetPosition)
        {
            transform.position = new Vector2(startPosition, transform.position.y);
        }
    }
}
