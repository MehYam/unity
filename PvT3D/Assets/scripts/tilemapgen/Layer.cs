using System;
using System.Text;

namespace lifeEngine
{
    public sealed class Layer<T>
    {
        readonly T[,] tiles;

        public readonly Point<int> size;
        public Layer(int width, int height)
        {
            tiles = new T[width, height];
            this.size = new Point<int>(width, height);
        }
        public Layer(Layer<T> rhs)
        {
            tiles = new T[rhs.size.x, rhs.size.y];
            this.size = new Point<int>(rhs.size.x, rhs.size.y);

            ForEach((x, y, tile) =>
            {
                //KAI: latent bug - this is a shallow copy, which happens to work now because T is always a value type (i.e. struct Tile)
                tiles[x, y] = rhs.Get(x, y);
            });
        }
        public T Get(int x, int y)
        {
            return tiles[x, y];
        }
        public void Set(int x, int y, T t)
        {
            tiles[x, y] = t;
        }
        public T Get(Point<int> pos)
        {
            return tiles[pos.x, pos.y];
        }
        public void Set(Point<int> pos, T t)
        {
            tiles[pos.x, pos.y] = t;
        }
        public bool IsValid(Point<int> pos)
        {
            return pos.x >= 0 && pos.y >= 0 && pos.x < size.x && pos.y < size.y;
        }
        /// <summary>
        ///  Iterate the layer, invoking the callback with (x, y, item) arguments
        /// </summary>
        /// <param name="callback"></param>
        public void ForEach(Action<int, int, T> callback)
        {
            for (var y = size.y-1; y >= 0; --y)
            {
                for (var x = 0; x < size.x; ++x)
                {
                    callback(x, y, tiles[x, y]);
                }
            }
        }
        public void Fill(Func<int, int, T> callback)
        {
            for (var y = 0; y < size.y; ++y)
            {
                for (var x = 0; x < size.x; ++x)
                {
                    tiles[x, y] = callback(x, y);
                }
            }
        }
        public override string ToString()
        {
            var sb = new StringBuilder();
            int currentY = size.y - 1;
            ForEach((int x, int y, T t) =>
            {
                if (currentY != y)
                {
                    currentY = y;
                    sb.AppendLine();
                }
                sb.Append(t.ToString());
            });
            return sb.ToString();
        }
    }
}
