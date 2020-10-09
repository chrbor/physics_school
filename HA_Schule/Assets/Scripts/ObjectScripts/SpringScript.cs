using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Script, das die Sprungfeder im Coregame beschreibt
/// </summary>
public class SpringScript : ObjectScript
{
    private SpringJoint2D spring;
    private Transform spring_Transform;
    private float prevHeight;
    private float groundHeight;
    private float factor;

    // Start is called before the first frame update
    void Awake()
    {
        spring_Transform = transform.Find("Spring");
        spring = spring_Transform.GetComponent<SpringJoint2D>();
        properties.size = transform.localScale.x / 2;
        groundHeight = transform.position.y + (transform.eulerAngles.z == 0 ? -1 : 1) * spring_Transform.localScale.x * transform.localScale.x * 2;
        spring.distance = properties.size * spring_Transform.localScale.x * 4;
        factor = (transform.eulerAngles.z == 0 ? 0.25f : -0.25f);

        if (spring == null || spring_Transform == null)
            Destroy(transform.parent.gameObject);
    }

    private void Update()
    {
        //skalierung der Feder:
        spring.frequency = new Vector2(properties.force_x, properties.force_y).magnitude;
        transform.localScale = Vector2.one * properties.size * 2;
        spring_Transform.localScale = new Vector3(factor * (transform.position.y-groundHeight), 0.5f)/(properties.size);//transform.localPosition.y * 0.0f);
        spring_Transform.position = new Vector2(spring_Transform.position.x, groundHeight);
    }

    new public void FixedUpdate() { }//Überschreibe das alte Update, damit die Steuerung von Movementscript nich ausgeführt wird

    override public bool changeSize (float change)
    {
        properties.size += change;
        transform.localScale = (Vector3)Vector2.one * properties.size * 2;
        transform.GetChild(0).localScale = (Vector3)Vector2.one / transform.localScale.x; 
        return true;
    }
    override public bool changeForce (float change){ return true; }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        //Zurücksetzen von prevHeight:
        if (collision.gameObject.CompareTag("Player"))
            prevHeight = transform.position.y;
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        //Aufaddierung der Kraft wenn Spieler sich auf der Feder befindet
        if(collision.gameObject.CompareTag("Player"))
        {
            if (prevHeight < transform.position.y)
                collision.gameObject.GetComponent<Rigidbody2D>().AddForce(spring.reactionForce);
            prevHeight = transform.position.y;
        }
    }
}
