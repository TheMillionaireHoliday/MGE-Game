using FishNet.Component.Animating;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

public class BaseWeaponBehaviour : NetworkBehaviour, IResettableServer
{
    public bool IsInitialized { get; private set; }

    // References

    public MonoBehaviour weaponFiringMechanismComponent;
    public IWeaponFiringMechanism weaponFiringMechanism;

    public MonoBehaviour weaponReloadingMechanismComponent;
    public IWeaponReloadingMechanism weaponReloadingMechanism;

    private Transform cameraEyes;
    private NetworkAnimator animator;
    private AnimatorStateObserver animatorStateObserver;

    // Weapon model

    [SerializeField] private MeshRenderer[] weaponModels;

    public readonly SyncVar<bool> weaponModelsEnabled = new SyncVar<bool>(new SyncTypeSettings(WritePermission.ClientUnsynchronized, ReadPermission.ExcludeOwner));
    private void SetModelEnabled(bool value)
    {
        if (!IsOwner) return;
        SetModelEnabledServerRpc(value);
    }
    [ServerRpc(RunLocally = true)] private void SetModelEnabledServerRpc(bool value) => weaponModelsEnabled.Value = value;

    [SerializeField] private Transform offset;

    [SerializeField] private Vector3 viewmodelOffsetPosition = Vector3.zero;
    [SerializeField] private Vector3 worldmodelOffsetPosition = new Vector3(0f, -0.5f, 0f);
    public Vector3 ViewmodelOffsetPosition
    {
        get { return viewmodelOffsetPosition; }

        set
        {
            viewmodelOffsetPosition = value;

            if (IsOwner)
                offset.transform.localPosition = value;
        }
    }

    public Vector3 WorldmodelOffsetPosition
    {
        get { return worldmodelOffsetPosition; }

        set
        {
            worldmodelOffsetPosition = value;

            if (!IsOwner)
                offset.transform.localPosition = value;
        }
    }

    // State variables

    public bool IsEquipped { get; private set; }
    public bool IsPlayingEquippingAnimation { get; private set; }

    private float timeBeforeCanReload = 0.7f;

    // Checks
    public bool CanFireBaseCheck => weaponReloadingMechanism.CanFireReloadCheck() &&
            IsEquipped && !IsPlayingEquippingAnimation &&
            (!RoundManager.Instance.GameActive.Value || RoundManager.Instance.IntroFinished.Value) &&
            !RoundManager.Instance.IsOnVictoryScreen.Value;
    public bool CanReloadBaseCheck => Time.time >= weaponFiringMechanism.GetLastActivityTime() + timeBeforeCanReload &&
            IsEquipped && !IsPlayingEquippingAnimation;

    // Event system for UI and feedback

    [SerializeField] private UIEventChannelSO UIChannel;

    // =========================

    private void OnEnable()
    {
        if (animatorStateObserver == null)
            animatorStateObserver = GetComponentInChildren<AnimatorStateObserver>();

        animatorStateObserver.OnStateExited += OnEquippingEnd;

        SetMechanisms();

        weaponFiringMechanism.OnFireClient += OnFireClient;

        GlobalEventsManager.Instance.OnPlayerDeath_Observer += OnPlayerDeath;

        weaponModelsEnabled.OnChange += OnRendererVisibilityChanged;
    }

    private void Awake()
    {
        // Weapons spawn with models enabled, and disabled only in OnStartClient.
        // This leaves a short window where it is visible during spawning
        // weaponModelsEnabled = false by default, so shouldn't cause problems.

        foreach (MeshRenderer weaponModel in weaponModels)
            weaponModel.enabled = false;
    }

    private void OnDisable()
    {
        SetMechanisms();

        if (animatorStateObserver != null)
        {
            animatorStateObserver.OnStateExited -= OnEquippingEnd;

            weaponFiringMechanism.OnFireClient -= OnFireClient;
        }

        if (GlobalEventsManager.Instance != null)
            GlobalEventsManager.Instance.OnPlayerDeath_Observer -= OnPlayerDeath;

        weaponModelsEnabled.OnChange -= OnRendererVisibilityChanged;
    }

    public override void OnStartClient()
    {
        SetMechanisms();

        animator = GetComponentInChildren<NetworkAnimator>();
        cameraEyes = transform.parent;

        weaponFiringMechanism.Initialize(cameraEyes, animator, this);
        weaponReloadingMechanism.Initialize(animator, this, UIChannel);

        base.OnStartClient();

        if (IsOwner)
            SetupViewmodel();
        else
            SetupWorldmodel();

        Unequip();

        IsInitialized = true;
    }

    private void SetMechanisms()
    {
        if (weaponFiringMechanism == null)
            weaponFiringMechanism = weaponFiringMechanismComponent as IWeaponFiringMechanism;

        if (weaponReloadingMechanism == null)
            weaponReloadingMechanism = weaponReloadingMechanismComponent as IWeaponReloadingMechanism;
    }


    private void SetupViewmodel()
    {
        SetModelEnabled(true);

        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;

        offset.transform.localPosition = viewmodelOffsetPosition;

        int layer = LayerMask.NameToLayer("Viewmodel");

        var children = transform.GetComponentsInChildren<Transform>(includeInactive: true);
        foreach (var child in children)
        {
            child.gameObject.layer = layer;
        }
    }

    private void SetupWorldmodel()
    {
        SetModelEnabled(true);

        transform.localPosition = worldmodelOffsetPosition;
        transform.localRotation = Quaternion.identity;
        transform.localScale = Vector3.one * 1.2f;
    }

    private void Update()
    {
        if (!IsOwner || !IsEquipped || IsPlayingEquippingAnimation)
            return;

        weaponFiringMechanism.FiringCheck();
        weaponReloadingMechanism.ReloadCheck();
    }

    // ===================== Shooting

    private void OnFireClient()
    {
        weaponReloadingMechanism.OnFire();
    }

    // ===================== Reloading

    public void OnEquippingEnd(string state)
    {
        if (state == "Equip")
        {
            IsPlayingEquippingAnimation = false;
        }
    }

    // ===================== Equipping

    [Client]
    public virtual void Equip()
    {
        IsEquipped = true;
        IsPlayingEquippingAnimation = true;

        SetModelEnabled(true);

        weaponReloadingMechanism.OnEquip();

        animator.SetTrigger("Equip");
    }

    [Client]
    public virtual void Unequip()
    {
        IsEquipped = false;
        IsPlayingEquippingAnimation = false;

        weaponReloadingMechanism.Interrupt();

        SetModelEnabled(false);
    }

    private void OnRendererVisibilityChanged(bool prev, bool next, bool asServer)
    {
        foreach (MeshRenderer weaponModel in weaponModels)
            weaponModel.enabled = next;
    }

    // ===================== Resetting

    public void ResetStateServer()
    {
        ResetStateOwner(Owner);
    }

    [TargetRpc]
    private void ResetStateOwner(NetworkConnection conn)
    {
        RefillAmmo();
        weaponReloadingMechanism.Interrupt();

        if (IsEquipped)
            Equip();
    }

    [Client]
    private void OnPlayerDeath(PlayerDiedEvent _event)
    {
        if (_event.playerNO.Owner == Owner)
            return;

        weaponReloadingMechanism.Interrupt();
        RefillAmmo();
    }

    [Client]
    void RefillAmmo()
    {
        weaponReloadingMechanism.RefillAmmo();
    }
}