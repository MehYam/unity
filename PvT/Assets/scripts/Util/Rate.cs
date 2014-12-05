using UnityEngine;
using System.Collections;

public struct Rate
{
    float _baseRate;
    float _deadline;
    readonly float _randomness;

    /// <summary>
    /// Constructs a limiter that tells you when baseRate seconds have elapsed
    /// </summary>
    /// <param name="baseRate">The number of seconds after which Now returns true</param>
    /// <param name="randomnessPct">Gives each quantum some randomness.  Passing in 0.5 makes gives rate a range of baseRate/2 to baseRase*3/2</param>
    public Rate(float baseRate, float randomnessPct)
    {
        _baseRate = baseRate;
        _deadline = 0;
        _randomness = randomnessPct;
        Start();
    }
    public Rate(float baseRate)
    {
        _baseRate = baseRate;
        _deadline = 0;
        _randomness = 0;
        Start();
    }
    public bool reached { get { return Time.fixedTime >= _deadline; } }
    public float remaining { get { return Mathf.Max(0, _deadline - Time.fixedTime); } }

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
        return string.Format("Rate base {0} randomness {1}, deadline {2}, seconds from now {3}, reached {4}", _baseRate, _randomness, _deadline, _deadline - Time.fixedTime, reached);
    }
}
