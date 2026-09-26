using UnityEngine;

public class PlayerDeathTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        PlayerHealth playerHealth = other.gameObject.GetComponentInParent<PlayerHealth>();

        if (playerHealth != null)
        {
            playerHealth.TakeDamage(new DamageInfo(999f, playerHealth.transform.position));
        }
    }
}
