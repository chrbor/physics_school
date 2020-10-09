using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(EditorCursor))]
public class Helper : Editor
{
    void OnSceneGUI()
    {
        EditorCursor cursor = (EditorCursor)target;
        Handles.color = Color.red;

        if (cursor.load)
        {
            cursor.load = false;
            LevelScript scriptLoaded = Resources.Load<LevelScript>(cursor.savePath);
            if (scriptLoaded)
                cursor.events = scriptLoaded.events;
            else
                Debug.Log("Could not load LevelScript");
            return;
        }

        if(cursor.add)
        {
            if(cursor.addAt <= cursor.events.Length && cursor.addAt >= 0)
            {
                LevelScript.LevelEvent[] temp = cursor.events;
                cursor.events = new LevelScript.LevelEvent[cursor.events.Length+1];
                int ptr = 0;
                for(; ptr < cursor.addAt; ptr++)
                {
                    cursor.events[ptr] = temp[ptr];
                }
                cursor.events[ptr++] = new LevelScript.LevelEvent();
                for (; ptr <= temp.Length; ptr++)
                {
                    cursor.events[ptr] = temp[ptr-1];
                }
            }
            cursor.add = false;
            return;
        }

        if (cursor.remove)
        {
            if (cursor.removeAt < cursor.events.Length && cursor.removeAt >= 0 && cursor.events.Length > 0)
            {
                LevelScript.LevelEvent[] temp = cursor.events;
                cursor.events = new LevelScript.LevelEvent[cursor.events.Length -1];
                int ptr = 0;
                for (; ptr < cursor.removeAt; ptr++)
                {
                    cursor.events[ptr] = temp[ptr];
                }
                ptr++;
                for (; ptr < temp.Length; ptr++)
                {
                    cursor.events[ptr - 1] = temp[ptr];
                }
            }
            cursor.remove = false;
            return;
        }

        //get mouse position:
        Vector2 mousePosition = Event.current.mousePosition;
        mousePosition.y = SceneView.currentDrawingSceneView.camera.pixelHeight - mousePosition.y;
        mousePosition = SceneView.currentDrawingSceneView.camera.ScreenToWorldPoint(mousePosition + new Vector2(0f, -70f)) * 1.25f;


        EditorGUI.BeginChangeCheck();
        float size = Camera.current != null ? SceneView.currentDrawingSceneView.camera.orthographicSize : 10;
        cursor.transform.position = mousePosition;
        Handles.Label(mousePosition + new Vector2(0.05f, 0.1f) * size, "x: " + mousePosition.x + "\ny: " + mousePosition.y);


        if(cursor.save)
        {
            Debug.Log("save");
            cursor.scriptToSave = CreateInstance<LevelScript>();
            cursor.scriptToSave.events = cursor.events;
            AssetDatabase.CreateAsset(cursor.scriptToSave, "Assets/Resources/" + cursor.savePath + ".asset");
            AssetDatabase.SaveAssets();
            cursor.save = false;
        }

        EditorGUI.EndChangeCheck();
    }
}
