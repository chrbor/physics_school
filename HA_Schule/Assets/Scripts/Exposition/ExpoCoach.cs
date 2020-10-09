using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExpoCoach : MonoBehaviour
{
    public bool runCoach;

    // Update is called once per frame
    void FixedUpdate()
    {
        if(runCoach) transform.position += Vector3.left * 0.019f;
    }
}
