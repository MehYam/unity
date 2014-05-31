using UnityEngine;
using System.Collections;

public sealed class RateLimiter
{
    readonly float _baseRate;
    readonly float _randomness;
    float _next;

    /// <summary>
    /// Constructs a limiter that tells you when baseRate seconds have elapsed
    /// </summary>
    /// <param name="baseRate">The number of seconds after which Now returns true</param>
    /// <param name="randomness">Pad the rate with random seconds on each interval</param>
    public RateLimiter(float baseRate, float randomness)
    {
        _baseRate = baseRate;
        _randomness = randomness;
    }

    public RateLimiter(float rate)
    {
        _baseRate = _randomness = rate;
    }

    public void Start()
    {
        float delta = Time.fixedTime + 
            _baseRate != _randomness ? Random.Range(_baseRate, _randomness) : _baseRate;

        _next = Time.fixedTime + delta;
    }

    public bool now
    {
        get
        {
            if (Time.fixedTime > _next)
            {
                Start();
                return true;
            }
            return false;
        }
    }
}
