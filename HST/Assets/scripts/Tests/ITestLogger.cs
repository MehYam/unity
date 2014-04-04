using UnityEngine;
using System.Collections;

public interface ITestLogger
{
    void LogPass(string name, string msg);
    void LogFailure(string name, string msg);
}
