
// This allows cross-platform logging between Unity and regular VS.NET

public interface ILogger
{
    void Log(string str);
    void LogResult(string str, bool pass);
}
