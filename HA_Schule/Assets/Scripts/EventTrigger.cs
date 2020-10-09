using UnityEngine;
using UnityEngine.Events;
using static DevInfoScript;
using static GameManager;
public class EventTrigger : MonoBehaviour
{
    [System.Serializable]
    public class OnEvent : UnityEvent { };
    public OnEvent PlayEvent;

    private void OnTriggerStay2D(Collider2D other)
    {
        if (!other.CompareTag("Player") || stateType.Equals(GameStateType.sequence) || stateType.Equals(GameStateType.menu)) return;
        PlayEvent.Invoke();
        if(devInfo_On) Destroy(gameObject);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1, 1, 1, 0.5f);
        Gizmos.DrawIcon(transform.position, "DevInfo", true);
    }
}
