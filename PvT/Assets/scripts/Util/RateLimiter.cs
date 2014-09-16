using UnityEngine;
using System.Collections;

public sealed class RateLimiter
{
    readonly float _baseRate;
    readonly float _randomExtra;
    float _next;

    /// <summary>
    /// Constructs a limiter that tells you when baseRate seconds have elapsed
    /// </summary>
    /// <param name="baseRate">The number of seconds after which Now returns true</param>
    /// <param name="randomExtra">Pad the rate with random seconds on each interval</param>
    public RateLimiter(float baseRate, float randomExtra = 0)
    {
        _baseRate = baseRate;
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
            return Time.fixedTime > _next;
        }
    }
    public float timeRemaining
    {
        get
        {
            return Mathf.Max(0, _next - Time.fixedTime);
        }
    }
    public float baseRate
    {
        get
        {
            return _baseRate;
        }
    }
    public void Start()
    {
        float delta = (_randomExtra > 0) ? Random.Range(_baseRate, _randomExtra) : _baseRate;
        _next = Time.fixedTime + delta;

        ++numStarts;
    }
    public override string ToString()
    {
        return string.Format("RateLimiter base {0} random {1}, next in {2}, started {3} times", _baseRate, _randomExtra, _next - Time.fixedTime, numStarts);
    }
}
