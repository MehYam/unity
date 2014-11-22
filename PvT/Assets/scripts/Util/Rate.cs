using UnityEngine;
using System.Collections;

public sealed class Rate
{
    float _baseRate;
    readonly float _randomness;

    /// <summary>
    /// Constructs a limiter that tells you when baseRate seconds have elapsed
    /// </summary>
    /// <param name="baseRate">The number of seconds after which Now returns true</param>
    /// <param name="randomnessPct">Gives each quantum some randomness.  Passing in 0.5 makes gives rate a range of baseRate/2 to baseRase*3/2</param>
    public Rate(float baseRate = 0, float randomnessPct = 0)
    {
        _baseRate = baseRate;
        _randomness = randomnessPct;

        Start();
    }
    public int numStarts { get; private set; }
    public bool reached { get { return Time.fixedTime >= _deadline; } }
    public float remaining { get { return Mathf.Max(0, _deadline - Time.fixedTime); } }

    float _deadline;
    public void Start()
    {
        if (_randomness != 0)
        {
            float fuzziness = _randomness * _baseRate;
            _deadline = Time.fixedTime + Random.Range(_baseRate - fuzziness, _baseRate + fuzziness);
        }
        else
        {
            _deadline = Time.fixedTime + _baseRate;
        }
        ++numStarts;
    }
    public void Start(float newBaseRate)
    {
        _baseRate = newBaseRate;
        Start();
    }
    public void Stop()
    {
        _deadline = 0;
    }
    public override string ToString()
    {
        return string.Format("RateLimiter base {0} randomness {1}, next in {2}, started {3} times", _baseRate, _randomness, _deadline - Time.fixedTime, numStarts);
    }
}
