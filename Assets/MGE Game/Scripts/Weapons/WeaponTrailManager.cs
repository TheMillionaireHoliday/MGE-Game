using FishNet.Object;
using System.Collections;
using UnityEngine;

public class WeaponTrailManager : NetworkBehaviour
{
    [SerializeField] private MonoBehaviour weaponComponent;
    private IWeaponSpawnsTrail weapon;

    [SerializeField] Transform startTransform;
    [SerializeField] WeaponImpactManager weaponImpactManager;

    [SerializeField] private ReusablePool pool;

    [SerializeField] private float maxDistance = 5f;

    private float bulletSpeed = 100;

    private void Awake()
    {
        weapon = weaponComponent as IWeaponSpawnsTrail;
    }

    private void OnEnable()
    {
        weapon.OnShotBulletInDirection += SpawnTrailByDirection;
        weapon.OnShotBulletInHitpoint += SpawnTrailByHitPoint;
    }
    private void OnDisable()
    {
        weapon.OnShotBulletInDirection -= SpawnTrailByDirection;
        weapon.OnShotBulletInHitpoint -= SpawnTrailByHitPoint;
    }
    public struct SpawnTrailInfo
    {
        public Vector3 hitPoint;
        public Vector3 hitNormal;
        public Vector3? impactHitPoint;
        public ImpactMaterials.MaterialType materialType;

        public SpawnTrailInfo(Vector3 hitPoint, Vector3 hitNormal, Vector3? impactHitPoint, ImpactMaterials.MaterialType materialType)
        {
            this.hitPoint = hitPoint;
            this.hitNormal = hitNormal;
            this.impactHitPoint = impactHitPoint;
            this.materialType = materialType;
        }
    }





    // ================== Trail methods
    public void SpawnTrailByHitPoint(Vector3 camPos, Vector3 hitPoint, Vector3 hitNormal, ImpactMaterials.MaterialType materialType)
    {
        Vector3 endPoint = hitPoint;
        Vector3 impactHitPoint = hitPoint;

        if (Vector3.Distance(camPos, hitPoint) > maxDistance)
        {
            endPoint = camPos + (hitPoint - camPos).normalized * maxDistance;
        }

        SpawnTrailRequest(new SpawnTrailInfo(endPoint, hitNormal, impactHitPoint, materialType));
    }

    public void SpawnTrailByDirection(Vector3 camPos, Vector3 dir)
    {
        Vector3 realHitPoint = camPos + dir * maxDistance;

        SpawnTrailRequest(new SpawnTrailInfo(realHitPoint, Vector3.zero, null, ImpactMaterials.MaterialType.None));
    }

    private void SpawnTrailRequest(SpawnTrailInfo info)
    {
        StartCoroutine(SpawnTrail_Coroutine(info)); // For owner
        SpawnTrailServer(info);                                // For everyone else
    }

    [ServerRpc]
    private void SpawnTrailServer(SpawnTrailInfo info)
    {
        SpawnTrailObserver(info);
    }

    [ObserversRpc(ExcludeOwner = true)]
    private void SpawnTrailObserver(SpawnTrailInfo info)
    {
        StartCoroutine(SpawnTrail_Coroutine(info));
    }

    private IEnumerator SpawnTrail_Coroutine(SpawnTrailInfo info)
    {
        yield return null; // wait for shooting animation to start

        Vector3 startPosition = startTransform.transform.position;

        GameObject trailGO = pool.GetObject();
        TrailRenderer trail = trailGO.GetComponent<TrailRenderer>();

        trailGO.transform.position = startPosition;
        trail.Clear();

        float distance = Vector3.Distance(startPosition, info.hitPoint);
        float remainingDistance = distance;

        while (remainingDistance > 0)
        {
            trail.transform.position = Vector3.Lerp(startPosition, info.hitPoint, 1 - (remainingDistance / distance));

            remainingDistance -= bulletSpeed * Time.deltaTime;

            yield return null;
        }

        trail.transform.position = info.hitPoint;
        if (info.impactHitPoint.HasValue)
        {
            weaponImpactManager.SpawnImpact(info.impactHitPoint.Value, info.hitNormal, info.materialType);
        }

        pool.Release(trailGO);
    }
}

