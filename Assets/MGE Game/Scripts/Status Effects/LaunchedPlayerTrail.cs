using FishNet;
using FishNet.Object;
using UnityEngine;

public class LaunchedPlayerTrail : MonoBehaviour
{
    private int ownerId = -1;
    private NetworkObject supposedParent;
    bool died = false;

    Vector3 newPos = Vector3.zero;

    public void Initialize(int ownerId, NetworkObject _parent)
    {
        this.ownerId = ownerId;
        this.supposedParent = _parent;

        newPos = _parent.transform.position;
    }

    private void Start()
    {
        if (ownerId == InstanceFinder.ClientManager.Connection.ClientId)
        {
            Destroy(gameObject);
        }
    }

    private void Awake()
    {
        GlobalEventsManager.Instance.OnPlayerDeath_Observer += OnDeath;
        InstanceFinder.TimeManager.OnTick += OnTick;
    }

    private void OnDestroy()
    {
        GlobalEventsManager.Instance.OnPlayerDeath_Observer -= OnDeath;
        InstanceFinder.TimeManager.OnTick -= OnTick;
    }

    private void OnDeath(PlayerDiedEvent e)
    {
        if (e.playerNO.OwnerId == ownerId)
        {
            died = true;
        }
    }

    private void OnTick()
    {
        if (died)
        {
            GetComponent<TrailRenderer>().Clear();
            Destroy(gameObject);
            return;
        }

        transform.position = newPos;
        newPos = supposedParent.transform.position;
    }
}
