using FishNet.Component.Animating;

public interface IWeaponReloadingMechanism
{
    public bool CanFireReloadCheck();
    public void Initialize(NetworkAnimator _animator, BaseWeaponBehaviour _baseWeapon, UIEventChannelSO UIChannel);
    public void ReloadCheck();
    public void Interrupt();
    public void OnFire();
    public void OnEquip();
    public void RefillAmmo();
}
