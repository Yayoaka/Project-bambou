using UnityEngine;

public class RandomPositionZone : MonoBehaviour
{
    public float firstRadius = 75f;
    public float secondRadius = 125f;
    public float maxRadius = 150f;

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        DrawCircle(transform.position, firstRadius);
        
        Gizmos.color = Color.green;
        DrawCircle(transform.position, secondRadius);

        Gizmos.color = Color.red;
        DrawCircle(transform.position, maxRadius);
    }

    private void DrawCircle(Vector3 center, float radius, int segments = 100)
    {
        Vector3 prevPoint = center + new Vector3(Mathf.Cos(0f) * radius, 0f, Mathf.Sin(0f) * radius);

        for (int i = 1; i <= segments; i++)
        {
            float angle = i * 2f * Mathf.PI / segments;
            Vector3 newPoint = center + new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius);

            Gizmos.DrawLine(prevPoint, newPoint);
            prevPoint = newPoint;
        }
    }
}