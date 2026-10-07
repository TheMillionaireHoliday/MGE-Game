using UnityEngine;
using UnityEngine.UI;

public class LayoutGroupForceRebuild : MonoBehaviour
{

    void Start()
    {
        LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
    }
}
