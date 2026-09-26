using UnityEngine;

public class CameraFovSetter : MonoBehaviour
{
    [SerializeField] private Camera _camera;
    private void Awake()
    {
        VideoManager.Instance.OnFovChanged += UpdateFov;
        UpdateFov(VideoManager.Instance.FovValue);
    }
    private void OnDestroy()
    {
        VideoManager.Instance.OnFovChanged -= UpdateFov;
    }

    private void UpdateFov(int value)
    {
        _camera.fieldOfView = value;
    }
}
