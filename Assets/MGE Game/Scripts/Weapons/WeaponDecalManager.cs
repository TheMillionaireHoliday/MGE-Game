using System.Collections;
using UnityEngine;

public class WeaponDecalManager : MonoBehaviour
{
    [SerializeField] private float autoReleaseDelay = 100f; // Auto-release after this time

    [SerializeField] private ReusablePool pool;

    public void SpawnDecal(Vector3 hitPoint, Vector3 hitNormal, ImpactMaterials.MaterialType materialType)
    {
        if (materialType == ImpactMaterials.MaterialType.Player ||
            materialType == ImpactMaterials.MaterialType.None)
        {

            return;
        }

        var decalGO = pool.GetObject();

        decalGO.transform.position = hitPoint + hitNormal * 0.02f;
        decalGO.transform.rotation = Quaternion.LookRotation(-hitNormal);

        float randomAngle = Random.Range(0f, 360f);
        decalGO.transform.Rotate(hitNormal, randomAngle, Space.World);

        StartCoroutine(FallbackRelease(decalGO));
    }

    private IEnumerator FallbackRelease(GameObject decalGO)
    {
        yield return new WaitForSeconds(autoReleaseDelay);
        if (decalGO != null && pool.ActiveObjects.Contains(decalGO))
        {
            pool.Release(decalGO);
        }
    }
}