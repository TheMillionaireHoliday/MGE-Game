using FishNet.Object;
using Fragsurf.Movement;
using UnityEngine;

public class PlayerKnockback : NetworkBehaviour
{
    private float knockbackCrouchMultiplier = 1.33f;

    private float minYVelToUnground = 0.1f;

    SurfCharacter surfCharacter;

    public override void OnStartServer()
    {
        surfCharacter = GetComponent<SurfCharacter>();
    }

    [Server]
    public void SetKnockback(DamageInfo damageInfo)
    {
        if (surfCharacter == null)
            return;

        float _crouchMultiplier = 1.0f;
        float _groundedMultiplier = 1.0f;

        if (surfCharacter.moveData.crouching)
            _crouchMultiplier = knockbackCrouchMultiplier;

        float reducer = 0.1f; // to make absolute knockback value less and easier to edit (100 damage = 1 knockback)

        Vector3 endVel = damageInfo.knockForce * damageInfo.knockDirection * _crouchMultiplier * _groundedMultiplier * reducer;
        bool forceUnground = endVel.y >= minYVelToUnground;

        surfCharacter.AddVelocityRpc(Owner,
            endVel,
            forceUnground);

    }
}
