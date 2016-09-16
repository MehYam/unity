using UnityEngine;

using lifeEngine;

namespace life.util
{
    static class PointSpecialization
    {
        public static Vector2 ToVector2(this Point<float> point)
        {
            return new Vector2(point.x, point.y);
        }
        public static Vector2 ToVector2(this Point<int> point)
        {
            return new Vector2(point.x, point.y);
        }
    }
}