using FishNet.Object;
using UnityEngine;

public class PlayerAiming : NetworkBehaviour
{
    [Header("References")]
    public Transform bodyTransform; // The body that rotates horizontally
    public Transform cameraTransform; // The camera that rotates vertically (viewTransform from SurfCharacter)

    [Header("Sensitivity")]
    public static float sensitivityMultiplier = 1f;
    public float horizontalSensitivity = 1f;
    public float verticalSensitivity = 1f;

    [Header("Restrictions")]
    public float minYRotation = -90f;
    public float maxYRotation = 90f;

    // The real rotation of the camera without recoil
    private Vector3 realRotation;

    [Header("Aimpunch")]
    [Tooltip("bigger number makes the response more damped, smaller is less damped, currently the system will overshoot, with larger damping values it won't")]
    public float punchDamping = 9.0f;

    [Tooltip("bigger number increases the speed at which the view corrects")]
    public float punchSpringConstant = 65.0f;

    [HideInInspector]
    public Vector2 punchAngle;

    [HideInInspector]
    public Vector2 punchAngleVel;

    public override void OnStartClient()
    {
        base.OnStartClient();

        realRotation = transform.rotation.eulerAngles;

        if (IsOwner)
        {
            // The cameraTransform should be your viewTransform from SurfCharacter
            if (cameraTransform == null)
                cameraTransform = GetComponentInChildren<Camera>()?.transform;
        }
        else
        {
            enabled = false;
        }
    }

    private void Update()
    {
        // Only process if we own this object
        if (!IsOwner)
            return;

        // Fix pausing
        if (Mathf.Abs(Time.timeScale) <= 0)
            return;

        DecayPunchAngle();

        // Read input

        Vector2 mouseXY = GameInputHolder.Input.FirstPersonController.Aiming.ReadValue<Vector2>();

        float xMovement = mouseXY.x * horizontalSensitivity / 20f * sensitivityMultiplier;
        float yMovement = -mouseXY.y * verticalSensitivity / 20f * sensitivityMultiplier;

        // Calculate real rotation from input
        realRotation = new Vector3(
            Mathf.Clamp(realRotation.x + yMovement, minYRotation, maxYRotation),
            realRotation.y + xMovement,
            realRotation.z
        );
        realRotation.z = Mathf.Lerp(realRotation.z, 0f, Time.deltaTime * 3f);

        // Apply rotation to body (horizontal)
        if (bodyTransform != null)
        {
            bodyTransform.eulerAngles = Vector3.Scale(realRotation, new Vector3(0f, 1f, 0f));
        }

        // Apply rotation and recoil to camera (vertical)
        if (cameraTransform != null)
        {
            Vector3 cameraEulerPunchApplied = realRotation;
            cameraEulerPunchApplied.x += punchAngle.x;
            cameraEulerPunchApplied.y += punchAngle.y;

            cameraTransform.localEulerAngles = new Vector3(
                cameraEulerPunchApplied.x,
                0f,
                cameraEulerPunchApplied.z
            );
        }
    }

    /// <summary>
    /// Apply a punch/recoil to the camera. Should be called from a ServerRpc.
    /// </summary>
    /// <param name="punchAmount">The amount of punch to apply</param>
    public void ViewPunch(Vector2 punchAmount)
    {
        // Remove previous recoil
        punchAngle = Vector2.zero;

        // Recoil go up
        punchAngleVel -= punchAmount * 20;
    }

    /// <summary>
    /// Sets the rotation directly (useful for respawning or teleporting)
    /// </summary>
    public void SetRotation(Vector3 rotation)
    {
        realRotation = new Vector3(
            Mathf.Clamp(rotation.x, minYRotation, maxYRotation),
            rotation.y,
            0f
        );

        // Apply immediately
        if (bodyTransform != null)
        {
            bodyTransform.eulerAngles = Vector3.Scale(realRotation, new Vector3(0f, 1f, 0f));
        }

        if (cameraTransform != null)
        {
            cameraTransform.localEulerAngles = new Vector3(realRotation.x, 0f, 0f);
        }
    }

    /// <summary>
    /// Get the current real rotation (without punch)
    /// </summary>
    public Vector3 GetRealRotation()
    {
        return realRotation;
    }

    /// <summary>
    /// Get the current look direction
    /// </summary>
    public Vector3 GetLookDirection()
    {
        if (cameraTransform != null)
            return cameraTransform.forward;
        return transform.forward;
    }

    private void DecayPunchAngle()
    {
        if (punchAngle.sqrMagnitude > 0.001 || punchAngleVel.sqrMagnitude > 0.001)
        {
            punchAngle += punchAngleVel * Time.deltaTime;
            float damping = 1 - (punchDamping * Time.deltaTime);

            if (damping < 0)
                damping = 0;

            punchAngleVel *= damping;

            float springForceMagnitude = punchSpringConstant * Time.deltaTime;
            punchAngleVel -= punchAngle * springForceMagnitude;
        }
        else
        {
            punchAngle = Vector2.zero;
            punchAngleVel = Vector2.zero;
        }
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        // Only manage cursor for the local player
        if (!IsOwner)
            return;
    }
}