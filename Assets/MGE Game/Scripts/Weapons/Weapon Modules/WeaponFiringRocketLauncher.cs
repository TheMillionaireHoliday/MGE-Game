using FishNet.Component.Animating;
using FishNet.Object;
using Fragsurf.Movement;
using System;
using UnityEngine;

public class WeaponFiringRocketLauncher : NetworkBehaviour, IWeaponFiringMechanism
{
    // ======== Outside references =======

    [Header("Combat Stats")]

    public float fireRate = 1.6f; // shots per second

    // ===================================

    private Transform cameraEyes;
    private NetworkAnimator animator;
    private BaseWeaponBehaviour baseWeapon;

    [SerializeField] Vector3 rocketSpawnPoint = new Vector3(-0.2f, -0.5f, 0);

    private float autoAimMinDist = 6f;
    private float autoAimMaxDist = 60f;

    // ===================================


    [SerializeField] private GameObject rocketPrefab;
    public bool CanFire => Time.time >= lastFireTime + 1f / fireRate && baseWeapon.CanFireBaseCheck;

    private float lastFireTime;

    private LayerMask layerMask;

    private Collider shooterCollider;

    // =========== Actions ===========

    public Action OnFireClient { get; set; }

    public void Initialize(Transform _cameraEyes, NetworkAnimator _animator, BaseWeaponBehaviour _baseWeapon)
    {
        cameraEyes = _cameraEyes;
        animator = _animator;
        baseWeapon = _baseWeapon;

        layerMask = LayerMask.GetMask("Default", "Player");
    }

    public void FiringCheck()
    {
        if (GameInputHolder.Input.FirstPersonController.Fire.WasPressedThisFrame())
            FireClient();
    }

    private void FireClient()
    {
        if (!CanFire) return;

        if (shooterCollider == null)
            shooterCollider = GetComponentInParent<SurfCharacter>().collider;

        lastFireTime = Time.time;

        OnFireClient.Invoke();

        animator.SetTrigger("Fire");

        ClientShotRealization();
    }

    public void ClientShotRealization()
    {
        Vector3 camOrigin = cameraEyes.transform.position;

        Vector3 spawnOrigin = cameraEyes.transform.position +
            cameraEyes.transform.forward * rocketSpawnPoint.x +
            cameraEyes.transform.up * rocketSpawnPoint.y +
            cameraEyes.transform.right * rocketSpawnPoint.z;

        Vector3 direction = RocketGetDirection(camOrigin, spawnOrigin, cameraEyes.transform.forward);

        ServerShot(spawnOrigin, direction);
    }

    private Vector3 RocketGetDirection(Vector3 camOrigin, Vector3 spawnOrigin, Vector3 camDir)
    {
        RaycastHit hit;

        if (Physics.Raycast(camOrigin, camDir, out hit, autoAimMinDist, layerMask, QueryTriggerInteraction.Ignore))
        { // If less than autoAimMinDist, just shoot straight
            return camDir;
        }

        if (Physics.Raycast(camOrigin, camDir, out hit, autoAimMaxDist, layerMask, QueryTriggerInteraction.Ignore))
        { // If found a target between autoAimMinDist and autoAimMaxDist, aim at that target

            Vector3 endDir = (hit.point - spawnOrigin).normalized;

            return endDir;
        }

        return (camOrigin + (camDir * autoAimMaxDist) - spawnOrigin).normalized; // Else just aim at the point at the end of that
    }

    [ServerRpc]
    private void ServerShot(Vector3 origin, Vector3 direction)
    {
        GameObject rocketObj = Instantiate(rocketPrefab, origin, Quaternion.LookRotation(direction));
        StraightRocket rocket = rocketObj.GetComponent<StraightRocket>();
        rocket.Initialize(origin, direction, NetworkObject);

        Spawn(rocketObj);
    }

    public float GetLastActivityTime()
    {
        return lastFireTime;
    }
}
