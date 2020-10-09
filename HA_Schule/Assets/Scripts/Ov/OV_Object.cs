using UnityEngine;

/// <summary>
/// Klasse an Objekten in der Oberwelt, mit denen man interagieren kann. Es muss daran gedacht werden die Layer entsprechend einzustellen
/// </summary>
public class OV_Object : MonoBehaviour, IInteractable
{
    public enum Type { noAction, chairUp, chairDown, image}
    [Header("Layer einstellen")]
    public Type type;
    [Header("_SequenceCount will be added to Start Sequence")]
    public string startSequence;
    public bool incrementSequenceCount;

    public bool Interact(GameObject triggerObj)
    {
        if(type == Type.chairUp || type == Type.chairDown)
        {
            Vector2 diff = transform.position - triggerObj.transform.position;
            Vector2 pos = new Vector2(
                Mathf.Abs(diff.x) > Mathf.Abs(diff.y) ? diff.x : 0,
                Mathf.Abs(diff.y) > Mathf.Abs(diff.x) ? diff.y : 0);
            Vector2 dir = type == Type.chairUp ? Vector2.up : Vector2.down;

            triggerObj.transform.position = (Vector2)transform.position - pos; 
            triggerObj.GetComponent<OV_NPC>().Set_Sit(pos, dir);
        }

        if (incrementSequenceCount)
            GameManager.SequenceCount++;
        if (startSequence.Length != 0)
        {
            Debug.Log(startSequence + "_" + GameManager.SequenceCount.ToString());
            return GameManager.manager.StartExecutingEvents(startSequence + "_" + GameManager.SequenceCount.ToString());
        }
        else
            return false;
    }
}
