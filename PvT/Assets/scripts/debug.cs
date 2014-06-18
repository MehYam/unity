using UnityEngine;
using System.Collections;

public class debug : MonoBehaviour
{
    public bool Log;

    void Awake() { Debug.Log("debug.Awake"); }
	void Start() { Debug.Log("debug.Start"); }
    void OnDisable() { Debug.Log("debug.OnDisable"); }
    void OnEnable() { Debug.Log("debug.OnEnable");  }
    struct Stats
    {
        public int updates;
        public int fixedUpdates;
        public float lastInterval;

        public void EndOfInterval()
        {
            fixedUpdates = 0;
            updates = 0;

            lastInterval = Time.time;
        }
    }
    Stats _stats;
	void Update()
    {
        ++_stats.updates;
    }
    void FixedUpdate()
    {
        ++_stats.fixedUpdates;

        var elapsed = Time.time - _stats.lastInterval;
        if (elapsed > 5)
        {
            if (_stats.lastInterval > 0)
            {
                PvT.Util.Util.Log("debug updates: {0}({2}/sec), fixedUpdates: {1}({3}/sec)", _stats.updates, _stats.fixedUpdates, _stats.updates/elapsed, _stats.fixedUpdates/elapsed);
            }
            _stats.EndOfInterval();
        }
    }
}
