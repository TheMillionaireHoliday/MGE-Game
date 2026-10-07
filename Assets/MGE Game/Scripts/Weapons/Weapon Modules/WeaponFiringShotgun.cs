using FishNet.Component.Animating;
using FishNet.Object;
using Fragsurf.Movement;
using System;
using UnityEngine;

public class WeaponFiringShotgun : NetworkBehaviour, IWeaponFiringMechanism, IWeaponSpawnsTrail
{

    // ======== Outside references =======

    private Transform cameraEyes;
    private NetworkAnimator animator;
    private BaseWeaponBehaviour baseWeapon;

    // ===================================

    [Header("Combat Stats")]
    public float baseDamage = 90f;
    public float fireRate = 1.6f; // shots per second
    public float spread = 0.1f;

    // ===================================

    public bool CanFire => Time.time >= lastFireTime + 1f / fireRate && baseWeapon.CanFireBaseCheck;

    private float lastFireTime;

    private float pelletDamage;
    private int layerMask;

    private Collider shooterCollider;

    [SerializeField] private float maxFalloffDistance = 10f;
    [SerializeField] private float maxFalloffMultiplier = 0.5f;

    // =========== Actions ===========

    public Action OnFireClient { get; set; }
    public Action<Vector3, Vector3, Vector3, ImpactMaterials.MaterialType> OnShotBulletInHitpoint { get; set; } // cameraTransform, hitPoint, hitNormal, materialType
    public Action<Vector3, Vector3> OnShotBulletInDirection { get; set; } // cameraTransform, direction

    public void Initialize(Transform _cameraEyes, NetworkAnimator _animator, BaseWeaponBehaviour _baseWeapon)
    {
        cameraEyes = _cameraEyes;
        animator = _animator;
        baseWeapon = _baseWeapon;

        pelletDamage = baseDamage / 9f;

        shooterCollider = GetComponentInParent<SurfCharacter>().collider;

        layerMask = LayerMask.GetMask("Default", "Player");
    }

    public void FiringCheck()
    {
        if (!IsOwner)
            return;

        if (GameInputHolder.Input.FirstPersonController.Fire.WasPressedThisFrame())
            FireClient();
    }

    private void FireClient()
    {
        if (!CanFire) return;

        lastFireTime = Time.time;

        OnFireClient.Invoke();

        animator.SetTrigger("Fire");

        ClientShotRealization();
    }

    public void ClientShotRealization()
    {
        Vector3 origin = cameraEyes.transform.position;
        Vector3 centralDirection = cameraEyes.transform.forward;

        int hitsAmount = 0;
        Vector3 hitMidPoint = new Vector3(); // Used for calculating blood particles spawn position
        bool hitMidPointWithMidBullet = false; // Always prioritize middle bullet position over any other.

        PlayerHealth playerHealth = null; // Damage is dealt only to one player

        foreach (Vector3 dir in GetPelletVectors(origin, centralDirection))
        {
            RaycastHit hit;

            if (RaycastIgnoreShooter(origin, dir, out hit, layerMask))
            {
                OnShotBulletInHitpoint?.Invoke(cameraEyes.transform.position, hit.point, hit.normal, ImpactMaterials.GetMaterialType(hit.collider.gameObject));

                var nb = hit.collider.gameObject.GetComponentInParent<NetworkObject>();

                if (nb == null)
                    continue;

                PlayerHealth _health = hit.collider.gameObject.GetComponentInParent<PlayerHealth>();
                if (_health == null)
                    continue;

                hitsAmount++;
                playerHealth = _health;

                if (!hitMidPointWithMidBullet)
                {
                    hitMidPoint = hit.point;

                    if (dir == centralDirection)
                        hitMidPointWithMidBullet = true;
                }
            }
            else
            {
                OnShotBulletInDirection?.Invoke(cameraEyes.transform.position, dir);
            }
        }

        if (playerHealth == null)
            return;

        var damageFalloff = GetDamageFalloff(cameraEyes.transform.position, playerHealth.transform.position);

        DamageInfo damageInfo = new DamageInfo(
            baseDamage: pelletDamage * hitsAmount * damageFalloff,
            hitPosition: hitMidPoint,
            attacker: Owner);

        PerformShotServer(damageInfo, playerHealth);
    }

    private Vector3[] GetPelletVectors(Vector3 origin, Vector3 forward)
    {
        Vector3[] vectors = new Vector3[9];

        Vector3 right = Vector3.Cross(Vector3.up, forward).normalized;
        if (right == Vector3.zero) // Straight up or down
            right = Vector3.Cross(Vector3.forward, forward).normalized;

        Vector3 up = Vector3.Cross(forward, right).normalized;

        for (int i = -1; i < 2; i++)
        {
            for (int j = -1; j < 2; j++)
            {
                float diagonalMultiplier = 1f; // Used to make the pattern less square
                if (i != 0 && j != 0)
                    diagonalMultiplier = 0.8f;

                Vector3 dir = (forward +
                    up * i * spread * diagonalMultiplier +
                    right * j * spread * diagonalMultiplier).normalized;

                vectors[j + 1 + 3 * (i + 1)] = dir;
            }
        }

        return vectors;
    }

    private bool RaycastIgnoreShooter(Vector3 origin, Vector3 dir, out RaycastHit hitInfo, int layerMask)
    {
        hitInfo = new RaycastHit();

        RaycastHit[] hits;

        hits = Physics.RaycastAll(origin, dir, 99999999f, layerMask, QueryTriggerInteraction.Ignore);
        float shortestRay = 999999999f;

        foreach (RaycastHit hit in hits)
        {
            if (hit.collider == shooterCollider)
                continue;

            if (hit.distance < shortestRay)
            {
                hitInfo = hit;
                shortestRay = hit.distance;
            }
        }

        if (shortestRay != 999999999f)
        {
            return true;
        }

        return false;
    }

    private float GetDamageFalloff(Vector3 shooterPos, Vector3 hitPos)
    {
        float dist = Vector3.Distance(hitPos, shooterPos);

        float relativeDist = Mathf.Min(dist, maxFalloffDistance) / maxFalloffDistance;

        return Mathf.Lerp(1.0f, maxFalloffMultiplier, relativeDist);
    }

    [ServerRpc]
    private void PerformShotServer(DamageInfo damageInfo, PlayerHealth victim)
    {
        victim.TakeDamage(damageInfo);
        PerformShotObserver(damageInfo, victim);
    }

    [ObserversRpc]
    private void PerformShotObserver(DamageInfo damageInfo, PlayerHealth victim)
    {

    }

    public float GetLastActivityTime()
    {
        return lastFireTime;
    }
}
