using UnityEngine;

public class ResettableTrail : MonoBehaviour, IResettableObserver
{
    private TrailRenderer trailRenderer;
    public void Start()
    {
        trailRenderer = GetComponent<TrailRenderer>();
    }

    public void ResetStateObserver()
    {
        trailRenderer.Clear();
    }
}