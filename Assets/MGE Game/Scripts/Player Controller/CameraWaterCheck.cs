using System.Collections.Generic;
using UnityEngine;

public class CameraWaterCheck : MonoBehaviour
{

    private List<Collider> triggers = new List<Collider>();

    private void OnTriggerEnter(Collider other)
    {

        if (!triggers.Contains(other))
            triggers.Add(other);

    }

    private void OnTriggerExit(Collider other)
    {

        if (triggers.Contains(other))
            triggers.Remove(other);

    }

    public bool IsUnderwater()
    {

        triggers.RemoveAll(trigger => !trigger); // remove null entries

        foreach (Collider trigger in triggers)
        {

            if (trigger.GetComponentInParent<Water>())
                return true;

        }

        return false;

    }

}
