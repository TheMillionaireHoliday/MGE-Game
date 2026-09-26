using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class WeaponData
{
    [Header("Basic Info")]
    public string weaponName;

    [Header("Combat Stats")]
    public float baseDamage = 90f;
    public float fireRate = 1.6f; // shots per second
    public float spread = 0.1f;

    [Header("Ammunition")]
    public int magazineSize = 30;
    public int maxAmmo = 120;
    public bool reloadBulletByBullet = false;

    public bool hasInfiniteAmmo = false;
    public bool hasInfiniteMagazine = false;

    [Header("Audio")]
    public List<AudioClipSettings> animationAssociatedAudio;
}

[System.Serializable]
public struct AudioClipSettings
{
    public string stateName;

    public AudioClip clip;
    public float volume;
    public float delay;

    public bool interruptAllOtherSounds;
}