using System.Collections;
using UnityEngine;

public class WeaponImpactManager : MonoBehaviour
{
    [SerializeField] WeaponDecalManager decalManager;

    [SerializeField] private float autoReleaseDelay = 1f; // Auto-release after this times

    [SerializeField] private ReusablePool defaultImpactPool;
    [SerializeField] private ReusablePool bloodImpactPool;


    public void SpawnImpact(Vector3 hitPoint, Vector3 hitNormal, ImpactMaterials.MaterialType impactMaterial)
    {
        ReusablePool pool;

        switch (impactMaterial)
        {
            case ImpactMaterials.MaterialType.None:
                return;

            case ImpactMaterials.MaterialType.Player:
                pool = bloodImpactPool; break;

            default:
                pool = defaultImpactPool; break;
        }

        var particleGO = pool.GetObject();
        var particleSystem = particleGO.GetComponent<ParticleSystem>();

        particleSystem.transform.position = hitPoint;
        particleSystem.transform.rotation = Quaternion.LookRotation(hitNormal);

        particleSystem.Play();
        decalManager.SpawnDecal(hitPoint, hitNormal, impactMaterial);

        StartCoroutine(FallbackRelease(particleGO, pool));
    }

    private IEnumerator FallbackRelease(GameObject impactGO, ReusablePool pool)
    {
        yield return new WaitForSeconds(autoReleaseDelay);
        if (impactGO != null && pool.ActiveObjects.Contains(impactGO))
        {
            pool.Release(impactGO);
        }
    }
}