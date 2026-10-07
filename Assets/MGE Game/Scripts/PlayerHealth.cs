using FishNet.Object;
using FishNet.Object.Synchronizing;
using System;
using System.Collections;
using UnityEngine;

public class PlayerHealth : NetworkBehaviour, IResettableServer
{
    public readonly SyncVar<int> currentHealth = new SyncVar<int>(200);
    public readonly SyncVar<int> maxHealth = new SyncVar<int>(200);

    private PlayerKnockback knockback;

    [SerializeField] private UIEventChannelSO UIChannel;

    public Action<DamageInfo> OnDamageTakenObserver;
    public Action OnDeathObserver;

    public float externalDamageMultiplier = 1.0f; // Edited by status effects

    private float maxOverheal = 1.5f;
    [SerializeField] private float overhealTickWait = 1 / 6f;

    public override void OnStartServer()
    {
        knockback = GetComponent<PlayerKnockback>();
        StartCoroutine(OverhealCoroutine());
    }
    private void OnEnable()
    {
        currentHealth.OnChange += OnCurrentHealth;
        maxHealth.OnChange += OnMaxHealth;
        GlobalEventsManager.Instance.OnPlayerDeath_Server += OnPlayerDeath;
    }
    private void OnDisable()
    {
        currentHealth.OnChange -= OnCurrentHealth;
        maxHealth.OnChange -= OnMaxHealth;

        if (GlobalEventsManager.Instance != null)
            GlobalEventsManager.Instance.OnPlayerDeath_Server -= OnPlayerDeath;
    }

    private void OnCurrentHealth(int prev, int next, bool asServer)
    {
        if (IsOwner)
            UIChannel.HealthChanged(next, maxHealth.Value);
    }
    private void OnMaxHealth(int prev, int next, bool asServer)
    {
        if (IsOwner)
            UIChannel.HealthChanged(currentHealth.Value, next);
    }

    // ================== Overheal ========================

    [Server]
    private void OnPlayerDeath(PlayerDiedEvent _event)
    {
        if (_event.playerNO.Owner == Owner) // add overheal but not if you died yourself
            return;

        AddOverheal();
    }

    [Server]
    private void AddOverheal()
    {
        currentHealth.Value = (int)Mathf.Ceil(maxHealth.Value * maxOverheal);
    }

    [Server]
    private IEnumerator OverhealCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(overhealTickWait);

            if (currentHealth.Value > maxHealth.Value)
                currentHealth.Value -= 1;
        }
    }

    // ================ Taking Damage ======================

    [Server]
    public void TakeDamage(DamageInfo _inDamageInfo)
    {
        var outDamageInfo = _inDamageInfo;

        var finalDamage = CalculateFinalDamage(_inDamageInfo);

        outDamageInfo.baseDamage = finalDamage;

        currentHealth.Value -= finalDamage;

        BroadcastDamage(outDamageInfo);

        if (currentHealth.Value <= 0)
        {
            OnDeath(outDamageInfo);
            return;
        }

        knockback.SetKnockback(outDamageInfo);
    }

    [Server]
    public int CalculateFinalDamage(DamageInfo damageInfo)
    {
        if (damageInfo.attacker != Owner)
        { // Don't do it for rocket jumps or other self damage
            damageInfo.baseDamage *= externalDamageMultiplier;
        }

        return (int)Mathf.Ceil(damageInfo.baseDamage);
    }

    [Server]
    public bool ThisDamageKillsThePlayer(DamageInfo damageInfo)
    {
        return (CalculateFinalDamage(damageInfo) >= currentHealth.Value);
    }

    [ObserversRpc]
    private void BroadcastDamage(DamageInfo damageInfo)
    {
        OnDamageTakenObserver?.Invoke(damageInfo);
    }

    // ================ Death ======================

    [Server]
    private void OnDeath(DamageInfo damageInfo)
    {
        BroadcastDeath(damageInfo);
        GlobalEventsManager.Instance.PlayerDied_Server(new PlayerDiedEvent(damageInfo, this.NetworkObject));
    }

    [ObserversRpc]
    private void BroadcastDeath(DamageInfo damageInfo)
    {
        OnDeathObserver?.Invoke();
        GlobalEventsManager.Instance.PlayerDied_Observer(new PlayerDiedEvent(damageInfo, this.NetworkObject));
    }

    public void ResetStateServer()
    {
        ResetHealth();
    }
    private void ResetHealth()
    {
        currentHealth.Value = maxHealth.Value;
    }
}