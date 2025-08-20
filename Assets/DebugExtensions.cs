using UnityEngine;

public static class DebugExtensions
{
    public static void DrawCircle(Vector3 position, float radius, Color color, int segments = 32, float duration = 0)
    {
        // If the number of segments is less than 3, just draw a point
        if (segments < 3)
        {
            Debug.DrawRay(position, Vector3.up * radius, color, duration);
            return;
        }

        Vector3 prevPos = position + new Vector3(radius, 0, 0);
        for (int i = 0; i < segments + 1; i++)
        {
            float angle = (float)i / (float)segments * 360 * Mathf.Deg2Rad;
            Vector3 newPos = position + new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0);
            Debug.DrawLine(prevPos, newPos, color, duration);
            prevPos = newPos;
        }
    }
}