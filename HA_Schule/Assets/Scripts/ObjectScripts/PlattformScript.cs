using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Plattformen bewegen sich in eine Richtung, bis sie gegen ein Objekt stoßen und sich dann in die andere Richtung bewegen
/// </summary>
public class PlattformScript : MovementScript
{
    public Vector2 movingDirection;
    private GameObject cog;
    private void Start()
    {
        rb.bodyType = RigidbodyType2D.Kinematic;
        prePos = (Vector2)transform.position + Vector2.one;
        properties.size = transform.localScale.x / 2;
        cog = transform.GetChild(0).gameObject;
    }


    new public void FixedUpdate() { }


    public void Update()//wird überschrieben
    {
        if (GameManager.block) return;

        transform.localScale = (Vector3)Vector2.one * properties.size * 2;
        prePos = transform.position;

        properties.velocity += properties.acceleration / 5;
        rb.velocity = properties.velocity * movingDirection;
        cog.transform.eulerAngles += Vector3.forward * properties.velocity * 2;
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Ground"))
        {
            properties.velocity *= -1;
            rb.position += 0.25f * properties.velocity * movingDirection;

        }
    }
}
