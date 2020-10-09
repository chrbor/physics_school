using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Diese Klasse regelt die Steuerung aller Chars im Klassenzimmer
/// </summary>
[RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
public class Ov_MoveScript : MonoBehaviour
{
    protected Rigidbody2D rb;
    protected OV_Sorter sorter;
    private SpriteRenderer sprite;
    private Animator anim;

    protected Vector2 savedPosition = Vector2.zero;
    //public Vector2 speed;

    public struct MovementState
    {
        public bool up, down, left, right;
        private bool _up, _down, _left, _right, buffer;
        public bool move, run, sit;
        public bool chat, jump, fall;

        public void Set_up(bool val)   { if (!(down || left || right)) { up = val;} }
        public void Set_down(bool val) { if (!(up || left || right)) { down = val;} }
        public void Set_left(bool val) { if (!(down || up || right)) { left = val;} }
        public void Set_right(bool val){ if (!(down || up || left)) { right = val;} }

        //Trigger steigende Flanke:
        public bool Get_up()    { buffer = up && !_up;        _up = up;       return buffer; }
        public bool Get_down()  { buffer = down && !_down;    _down = down;   return buffer; }
        public bool Get_left()  { buffer = left && !_left;    _left = left;   return buffer; }
        public bool Get_right() { buffer = right && !_right;  _right = right; return buffer; }

        public void Reset()
        {
            sit = false;
            chat = false;
            jump = false;
            fall = false;
            /*
            if(up) { down = false; left = false; right = false; }
            else if(left) { down = false; up = false; right = false; }
            else if(right) { down = false; left = false; up = false; }
            else if(down) { up = false; left = false; right = false; }
            */
        }
    }
    public MovementState state;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = transform.GetChild(0).GetComponent<Animator>();
        sorter = transform.GetChild(0).GetComponent<OV_Sorter>();
        sprite = anim.GetComponent<SpriteRenderer>();
    }

    /// <summary>
    /// Steuert die Bewegunsanimation mittels MovementState
    /// </summary>
    public void SetMovementState()
    {
        if (state.Get_up())
            anim.SetTrigger("Up");
        if (state.Get_down())
            anim.SetTrigger("Down");
        if (state.Get_left())
        {
            anim.SetTrigger("Side");
            sprite.flipX = true;
        }
        if (state.Get_right())
        {
            anim.SetTrigger("Side");
            sprite.flipX = false;
        }


        anim.SetBool("Move", state.move);
        anim.SetBool("Run", state.run);

        if (state.sit)
            anim.SetTrigger("Sit");
        else if (state.chat)
            anim.SetTrigger("Chat");
        else if (state.jump)
            anim.SetTrigger("Jump");
        else if (state.fall)
            anim.SetTrigger("Fall");


        state.Reset();
    }
       
    
    private void SetDrawOrder()
    {
        sprite.sortingOrder = 10000 - (int)transform.position.y;
    }
}
