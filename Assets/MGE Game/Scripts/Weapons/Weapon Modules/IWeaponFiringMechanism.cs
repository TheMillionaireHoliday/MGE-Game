using FishNet.Component.Animating;
using System;
using UnityEngine;

public interface IWeaponFiringMechanism
{
    public Action OnFireClient { get; set; }
    public void Initialize(Transform _cameraEyes, NetworkAnimator _animator, BaseWeaponBehaviour baseWeapon);
    public void ClientShotRealization();
    public float GetLastActivityTime();
    public void FiringCheck();
}
