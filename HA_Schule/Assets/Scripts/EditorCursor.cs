using UnityEngine;

[ExecuteInEditMode]
public class EditorCursor : MonoBehaviour
{
    [Header("save Path example: LevelScripts/-file-/-name-")]
    public string savePath;
    public bool load;
    public bool save;
    [Header("add/remove by writing position to implement:")]
    public int addAt = -1;
    public bool add;
    public int removeAt = -1;
    public bool remove;
    [Header("")]
    public LevelScript.LevelEvent[] events;
    [HideInInspector]
    public LevelScript scriptToSave;

    virtual public void Update()
    {
        
    }
}
