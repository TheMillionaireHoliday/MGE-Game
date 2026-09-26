using FishNet.Connection;
using FishNet.Observing;
using UnityEngine;

[CreateAssetMenu(menuName = "FishNet/Observers/Exclude Owner Condition", fileName = "New Exclude Owner Condition")]
public class ExcludeOwnerCondition : ObserverCondition
{
    public override bool ConditionMet(NetworkConnection connection, bool currentlyAdded, out bool notProcessed)
    {
        notProcessed = false;

        // If the object has no owner, or the connection checking is the owner, hide it.
        if (NetworkObject == null || connection == null || !NetworkObject.IsSpawned)
            return false;

        // Return false if the connection is the owner, true otherwise.
        return NetworkObject.Owner != connection;
    }
    public override ObserverConditionType GetConditionType() => ObserverConditionType.Normal;
}