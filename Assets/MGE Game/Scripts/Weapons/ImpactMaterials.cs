using UnityEngine;

public class ImpactMaterials
{
    public enum MaterialType
    {
        Concrete,
        Player,
        None
    }

    public static MaterialType GetMaterialType(GameObject hitObject)
    {
        if (hitObject.GetComponentInParent<PlayerHealth>() != null)
            return MaterialType.Player;

        return MaterialType.Concrete;
    }
}
