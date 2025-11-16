using System;
using Serilog.Core;
using Serilog.Events;

namespace FishingNewsWebScraper.Infrastructure;

/// <summary>
/// A minimal console sink used to emit logs directly to <see cref="System.Console"/>.
/// </summary>
public sealed class ConsoleLogEventSink : ILogEventSink
{
    private readonly IFormatProvider? _formatProvider;

    public ConsoleLogEventSink(IFormatProvider? formatProvider = null)
    {
        _formatProvider = formatProvider;
    }

    public void Emit(LogEvent logEvent)
    {
        if (logEvent is null)
        {
            return;
        }

        var timestamp = logEvent.Timestamp.ToString("yyyy-MM-dd HH:mm:ss");
        var level = logEvent.Level.ToString().ToUpperInvariant();
        var message = logEvent.RenderMessage(_formatProvider);

        Console.WriteLine($"{timestamp} [{level}] {message}");

        if (logEvent.Exception is not null)
        {
            Console.WriteLine(logEvent.Exception);
        }
    }
}
