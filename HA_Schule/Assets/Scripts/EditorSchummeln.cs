using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class EditorSchummeln : MonoBehaviour
{
    [Header("save Path example: lvl_0/checkpoint_0")]
    public string savePath;
    public bool load;
    public bool save;
    [Header("add/remove by writing position to implement:")]
    public int addAt = -1;
    public bool add;
    public int removeAt = -1;
    public bool remove;
    [Header("")]
    public List<SchummelScript.SchummelEvent> events;
    [HideInInspector]
    public SchummelScript scriptToSave;
}
