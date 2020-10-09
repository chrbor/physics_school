using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Scriptables",menuName = "LevelScript"), ExecuteInEditMode]
public class LevelScript : ScriptableObject
{
    public enum EventType { wait, spawn, move, run, chat, continousChat, jump, fall, sit, stand, stayInArea, textBox, cameraPosRelativ, cameraPosAbsolut, jumpEvent, startParallelEvent, stopSequence, startHub, startCoreGame, devInfo, setSequence, revealScene, stopGame, setCoreGame}
    public enum EventTrigger { waitForTime, waitForMoveTrigger, waitForTriggerCollect, next, waitForUserInput}
    public enum Expression { none, angry, sad, happy, exclamation, question, annoyed}
    [System.Serializable]
    public struct LevelEvent
    {
        public string title;
        public EventType type;
        public EventTrigger trigger;
        public float triggerVal;
        [Header("")]
        public string focus;
        public float eventVal;
        public Vector2 position;
        public Vector2 direction;
        [TextArea]
        public string spokenText;
        public Expression expression;
    }

    public LevelEvent[] events;
}
