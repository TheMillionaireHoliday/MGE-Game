using UnityEngine;

public class RespawnPointBox : MonoBehaviour
{
    private Color gizmoColor = Color.green;
    private Vector3 boxSize = new Vector3(1.5f, 2.5f, 1.5f);
    private bool showDirection = true;
    private float arrowLength = 1.5f;

    private void OnDrawGizmos()
    {
        // Draw the main box
        Gizmos.color = gizmoColor;
        Gizmos.DrawCube(transform.position, boxSize);

        // Draw wireframe for better visibility
        Gizmos.color = Color.white;
        Gizmos.DrawWireCube(transform.position, boxSize);

        // Draw direction arrow
        if (showDirection)
        {
            Gizmos.color = gizmoColor;
            Vector3 arrowStart = transform.position + Vector3.up * 0.5f;
            Vector3 arrowEnd = arrowStart + transform.forward * arrowLength;
            Gizmos.DrawLine(arrowStart, arrowEnd);

            // Draw arrowhead
            Vector3 direction = (arrowEnd - arrowStart).normalized;
            Vector3 right = Vector3.Cross(direction, Vector3.up).normalized;
            Vector3 up = Vector3.Cross(direction, right).normalized;

            Gizmos.DrawLine(arrowEnd, arrowEnd - direction * 0.3f + right * 0.15f);
            Gizmos.DrawLine(arrowEnd, arrowEnd - direction * 0.3f - right * 0.15f);
            Gizmos.DrawLine(arrowEnd, arrowEnd - direction * 0.3f + up * 0.15f);
            Gizmos.DrawLine(arrowEnd, arrowEnd - direction * 0.3f - up * 0.15f);
        }
    }

    private void OnDrawGizmosSelected()
    {
        // When selected, show additional info
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, boxSize * 1.2f);

        // Draw spawn radius
        Gizmos.color = new Color(1, 1, 0, 0.3f);
        Gizmos.DrawWireSphere(transform.position, 2f);
    }
}