using System;
using UnityEngine;
public interface IWeaponSpawnsTrail
{
    public Action<Vector3, Vector3, Vector3, ImpactMaterials.MaterialType> OnShotBulletInHitpoint { get; set; }
    public Action<Vector3, Vector3> OnShotBulletInDirection { get; set; }
}
