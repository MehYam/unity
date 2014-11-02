using UnityEngine;
using System.Collections;

public sealed class RateLimiter
{
    readonly float _min;
    readonly float _max;

    /// <summary>
    /// Constructs a limiter that tells you when baseRate seconds have elapsed
    /// </summary>
    /// <param name="baseRate">The number of seconds after which Now returns true</param>
    /// <param name="randomnessPct">Gives each quantum some randomness.  Passing in 0.5 makes gives rate a range of baseRate/2 to baseRase*3/2</param>
    public RateLimiter(float baseRate, float randomnessPct = 0)
    {
        var randomness = baseRate * randomnessPct;
        _min = baseRate - randomness;
        _max = baseRate + randomness;
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

        float delta = (_min != _max) ? Random.Range(_min, _max) : _min;
        _nextTime = Time.fixedTime + delta;

        ++numStarts;
    }
    public void End()
    {
        _nextTime = 0;
    }
    public override string ToString()
    {
        return string.Format("RateLimiter min {0} max {1}, next in {2}, started {3} times", _min, _max, _nextTime - Time.fixedTime, numStarts);
    }
}
