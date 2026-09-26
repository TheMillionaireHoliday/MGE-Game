using FishNet.Connection;
using FishNet.Object;
using Fragsurf.Movement;
using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

public class StraightRocket : NetworkBehaviour
{
    private LayerMask layerMask;
    private Vector3 velocity;
    private NetworkConnection shooter;
    private Collider shooterCollider;
    private Vector3 lastPosition = Vector3.zero;

    [Header("Rocket settings")]
    [SerializeField] private float speed = 25f;
    [SerializeField] private float maxLifeTime = 4f;
    [SerializeField] private float explosionRadius = 3f;
    [SerializeField] private float explosionDamage = 90f;
    [SerializeField] private float enemyKnockbackForce = 2f;

    [Header("Rocket jump settings")]
    [SerializeField] private float selfDamageMultiplier = 0.5f;
    [SerializeField] private float playerKnockbackForce = 3f;
    [SerializeField] private float groundedRocketJumpKnockbackMultiplier = 0.8f;

    [Header("References")]
    [SerializeField] private GameObject explosionPrefab;
    [SerializeField] private GameObject strongExplosionPrefab;
    [SerializeField] private GameObject fireworksLaunchedStatusPrefab;

    [SerializeField] private AudioClip normalExplosionSound;
    [SerializeField] private AudioClip strongExplosionSound;

    [SerializeField] private AudioMixerGroup mixerGroup;

    private NetworkObject weaponNO;

    public void Initialize(Vector3 position, Vector3 direction, NetworkObject weaponNO)
    {
        transform.position = position;
        transform.rotation = Quaternion.LookRotation(direction.normalized);

        velocity = direction.normalized * speed;

        this.weaponNO = weaponNO;
        shooter = weaponNO.Owner;
        shooterCollider = weaponNO.GetComponentInParent<SurfCharacter>().collider;

        layerMask = LayerMask.GetMask("Default", "Player");

        lastPosition = position;

        StartCoroutine(ExplodeProjectileAfterTime());
    }

    private void Update()
    {
        if (!IsServerInitialized)
            return;

        transform.position += velocity * Time.deltaTime;

        CheckCollisionsInPath();

        lastPosition = transform.position;
    }

    [Server]
    private void CheckCollisionsInPath()
    {
        var diff = transform.position - lastPosition;

        RaycastHit hit;

        if (Physics.Raycast(lastPosition, diff.normalized, out hit, diff.magnitude, layerMask, QueryTriggerInteraction.Ignore))
        {

            if (hit.collider == shooterCollider)
                return;

            transform.position = hit.point - diff.normalized * 0.02f; // 0.02 offset back

            OnValidCollisionDetected(hit);
        }
    }

    [Server]
    private void OnValidCollisionDetected(RaycastHit hit)
    {
        Explode(hit);
    }

