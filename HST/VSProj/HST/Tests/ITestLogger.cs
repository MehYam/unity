
public interface ITestLogger
{
    void Log(string name, string msg);
    void LogResult(string name, bool pass, string msg = null);
}
