using UnityEngine;

public class WeaponCameraShake : MonoBehaviour
{
    private PlayerAiming playerAiming;
    public AnimatorStateObserver animatorStateObserver;

    void Start()
    {
        playerAiming = GetComponentInParent<PlayerAiming>();
    }

    private void OnEnable()
    {
        animatorStateObserver.OnStateRestarted += DoPunch;
        animatorStateObserver.OnStateEntered += DoPunch;
    }

    private void OnDisable()
    {
        animatorStateObserver.OnStateRestarted -= DoPunch;
        animatorStateObserver.OnStateEntered -= DoPunch;
    }

    private void DoPunch(string state)
    {
        if (state == "Shot")
            playerAiming.ViewPunch(new Vector2(5f, 0f));
    }
}
