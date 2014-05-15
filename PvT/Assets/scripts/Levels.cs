using UnityEngine;
using System.Collections;

public sealed class Levels
{
    static private Levels _instance;
    static public Levels Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new Levels();
            }
            return _instance;
        }
    }

    public sealed class Enemy
    {
        public readonly string name;
        public readonly int count;

        public Enemy(string name, int count) { this.name = name; this.count = count; }
    }

    public sealed class Wave
    {

    }
}
