using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OV_NPC : Ov_MoveScript, IMovableObject
{
    [HideInInspector]
    public int eventID;

    private float moveVel = 0.1f;
    private float runVel = 0.2f;
    private float velocity = 0;

    private bool stop;
    private bool endReached;
    [HideInInspector]
    public bool block;

    public void Update()
    {
        if (block) return;
        SetMovementState();
    }

    public void Set_Move(Vector2 goalPosition, bool run = false)
    {
        ResetSavedPosition();

        state.move = !run;
        state.run = run;
        StartCoroutine(MoveToPosition(goalPosition, true));
    }

    public void Set_StayInArea(float distance)
    {
        ResetSavedPosition();

        StartCoroutine(StayInArea(distance));
    }

    public void Set_Stand(Vector2 direction = new Vector2())
    {
        Debug.Log(gameObject.name + ": stand");
        if(direction != Vector2.zero)
        {
            state.Set_right(false);
            state.Set_left(false);
            state.Set_up(false);
            state.Set_down(false);

            state.Set_right(direction.x > 0);
            state.Set_left(direction.x < 0);
            state.Set_up(direction.y > 0);
            state.Set_down(direction.y < 0);
        }
        ResetSavedPosition();
    }

    public void Set_Sit(Vector2 position, Vector2 direction)
    {
        stop = true;
        state.move = false;
        state.run = false;


        Debug.Log(rb.position);
        GetComponent<Collider2D>().isTrigger = true;
        rb.position = new Vector2(
            2*((int)((rb.position.x+Mathf.Sign(rb.position.x))/2))- Mathf.Sign(rb.position.x), 
            2*((int)((rb.position.y+Mathf.Sign(rb.position.y))/2))- Mathf.Sign(rb.position.y)
            );
        savedPosition = rb.position;

        if (position.y > 0) rb.position += Vector2.up*2;
        else if (position.y < 0) rb.position += Vector2.down*2;
        else if (position.x > 0) rb.position += Vector2.right*2;
        else rb.position += Vector2.left*2;

        if(direction.y <= 0)
            rb.position += Vector2.up;
        else
            rb.position += Vector2.up * 1.2f;

        savedPosition -= rb.position;
        //Set_Stand(direction);
        state.Set_right(false);
        state.Set_left(false);
        state.Set_up(false);
        state.Set_down(false);

        state.Set_right(direction.x > 0);
        state.Set_left(direction.x < 0);
        state.Set_up(direction.y > 0);
        state.Set_down(direction.y < 0);
        state.sit = true;
        sorter.sit = true;
        sorter.up = direction.y > 0;
        sorter.down = direction.y < 0;
    }

    public void Set_ContinousChat()
    {
        stop = true;
        state.move = false;
        state.run = false;
        StartCoroutine(ContinousChat());
    }

    public void Set_Chat()
    {
        stop = true;
        state.move = false;
        state.run = false;
        state.chat = true;
    }

    public void Set_Jump()
    {
        stop = true;
        state.move = false;
        state.run = false;
        state.jump = true;
    }

    public void Set_Fall()
    {
        stop = true;
        state.move = false;
        state.run = false;
        state.fall = true;
    }

    public void Set_Spawn(int blendTime)
    {
        StartCoroutine(BlendIn(blendTime));
    }

    private void ResetSavedPosition()
    {
        GetComponent<Collider2D>().isTrigger = false;
        sorter.sit = false;

        stop = true;
        if (savedPosition != Vector2.zero)
        {
            rb.position += savedPosition;
            savedPosition = Vector2.zero;
        }

        state.move = false;
        state.run = false;
    }

    public IEnumerator StayInArea(float distance)
    {
        Vector2 startpos = transform.position;
        stop = false;
        while (!stop)
        {
            state.move = true;
            state.run = false;
            StartCoroutine(MoveToPosition(startpos + Random.insideUnitCircle * distance));
            for(int i = Random.Range(100, 300); i > 0; i--)
            {
                yield return new WaitForFixedUpdate();
                if (stop) yield break;
            }
            endReached = true;
            yield return new WaitForFixedUpdate();
            if (stop) yield break;

            endReached = false;
            state.move = false;
            state.run = false;
            for (int i = 100; i > 0; i--)
            {
                yield return new WaitForFixedUpdate();
                if (stop) yield break;
            }
        }
        yield break;
    }

    public IEnumerator ContinousChat()
    {
        stop = false;
        while (!stop)
        {
            state.chat = true;
            yield return new WaitForSeconds(Random.Range(1f, 3f));
        }
        yield break;
    }

    public IEnumerator MoveToPosition(Vector2 goal, bool triggering = false)
    {
        yield return new WaitForFixedUpdate();
        endReached = false;
        stop = false;

        if (state.move) velocity = moveVel;
        else if (state.run) velocity = runVel;
        else velocity = 0;

        state.up = false;
        state.down = false;
        state.left = false;
        state.right = false;

        Vector2 diff;
        do
        {
            diff = goal - (Vector2)transform.position;
            //Debug.Log(diff);
            state.Set_left(diff.x < -0.1f);
            state.Set_right(diff.x > 0.1f);
            state.Set_up(diff.y > 0.1f);
            state.Set_down(diff.y < -0.1f);

            if (diff.x > 0.1f)
                rb.position += Vector2.right * velocity;
            else if(diff.x < -0.1f)
                rb.position += Vector2.left * velocity;
            else if(diff.y > 0.1f)
                rb.position += Vector2.up * velocity;
            else
                rb.position += Vector2.down * velocity;

            yield return new WaitForFixedUpdate();
            //Debug.Log(gameObject.name + ": " + eventID);
            endReached = diff.magnitude < 0.2f;
        } while (!(stop || endReached));


        if (!endReached) yield break;

        state.move = false;
        state.run = false;
        state.up = false;
        state.down = false;
        state.left = false;
        state.right = false;

        if (triggering)
        {
            Debug.Log("triggermove by " + gameObject + ", id: " + eventID);
            yield return new WaitUntil(() => !GameManager.eventLock);
            GameManager.manager.OnTriggerMove(eventID);
            //yield return new WaitForFixedUpdate();
            //GameManager.eventLock = false;
        }
        yield break;
    }

    public IEnumerator BlendIn(int blendTime = 100)
    {
        SpriteRenderer sprite = transform.GetChild(0).GetComponent<SpriteRenderer>();
        sprite.color = Color.clear;

        Color step = Color.white / blendTime;

        for(int i = 0; i < blendTime; i++)
        {
            sprite.color += step;
            yield return new WaitForEndOfFrame();
        }
        GameManager.manager.OnTriggerSignal();
        yield break;
    }
}
