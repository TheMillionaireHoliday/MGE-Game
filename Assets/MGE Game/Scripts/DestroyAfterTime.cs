using System.Collections;
using UnityEngine;

public class DestroyAfterTime : MonoBehaviour
{
    [Tooltip("Time in seconds before the object is despawned.")]
    public float secondsBeforeDestroy = 2f;

    public void Start()
    {
        StartCoroutine(DespawnAfterSeconds());
    }

    private IEnumerator DespawnAfterSeconds()
    {
        yield return new WaitForSeconds(secondsBeforeDestroy);

        Destroy(gameObject);
    }
}