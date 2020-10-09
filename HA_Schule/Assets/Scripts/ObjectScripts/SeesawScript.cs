using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SeesawScript : MovementScript
{
    public override bool changeSize(float change)
    {
        properties.size += change;
        transform.localScale = Vector2.one * properties.size;
        transform.position += Vector3.up * change / 2;
        return true;
    }
}
