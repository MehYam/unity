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
    public RateLimiter(float baseRate, float randomness = 0)
    {
        _baseRate = baseRate;
        _randomness = randomness;

        Start();
    }
    public void Start()
    {
        float delta = (_randomness > 0) ? Random.Range(_baseRate, _randomness) : _baseRate;
        _next = Time.fixedTime + delta;
    }
    public bool reached
    {
        get
        {
            return Time.fixedTime > _next;
        }
    }
}
