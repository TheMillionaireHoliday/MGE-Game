using Cysharp.Threading.Tasks;
using FishNet.Component.Animating;
using FishNet.Object;
using System;
using System.Threading;
using UnityEngine;

public class WeaponReloadingGeneric : NetworkBehaviour, IWeaponReloadingMechanism
{
    // ===================================

    [Header("Ammunition")]

    public int magazineSize = 6;
    public int maxAmmo = 6;

    //public bool reloadBulletByBullet = false;
    //public bool hasInfiniteAmmo = false;
    //public bool hasInfiniteMagazine = false;

    // ===================================

    private BaseWeaponBehaviour baseWeapon;
    private Animator animator;

    public Action OnReloadStarted;
    public Action OnReloadCompleted;
    public Action OnReloadCancelled;

    public bool IsReloading { get; private set; }

    public bool CanReload => CurrentAmmo < magazineSize &&
            CurrentReserveAmmo > 0 &&
            CurrentAmmo < CurrentReserveAmmo &&
            baseWeapon.CanReloadBaseCheck;

    private UIEventChannelSO UIChannel;

    private static readonly int ReloadHash = Animator.StringToHash("Reload");

    private int _currentAmmo;
    public int CurrentAmmo
    {
        get { return _currentAmmo; }
        private set
        {
            _currentAmmo = value;

            OnCurrentAmmo();
        }
    }

    private int _currentReserveAmmo = 6;
    public int CurrentReserveAmmo
    {
        get { return _currentReserveAmmo; }
        private set
        {
            _currentReserveAmmo = value;

            OnCurrentAmmo();
        }
    }

    private CancellationTokenSource cts;

    [Header("Animation Data")]

    [SerializeField] private string beginState = "Reload Start";
    [SerializeField] private string loopState = "Reload Loop";
    [SerializeField] private string endState = "Reload End";

    public void Initialize(NetworkAnimator _animator, BaseWeaponBehaviour _baseWeapon, UIEventChannelSO _UIChannel)
    {
        animator = _animator.Animator;
        baseWeapon = _baseWeapon;
        UIChannel = _UIChannel;

        CurrentAmmo = magazineSize;
        CurrentReserveAmmo = magazineSize;
    }

    public void ReloadCheck()
    {
        if (CanReload && !IsReloading)
            ReloadAsync(this.GetCancellationTokenOnDestroy()).Forget();
    }

    public async UniTask ReloadAsync(CancellationToken externalToken)
    {

        cts = CancellationTokenSource.CreateLinkedTokenSource(externalToken);
        var token = cts.Token;

        IsReloading = true;
        OnReloadStarted?.Invoke();

        try
        {
            // Start

            await PlayStateOnceAsync(beginState, token);

            // Loop

            while (CurrentAmmo < magazineSize)
            {
                await PlayStateOnceAsync(loopState, token);

                AddAmmoToClip();
            }

            // End

            await PlayStateOnceAsync(endState, token);

            OnReloadCompleted?.Invoke();
        }
        catch (OperationCanceledException)
        {
            OnReloadCancelled?.Invoke();
        }
        finally
        {
            IsReloading = false;

            cts.Dispose();
            cts = null;
        }
    }

    public void Interrupt() => cts?.Cancel();

    private async UniTask PlayStateOnceAsync(string stateName, CancellationToken token)
    {
        Crossfade(stateName);

        await UniTask.Yield(PlayerLoopTiming.Update, token); // Wait one frame before getting length

        float length = GetCurrentClipLength();
        if (length <= 0f) return;

        float elapsed = 0f;
        while (elapsed < length)
        {
            token.ThrowIfCancellationRequested();
            elapsed += Time.deltaTime;
            await UniTask.Yield(PlayerLoopTiming.Update, token);
        }
    }

    private void Crossfade(string statename)
    {
        animator.CrossFadeInFixedTime(statename, 0f, 0, 0f);
        CrossfadeRpc(statename);
    }

    [ServerRpc]
    void CrossfadeRpc(string statename) => CrossfadeRpcObserver(statename);

    [ObserversRpc(ExcludeOwner = true)]
    private void CrossfadeRpcObserver(string statename)
    {
        animator.CrossFadeInFixedTime(statename, 0f, 0, 0f);
    }

    private float GetCurrentClipLength()
    {
        AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(0);
        // If a transition is running, wait for it to settle.
        if (animator.IsInTransition(0))
            info = animator.GetNextAnimatorStateInfo(0);
        return info.length / Mathf.Max(0.0001f, info.speed);
    }

    private void AddAmmoToClip()
    {
        if (CurrentAmmo >= magazineSize)
            return;

        CurrentAmmo += 1;
    }

    private void OnCurrentAmmo()
    {
        if (!baseWeapon.IsEquipped)
            return;

        UIChannel.OnAmmoChanged(CurrentAmmo, CurrentReserveAmmo);
    }

    public bool CanFireReloadCheck()
    {
        return CurrentAmmo > 0;
    }

    public void OnFire()
    {
        CurrentAmmo -= 1;
        Interrupt();
    }

    public void OnEquip()
    {
        OnCurrentAmmo();
    }

    public void RefillAmmo()
    {
        CurrentAmmo = magazineSize;
    }
}
