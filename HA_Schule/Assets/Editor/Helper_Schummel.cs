using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(EditorSchummeln))]
public class Helper_Schummel : Editor
{
    void OnSceneGUI()
    {
        EditorSchummeln cursor = (EditorSchummeln)target;
        Handles.color = Color.red;

        if (cursor.load)
        {
            cursor.load = false;
            SchummelScript scriptLoaded = Resources.Load<SchummelScript>("SchummelScripts/" + cursor.savePath);
            if (scriptLoaded)
                cursor.events = scriptLoaded.events;
            else
                Debug.Log("Could not load SchummelScript");
            return;
        }

        if (cursor.add)
        {
            if (cursor.addAt <= cursor.events.Count && cursor.addAt >= 0)
                cursor.events.Insert(cursor.addAt, new SchummelScript.SchummelEvent());

            cursor.add = false;
            return;
        }

        if (cursor.remove)
        {
            if (cursor.removeAt < cursor.events.Count && cursor.removeAt >= 0 && cursor.events.Count > 0)
                cursor.events.RemoveAt(cursor.removeAt);

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


        if (cursor.save)
        {
            Debug.Log("save");
            cursor.scriptToSave = CreateInstance<SchummelScript>();
            cursor.scriptToSave.events = cursor.events;
            AssetDatabase.CreateAsset(cursor.scriptToSave, "Assets/Resources/SchummelScripts/" + cursor.savePath + ".asset");
            AssetDatabase.SaveAssets();
            cursor.save = false;
        }

        EditorGUI.EndChangeCheck();
    }
}