    [Server]
    private void Explode(RaycastHit inHit)
    {
        Collider col = inHit.collider;

        Collider[] hitColliders = Physics.OverlapSphere(transform.position, explosionRadius);
        var _spawnStrongExplosion = false;

        foreach (Collider hit in hitColliders)
        {
            PlayerHealth health = hit.GetComponentInParent<PlayerHealth>();
            if (health == null)
                continue;

            bool directHit = false;

            if (hit == col) // for some reasons nullables aren't serialized correctly, so just check hit.collider = null
                directHit = true;

            if (ExplosionHasObstacles(transform.position, GetFeetOrigin(hit), hit))
                continue;

            float distance = Vector3.Distance(transform.position, GetFeetOrigin(hit)); // distance is from explosion center to player center

            var relativeDist = distance / explosionRadius; // OverlapSphere gets objects if its collider touches other colliders

            if (relativeDist > 1) // So there might be cases when an object is detected by the sphere collider but is out of range
                continue;

            if (directHit)
                relativeDist = 0;

            float damageMultiplier = 1f - relativeDist;

            float _knockbackForce = this.enemyKnockbackForce;
            Vector3 _knockbackDirection = Vector3.zero;
            float _finalDamage = 0f;

            if (health.Owner == shooter)
            {
                _finalDamage = explosionDamage * damageMultiplier;

                _knockbackForce = playerKnockbackForce * _finalDamage;
                _knockbackDirection = (GetFeetOrigin(hit) - transform.position).normalized;

                if (hit.GetComponentInParent<SurfCharacter>().groundObject != null)
                    _knockbackForce = _knockbackForce * groundedRocketJumpKnockbackMultiplier;

                _finalDamage = _finalDamage * selfDamageMultiplier;
            }
            else
            {
                _finalDamage = explosionDamage * damageMultiplier;
                _knockbackForce = enemyKnockbackForce * _finalDamage;
                _knockbackDirection = Vector3.up;
            }

            DamageInfo damage = new DamageInfo(
                baseDamage: _finalDamage,
                hitPosition: hit.transform.position,
                attacker: shooter,
                knockForce: _knockbackForce,
                knockDirection: _knockbackDirection
            );

            var networkPlayer = hit.GetComponentInParent<NetworkObject>();
            var statusTarget = networkPlayer.GetComponent<StatusEffectTarget>();

            if (networkPlayer.Owner != shooter)
            {
                if (statusTarget.ContainsEffect(StatusFireworksLaunched.EffectName))
                {
                    _spawnStrongExplosion = true;
                }
            }

            bool shotKilledPlayer = health.ThisDamageKillsThePlayer(damage);
            health.TakeDamage(damage);

            if (networkPlayer.Owner != shooter && !shotKilledPlayer)
            {
                statusTarget.AddEffect(new StatusFireworksLaunched(fireworksLaunchedStatusPrefab)); // dont apply after the player has died
            }
        }

        ObserversSpawnExplosion(transform.position, _spawnStrongExplosion);


        if (inHit.collider != null)
        {
            ObserversSpawnDecal(
                hitPoint: inHit.point,
                hitNormal: inHit.normal,
                ImpactMaterials.GetMaterialType(inHit.collider.gameObject),
                _weaponNO: weaponNO
            );
        }

        Despawn();
    }

    [ObserversRpc]
    private void ObserversSpawnExplosion(Vector3 position, bool strongExplosion)
    {
        //if (!strongExplosion) // can occur only in air
        //    position -= velocity.normalized * explosionParticleOffsetFromWall;

        GameObject explosion;
        if (strongExplosion)
            explosion = Instantiate(strongExplosionPrefab);
        else
            explosion = Instantiate(explosionPrefab);

        explosion.transform.position = position;
        explosion.GetComponent<ParticleSystem>().Play();

        SpawnSound(position, strongExplosion);
    }

    [Client]
    private void SpawnSound(Vector3 position, bool strongExplosion)
    {
        GameObject tempAudio = new GameObject("Explosion Audio");
        tempAudio.transform.position = position;

        AudioSource audioSource = tempAudio.AddComponent<AudioSource>();

        var destroySound = strongExplosion ? strongExplosionSound : normalExplosionSound;

        audioSource.clip = destroySound;
        audioSource.volume = 1f;
        audioSource.spatialBlend = 0f;
        audioSource.outputAudioMixerGroup = mixerGroup;

        audioSource.Play();
        Destroy(tempAudio, destroySound.length + 0.1f);
    }

    [ObserversRpc]
    private void ObserversSpawnDecal(Vector3 hitPoint, Vector3 hitNormal, ImpactMaterials.MaterialType materialType, NetworkObject _weaponNO)
    {
        var weaponDecalManager = _weaponNO.GetComponentInChildren<WeaponDecalManager>(); // cause its not initialized on the clients

        weaponDecalManager.SpawnDecal(hitPoint, hitNormal, materialType);
    }

    private Vector3 GetFeetOrigin(Collider collider)
    {
        BoxCollider boxCollider = collider as BoxCollider;

        return boxCollider.transform.position + Vector3.down * boxCollider.size.y / 2;
    }

    private bool ExplosionHasObstacles(Vector3 explosionOrigin, Vector3 playerOrigin, Collider _collider)
    {
        return false;

        /*var diff = explosionOrigin - playerOrigin;

        RaycastHit hit;

        var layerMaskNoPlayer = layerMask & ~LayerMask.GetMask("Player");

        if (Physics.Raycast(explosionOrigin, diff.normalized, out hit, diff.magnitude, layerMaskNoPlayer, QueryTriggerInteraction.Ignore))
        {
            if (hit.collider != _collider && hit.distance < 0.99f)
                return false;
        }

        return true;*/

    }

    private IEnumerator ExplodeProjectileAfterTime()
    {
        yield return new WaitForSeconds(maxLifeTime);

        Explode(new RaycastHit());
    }

    [Server]
    void StartStatus(NetworkObject target)
    {

    }
}
