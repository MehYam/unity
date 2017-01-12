using System;

namespace lifeEngine
{
    public struct Point<T> : IEquatable<Point<T>>
    {
        public T x;
        public T y;

        public Point(T x, T y)
        {
            this.x = x;
            this.y = y;
        }
        public Point(Point<T> rhs)
        {
            this.x = rhs.x;
            this.y = rhs.y;
        }
        public bool Equals(Point<T> other)
        {
            return x.Equals(other.x) && y.Equals(other.y);
        }
        public override bool Equals(object obj)
        {
            return obj is Point<T> && this == (Point<T>)obj;
        }
        public override int GetHashCode()
        {
            return x.GetHashCode() ^ y.GetHashCode();
        }
        public override string ToString()
        {
            return string.Format("{0:0.0},{1:0.0}", x, y);
        }
        public static bool operator ==(Point<T> a, Point<T> b)
        {
            return a.Equals(b);
        }
        public static bool operator !=(Point<T> a, Point<T> b)
        {
            return !a.Equals(b);
        }
    }
    public static class PointSpecialization
    {
        public static Point<float> ToFloat(this Point<int> point)
        {
            return new Point<float>(point.x, point.y);
        }
        public static Point<int> ToInt(this Point<float> point)
        {
            return new Point<int>((int)Math.Round(point.x), (int)Math.Round(point.y));
        }
    }
}
