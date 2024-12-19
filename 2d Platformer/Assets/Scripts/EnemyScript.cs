using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine;

public class EnemyScript : MonoBehaviour
{
    float laserLength = 1f;

    // Velocidad del enemigo
    public float speed;

    // Dirección de movimiento: -1 para izquierda, 1 para derecha
    public int movementDirection = 1;

    // Referencia al Rigidbody2D
    private Rigidbody2D rb;
    void Start()
    {
        // Obtén el componente Rigidbody2D
        rb = GetComponent<Rigidbody2D>();
        speed = 0.05f;
    }

    // Update is called once per frame
    void Update()
    {
        CheckPosition();
        MoveEnemy();
    }

    void MoveEnemy()
    {
        float movement = movementDirection * speed;

        rb.MovePosition(new Vector2(transform.position.x + movement, transform.position.y));
    }

    void CheckPosition()
    {
        Vector3 vec = transform.position + (new Vector3(0.6f, 0, 0)* movementDirection);

        RaycastHit2D[] hit = Physics2D.RaycastAll(vec, Vector2.down, laserLength);
        Vector3 vec3 = new Vector3(0, -1, 0);

        Debug.DrawLine(vec, vec + vec3);
        Debug.DrawLine(transform.position, new Vector3(0, 0, 0));

        bool hittingGround = false;
        foreach (RaycastHit2D hit2d in hit)
        {
            if (hit2d.collider.tag == "Wall") hittingGround = true;
            //Hit something, print the tag of the object
            
        }

        
        if (hittingGround == false)
        {
            FlipDirection();
        }
    }

    void FlipDirection()
    {
        movementDirection *= -1;
    }
}
