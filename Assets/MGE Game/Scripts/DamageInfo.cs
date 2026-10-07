using FishNet.Connection;
using UnityEngine;
public class DamageInfo
{
    public float baseDamage;
    public Vector3 hitPosition;
    public NetworkConnection attacker;
    public float knockForce;
    public Vector3 knockDirection;
    public DamageInfo(float baseDamage, Vector3 hitPosition, NetworkConnection attacker = null, float knockForce = 0, Vector3? knockDirection = null)
    {
        this.baseDamage = baseDamage;
        this.hitPosition = hitPosition;
        this.attacker = attacker;
        this.knockForce = knockForce;
        this.knockDirection = knockDirection ?? Vector3.zero;
    }

    public DamageInfo() { } // A default constructor is required for serialization
}