using System.Diagnostics.CodeAnalysis;
using Yuniql.Extensibility;

namespace oed_authz.Helpers;

/// <summary>
/// Copied from sample project.
/// </summary>
[ExcludeFromCodeCoverage]
public class ConsoleTraceService : ITraceService
{
    /// <summary>
    /// Debug enabled
    /// </summary>
    public bool IsDebugEnabled { get; set; }

    /// <summary>
    /// Is traces sensitive
    /// </summary>
    public bool IsTraceSensitiveData { get; set; } = false;

    /// <summary>
    /// Trace to file enabled
    /// </summary>
    public bool IsTraceToFile { get; set; } = false;

    /// <inheritdoc/>>
    public bool IsTraceToDirectory { get; set; } = false;

    /// <inheritdoc/>>
    public string TraceDirectory { get; set; } = null!;

    /// <summary>
    /// Info
    /// </summary>
    public void Info(string message, object payload = null!)
    {
        var traceMessage = $"INF   {DateTime.UtcNow:o}   {message}{Environment.NewLine}";
        Console.Write(traceMessage);
    }

    /// <summary>
    /// Error
    /// </summary>
    public void Error(string message, object payload = null!)
    {
        var traceMessage = $"ERR   {DateTime.UtcNow:o}   {message}{Environment.NewLine}";
        Console.Write(traceMessage);
    }

    /// <summary>
    /// Debug
    /// </summary>
    public void Debug(string message, object payload = null!)
    {
        if (!IsDebugEnabled) return;
        var traceMessage = $"DBG   {DateTime.UtcNow:o}   {message}{Environment.NewLine}";
        Console.Write(traceMessage);
    }

    /// <summary>
    /// Success
    /// </summary>
    public void Success(string message, object payload = null!)
    {
        var traceMessage = $"INF   {DateTime.UtcNow:u}   {message}{Environment.NewLine}";
        Console.Write(traceMessage);
    }

    /// <summary>
    /// Warn
    /// </summary>
    public void Warn(string message, object payload = null!)
    {
        var traceMessage = $"WRN   {DateTime.UtcNow:o}   {message}{Environment.NewLine}";
        Console.Write(traceMessage);
    }
}
