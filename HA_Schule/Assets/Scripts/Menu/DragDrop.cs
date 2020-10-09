using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DragDrop : MonoBehaviour
{
    Vector3 startPos;
    bool dragActivated = false;
    RectTransform rTransform;
    // Start is called before the first frame update
    void Start()
    {
        rTransform = GetComponent<RectTransform>();
        startPos = rTransform.position;
    }

    public void StartDrag()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKey(KeyCode.Escape))
        {
            rTransform.position = startPos;
            dragActivated = false;
        }
        else if (dragActivated)
        {
            rTransform.position = Input.mousePosition;//folge Mauszeiger
        }

    }
}
