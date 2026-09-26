using UnityEngine;

public class HurtCameraShake : MonoBehaviour
{
    private PlayerAiming playerAiming;
    [SerializeField] private PlayerHealth playerHealth; // For some reason GetComponent wont work in Awake().

    private float minDamageClamp = 20f;
    private float maxDamageClamp = 100f;

    [SerializeField] private float maxPunchValue = 5f;

    void Awake()
    {
        playerAiming = GetComponentInChildren<PlayerAiming>();
    }

    private void OnEnable()
    {
        playerHealth.OnDamageTakenObserver += DoPunch;
    }

    private void OnDisable()
    {
        playerHealth.OnDamageTakenObserver -= DoPunch;
    }

    private void DoPunch(DamageInfo damageInfo)
    {
        float punchNormalized = Mathf.Clamp(damageInfo.baseDamage, minDamageClamp, maxDamageClamp) / maxDamageClamp;

        playerAiming.ViewPunch(new Vector2(maxPunchValue * punchNormalized, 0f));
    }
}
