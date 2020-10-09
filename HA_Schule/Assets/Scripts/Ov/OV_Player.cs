using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameManager;

public class OV_Player : Ov_MoveScript
{
    private bool triggerLockedOn;
    private CapsuleCollider2D trigger;
    private GameObject InteractSymbol;

    private float velocity = 0.2f;
    public static bool block;
    private static bool interactBlock;
    private OV_NPC npc;

    // Start is called before the first frame update
    void Start()
    {
        npc = GetComponent<OV_NPC>();
        trigger = GetComponent<CapsuleCollider2D>();
        InteractSymbol = transform.GetChild(1).gameObject;
        InteractSymbol.SetActive(false);
    }

    private void FixedUpdate()
    {
        if (block) return;
        if (state.up)    rb.position += Vector2.up    * velocity;
        if (state.down)  rb.position += Vector2.down  * velocity;
        if (state.left)  rb.position += Vector2.left  * velocity;
        if (state.right) rb.position += Vector2.right * velocity;        
    }

    void Update()
    {
        npc.block = !block;
        //trigger.enabled = !block;
        //triggerLockedOn &= !block;
        if (block)
            return;

        state.Set_up(Input.GetKey(KeyCode.W));
        state.Set_down(Input.GetKey(KeyCode.S));
        state.Set_left(Input.GetKey(KeyCode.A));
        state.Set_right(Input.GetKey(KeyCode.D));
        state.run = state.up || state.down || state.left || state.right;

        SetMovementState();
    }

    public IEnumerator ShowInteract(GameObject item)
    {
        while(triggerLockedOn)
        {
            InteractSymbol.transform.localPosition = (Vector2)((item.transform.position - transform.position)/2 + Vector3.up*4);
            InteractSymbol.SetActive(true);
            if (Input.GetKey(KeyCode.E) || Input.GetKey(KeyCode.Space))
            {
                block = true;
                yield return new WaitUntil(()=> !(Input.GetKey(KeyCode.E) || Input.GetKey(KeyCode.Space)));
                //triggerLockedOn = false;
                block = item.GetComponent<IInteractable>().Interact(gameObject);
                InteractSymbol.SetActive(false);
                yield break;
            }

            yield return new WaitForFixedUpdate();
        }
        yield break;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (triggerLockedOn || other.gameObject.layer == 0 || stateType == GameStateType.menu || stateType == GameStateType.sequence || other.CompareTag("Ghost")) return;
        triggerLockedOn = true;

        StartCoroutine(ShowInteract(other.gameObject));
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!triggerLockedOn || other.gameObject.layer == 0) return;
        triggerLockedOn = false;
        InteractSymbol.SetActive(false);
    }
}
