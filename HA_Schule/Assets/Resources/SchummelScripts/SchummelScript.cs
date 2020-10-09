using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Scriptables", menuName = "SchummelScript"), ExecuteInEditMode]
public class SchummelScript : ScriptableObject
{
    public enum EventType { spawn, despawn, move, wait, jump, add, combine, change_size, change_mass, change_vel, change_puls, change_force, change_energy, addSavePoint, set_Size, set_Mass,  }
    public enum TriggerType { time, none }

    [System.Serializable]
    public class SchummelEvent
    {
        public string title;

        public TriggerType trigger;
        public float triggerTime;
        [Header("")]
        public EventType eventType;
        public string[] focus;
        [HideInInspector]
        public Vector2 startPosition;
        public Vector2 endPosition;
        public float valueSet;
    }
    public List<SchummelEvent> events;
}
