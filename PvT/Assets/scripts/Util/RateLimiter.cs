using UnityEngine;
using System.Collections;

public sealed class RateLimiter
{
    readonly float _randomExtra;

    /// <summary>
    /// Constructs a limiter that tells you when baseRate seconds have elapsed
    /// </summary>
    /// <param name="baseRate">The number of seconds after which Now returns true</param>
    /// <param name="randomExtra">Pad the rate with random seconds on each interval</param>
    public RateLimiter(float baseRate, float randomExtra = 0)
    {
        this.baseRate = baseRate;
        _randomExtra = randomExtra;

        Start();
    }
    public int numStarts
    { 
        get; private set;
    }
    public bool reached
    {
        get
        {
            return Time.fixedTime >= _nextTime;
        }
    }
    public float timeRemaining
    {
        get
        {
            return Mathf.Max(0, _nextTime - Time.fixedTime);
        }
    }
    public float baseRate { get; private set; }
    public void Start()
    {
        Start(baseRate);
    }
    float _nextTime;
    public void Start(float newBaseRate)
    {
        baseRate = newBaseRate;

        float delta = (_randomExtra > 0) ? Random.Range(baseRate, _randomExtra) : baseRate;
        _nextTime = Time.fixedTime + delta;

        ++numStarts;
    }
    public override string ToString()
    {
        return string.Format("RateLimiter base {0} random {1}, next in {2}, started {3} times", baseRate, _randomExtra, _nextTime - Time.fixedTime, numStarts);
    }
}
