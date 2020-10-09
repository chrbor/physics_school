using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IMovableObject
{
    void Set_Move(Vector2 position, bool run);

}


public interface IInteractable
{
    bool Interact(GameObject triggerObj);
}
