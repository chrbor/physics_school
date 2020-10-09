using System.Collections;
using System.Collections.Generic;
using System;
using Random = UnityEngine.Random;
using UnityEngine;
using MyBox;

/// <summary>
/// MovementScript ist eine Klasse, die die Bewegung des Gameobjektes und dessen Eigenschaften steuert 
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class MovementScript : MonoBehaviour
{
    /// <summary>
    /// Die Struktur Properties beinhaltet alle maximalen Werte, die das der Spieler erreichen kann 
    /// </summary>
    [Serializable]
    public class Properties
    {
        public bool using_kineticEnergy;
        public bool using_potentialEnergy;
        public bool using_puls;
        public bool using_force;
        [ConditionalField("using_force")]
        public float force_x;       //unterscheidung von Anziehung, Abstoßung, Machtblitze etc. je nach Objekt
        [ConditionalField("using_force")]
        public float force_y;

        [Header("active Object:")]
        public bool using_mass;
        [ConditionalField("using_mass")]
        public float mass;

        public bool using_velocity;
        [ConditionalField("using_velocity")]
        public float velocity;       //kann auch Drehgeschwindigkeit sein (je nach Objekt))

        public bool using_velocity_Damping;
        [ConditionalField("using_velocity_damping")]
        public float velocity_Damping;

        public bool using_acceleration;
        [ConditionalField("using_acceleration")]
        public float acceleration;

        public bool using_jumpForce;
        [ConditionalField("using_jumpForce")]
        public float jumpForce;

        public bool using_time;
        [ConditionalField("using_time")]
        public float time;

        [Header("passive Object:")]
        public bool using_initialDrag;
        [ConditionalField("using_initialDrag")]
        public float initialDrag;

        public bool using_elasticity;
        [ConditionalField("using_elasticity")]
        public float elasticity;

        public bool using_current;
        [ConditionalField("using_current")]
        public float current;   //elekt. Widerstand

        public bool using_luminosity;
        [ConditionalField("using_luminosity")]
        public float luminosity;    //beleuchtet dunkle orte

        public bool using_friction;
        [ConditionalField("using_friction")]
        public float friction;

        public bool using_temperature;
        [ConditionalField("using_temperature")]
        public float temperature;   //kann zu Statusänderung eines Objektes führen (z.B. Wasser -> Dampf)

        public bool using_temp_damping;
        [ConditionalField("using_temp_damping")]
        public float temp_damping;  //wie gut der Körper Wärme leiten kann

        public bool using_frequency;
        [ConditionalField("using_frequency")]
        public float frequency;     //für periodische Abläufe wie z.B. Pendel

        public bool using_size;
        [ConditionalField("using_size")]
        public float size;

        private float density;
    }
    [SerializeField]
    public Properties properties;

    [HideInInspector]
    public bool bottom, left_down, right_down;//Trigger , die registrieren, ob sich das Objekt auf dem Boden oder an einer Kante befindet
    protected GameObject trigger;
    [HideInInspector]
    public Rigidbody2D rb;
    private Animator[] anim = new Animator[2];

    //Helfer:
    [HideInInspector]
    public Vector2 prePos;//vorherige Position
    [HideInInspector]
    public Vector2 preVel;//vorherige Geschwindigkeit, wird verwendet, damit beim Spawn die Objekte nicht schweben
    protected float momentum;   //preserved velocity,  enables to exceed the properties
    public float jumpCount;
    private int count;
    private bool isObject;
    private bool isSquishing;

    [HideInInspector]
    public int mask;        //Layermask für den Raycast

    protected CapsuleCollider2D mainCol;

    public class TransformSaved
    {
        public TransformSaved(Vector2 _position, Quaternion _rotation, Vector2 _scale)
        {
            position = _position;
            rotation = _rotation;
            scale = _scale;
        }

        public Vector2 position;
        public Quaternion rotation;
        public Vector2 scale;
    }
    public List<TransformSaved> pastPositions = new List<TransformSaved>();

    /// <summary>
    /// Awake weißt alle Objekte des Gameobjekts den Variablen zu.
    /// </summary>
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        mainCol = GetComponent<CapsuleCollider2D>();
        isSquishing = false;
        prePos = transform.position;
        preVel = new Vector2();
        //Layermask für den Raycast:
        mask = 0b1;// | (0b1 << 11);//Layer "default" + "object"

        if (isObject = CompareTag("Object"))
            return;

        anim[0] = transform.GetChild(0).GetComponent<Animator>();
        anim[1] = transform.GetChild(1).GetComponent<Animator>();
        if (!CompareTag("Player"))
        {
            anim[0].SetTrigger("Blink");
            anim[1].SetTrigger("Blink");
            StartCoroutine(Blink());
        }

        trigger = transform.GetChild(2).gameObject;
    }

    public void FixedUpdate()
    {
        if (GameManager.block || rb.bodyType != RigidbodyType2D.Dynamic) return;
        if (Mathf.Abs(rb.velocity.x) < 10e-8f)//verhindere Gleiten
            rb.velocity *= Vector2.up;

        //Debug.Log("object: " + Mathf.Abs(rb.velocity.x - x_prevel));
        //Hürde, um Objekt in Bewegung zu bringen:
        if (momentum != 0)
        {
            if (Mathf.Abs(rb.velocity.magnitude) < 10e-8f && preVel.magnitude < 10e-8f)
                momentum = 0;
        }
        else if (Mathf.Abs(rb.velocity.magnitude) < properties.initialDrag && preVel.magnitude < 10e-8f)
        {
            rb.velocity = Vector2.zero;//Vector2.up;
            transform.position = new Vector3(prePos.x, transform.position.y);
        }
        else
            momentum = 1;

        //geschw. und position des letzten Zyklus aufnehmen:
        prePos = transform.position;
        preVel = rb.velocity;

        //Update der Properties:
        //rb.mass = properties.mass;
        //transform.localScale = properties.size * Vector2.one;

        //Gravitation wirken lassen:
        if(properties.using_force && rb.bodyType == RigidbodyType2D.Dynamic) rb.velocity += new Vector2(properties.force_x, properties.force_y) / 20;
        return;

    }

    public float GetHeight()
    {
        RaycastHit2D rayHit = Physics2D.Raycast((Vector2)transform.position, Vector2.down, 20, mask);
        return rayHit.collider ? rayHit.distance : 20f;
    }

    public virtual bool changeSize(float change) { return false; }
    public virtual bool changeForce(float change) { return false; }

    /// <summary>
    /// EingangsAnimation, ind der die Augen geöffnet werden und der Char hochspringt
    /// </summary>
    /// <returns></returns>
    public IEnumerator StartAnimation()
    {
        GameManager.block = true;
        yield return new WaitForFixedUpdate();
        rb.bodyType = RigidbodyType2D.Static;

        yield return new WaitForSeconds(1);
        anim[0].SetTrigger("start");
        anim[1].SetTrigger("start");
        yield return new WaitForSeconds(2);

        Vector3 realPos = transform.position;
        Vector3 realScale = transform.localScale;

        int steps = 100;
        float b = -steps / 2f;
        float a = -4f / (steps * steps);
        float factor;

        for (int i = 0; i <= steps; i++)
        {
            factor = a * (i + b) * (i + b) + 1;
            transform.position = realPos + Vector3.up * factor;
            transform.localScale = realScale + (Vector3)Vector2.one * factor;

            yield return new WaitForEndOfFrame();
        }

        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.velocity = Vector2.down * 2;
        GameManager.block = false;
        StartCoroutine(Blink());
        yield break;
    }

    /// <summary>
    /// Startet den Blink-Zyklus durch das öffnen der Augen
    /// </summary>
    public void StartEyes()
    {
        anim[0].SetTrigger("start");
        anim[1].SetTrigger("start");
    }

    /// <summary>
    /// Aktualisiert die Stauchung des Sprites
    /// </summary>
    protected void UpdateSquish()
    {
        if (!isSquishing)
        {
            //Dehne oder stauche Figur, wenn sie sich bewegt:
            if (rb.velocity.y - preVel.y - Mathf.Abs(rb.velocity.x - preVel.x) > 8 && preVel.y < 0)// && !bottom)
            {
                //Debug.Log("do squish");
                StartCoroutine(Squish());
            }
            else
                transform.localScale = new Vector3(properties.size + (Mathf.Abs(rb.velocity.x) - Mathf.Abs(rb.velocity.y)) / 100f, properties.size + (Mathf.Abs(rb.velocity.y) - Mathf.Abs(rb.velocity.x)) / 100f);
        }
        //geschw. und position des letzten Zyklus aufnehmen:
        prePos = transform.position;
        preVel = rb.velocity;
    }

    /// <summary>
    /// Dreht den Char zum Boden durch Einsattz von zwei Raycasts
    /// </summary>
    protected void RotateToGround()
    {
        //Drehe die Figur so, sodass sie gerade auf der Oberfläche steht 
        RaycastHit2D rayHit_right = Physics2D.Raycast((Vector2)transform.position + RotToVec(rb.rotation)*0.2f, Vector2.down, 1.3f, mask);
        RaycastHit2D rayHit_left = Physics2D.Raycast((Vector2)transform.position + RotToVec(180 + rb.rotation)*0.2f, Vector2.down, 1.3f, mask);

        left_down  = rayHit_left.collider;
        right_down = rayHit_right.collider;

        if (left_down && right_down)
            rb.rotation += (rayHit_left.distance - rayHit_right.distance)*50f;
        else //gehe in die Ausgangslage zurück
            rb.rotation -= rb.rotation/10f; 
    }

    /// <summary>
    /// Animation, in der der Char zusammengestaucht wird -> Aufprall
    /// </summary>
    /// <returns></returns>
    public IEnumerator Squish()
    {
        isSquishing = true;

        Vector3 goal = new Vector3(0.6f, 0.25f);
        Vector3 step = (transform.localScale - goal) / 10;
        for (int i = 0; i < 10; i++)
        {
            transform.localScale -= step;
            transform.position -= new Vector3(0, 0.1f);
            yield return new WaitForFixedUpdate();
        }
        goal = new Vector3(properties.size, properties.size);
        step = (transform.localScale - goal) / 10;
        for (int i = 0; i < 10; i++)
        {
            transform.localScale -= step;
            yield return new WaitForFixedUpdate();
        }
        transform.localScale = goal;
        isSquishing = false;
        yield break;
    }

    /// <summary>
    /// BlinzelAnimation- Dauerroutine
    /// </summary>
    /// <returns></returns>
    protected IEnumerator Blink()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(2f, 6f));
            anim[0].SetTrigger("Blink");
            anim[1].SetTrigger("Blink");
        }
    }

    /// <summary>
    /// Setzt dieDrawing Order der Augen
    /// </summary>
    /// <param name="layerName"></param>
    /// <param name="order"></param>
    public void SetEyeOrder(string layerName, int order)
    {
        anim[0].GetComponent<SpriteRenderer>().sortingLayerName = layerName;
        anim[1].GetComponent<SpriteRenderer>().sortingLayerName = layerName;
        anim[0].GetComponent<SpriteRenderer>().sortingOrder = order;
        anim[1].GetComponent<SpriteRenderer>().sortingOrder = order;
    }

    /// <summary>
    /// Setzt die Farbe des kompletten Chars
    /// </summary>
    /// <param name="color"></param>
    public void SetColor(Color color)
    {
        GetComponent<SpriteRenderer>().color = color;
        anim[0].GetComponent<SpriteRenderer>().color = color;
        anim[1].GetComponent<SpriteRenderer>().color = color;
    }

    /// <summary>
    /// Umwandlung von der rotation in Grad zu einem Einheitsvektor, der in die Richtung zeigt
    /// </summary>
    /// <param name="rotation"></param>
    /// <returns></returns>
    public static Vector2 RotToVec(float rotation)
    {
        return new Vector2(Mathf.Cos(rotation * Mathf.Deg2Rad), Mathf.Sin(rotation * Mathf.Deg2Rad));
    }

    /// <summary>
    /// executeInput ist die Funktion, die Eingaben im Spiel in Aktionen umsetzt
    /// </summary>
    /// <param name="left">Bewegt den Charakter nach links</param>
    /// <param name="right">Bewegt den Charakter nach rechts</param>
    /// <param name="up">Lässt den Charakter springen</param>
    /// <param name="properties">maximale Werte, die das Moveset des Char parametrisiert</param>
    public void ExecuteInput(bool left, bool right, bool up, Properties properties)
    {
        float velDir = Mathf.Atan2(rb.velocity.y, rb.velocity.x) * Mathf.Rad2Deg;
        float diff = rb.rotation - velDir;

        //Links-Rechts:
        if (left)
        {
            if (rb.velocity.x > -properties.velocity * 5)
            {
                momentum = 0;
                rb.velocity += Vector2.left * properties.acceleration;
            }
            else if (momentum > rb.velocity.x)
                momentum = rb.velocity.x;
        }
        if (right)
        {
            if (rb.velocity.x < properties.velocity * 5)
            {
                momentum = 0;
                rb.velocity += Vector2.right * properties.acceleration;
            }
            else if (momentum < rb.velocity.x)
                momentum = rb.velocity.x;            
        }
        if (left == right)
            momentum = 0;

        //Springen:
        if (up && jumpCount > 0)
        {
            //rb.AddForce(Vector2.up * jumpCount * 30f);
            rb.velocity += Vector2.up * jumpCount;
            jumpCount -= 0.5f;
        }
        else if (jumpCount != properties.jumpForce)
            jumpCount = 0;

        //Dämpfung:
        if (momentum == 0)
            rb.velocity *= new Vector2(1 - properties.velocity_Damping, 1);
    }

    /// <summary>
    /// Diese Funktion ändert über Steps=20 Bildern die Farbe des Sprites des Objektes zu der angegebenen Farbe color.
    /// </summary>
    /// <param name="color">Farbe, die das Objekt am Ende Haben soll</param>
    public void PulsColor(Color color)
    {
        StartCoroutine(SweepColor(color));
    }

    IEnumerator SweepColor(Color color, int steps = 10)
    {
        SpriteRenderer sprite = GetComponent<SpriteRenderer>();
        Color stepColor = (color - sprite.color)/steps; 
        /*
        float stepRed   = color.r - sprite.color.r;
        float stepGreen = color.g - sprite.color.g;
        float stepBlue  = color.b - sprite.color.b;
        float stepGamma = color.a - sprite.color.a;
        */
        for (int i = 0; i < steps; i++)
        {
            sprite.color += stepColor;
            yield return new WaitForEndOfFrame();
        }

        for (int i = 0; i < steps; i++)
        {
            sprite.color -= stepColor;
            yield return new WaitForEndOfFrame();
        }

        yield break;
    }

    /// <summary>
    /// Setzt den Sprung zurück, sobald der Boden wieder berührt wird
    /// </summary>
    /// <param name="collider"></param>
    public virtual void ResetJump(Collider2D collider)
    {
        //Falls Verbündet, dann gebe das Signal zu folgen
        if ((collider.CompareTag("Ground") || collider.CompareTag("Object")) && rb.velocity.y < 1)
            jumpCount = properties.jumpForce;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Ground")) bottom = true;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Ground")) bottom = false;
    }
}
