using UnityEngine;

namespace Assets.Scripts.IsometricTools
{
    [ExecuteInEditMode]
    public class IsometricGrid : MonoBehaviour {

        // Use this for initialization
        public float GridStep = 1;
        public bool ShowGrid = true;
        public bool ShowSpritesLines = true;
        public static Vector2 IsoTo2D(Vector2 point)
        {
            return IsoTo2D(point.x, point.y);
        }

        public static Vector2 IsoTo2D(float x, float y)
        {
            return new Vector2(y + x * 0.5f, y - x * 0.5f);
        }

        public static Vector2 TwoDToIso(Vector2 point)
        {
            return TwoDToIso(point.x, point.y);
        }

        public static Vector2 TwoDToIso(float x, float y)
        {
            return new Vector2(x - y, (x + y) * 0.5f);
        }

        private const float MaxFloatValue = 100.0f;

        private static void DrawLines(float value)
        {
            var p0 = TwoDToIso(value, MaxFloatValue);
            var p1 = TwoDToIso(value, -MaxFloatValue);
            Gizmos.DrawLine(new Vector3(p0.x, p0.y, 0.0f), new Vector3(p1.x, p1.y, 0.0f));
            p0 = TwoDToIso(MaxFloatValue, value);
            p1 = TwoDToIso(-MaxFloatValue, value);
            Gizmos.DrawLine(new Vector3(p0.x, p0.y, 0.0f), new Vector3(p1.x, p1.y, 0.0f));
        }
        
        void OnDrawGizmos()
        {
            if (!ShowGrid) return;
            var oldColor = Gizmos.color;
            Gizmos.color = new Color(0.5f, 0.5f, 0.5f, 0.3f);
            var step = GridStep > 0.0f ? GridStep : 1.0f;
            for (float value = 0.0f; value < MaxFloatValue; value += step)
                DrawLines(value);
            for (float value = 0.0f; value > -MaxFloatValue; value -= step)
                DrawLines(value);
            Gizmos.color = oldColor;
        }
    }
}
