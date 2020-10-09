using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResetTrigger : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other.CompareTag("Player") && other.name != "Trigger") CheckpointScript.current.ReloadCheckpoint();
    }
}
