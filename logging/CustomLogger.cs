using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging.Configuration;
using Microsoft.Extensions.Options;

public class FileLoggerConfiguration
{
    public int EventId { get; set; }
    public string PathFolderName { get; set; } = "logs";
    public bool IsRollingFile { get; set; }
}

public class FileLogger : ILogger
{
    private readonly string name;
    private readonly Func<FileLoggerConfiguration> getCurrentConfig;
    public FileLogger(string name, Func<FileLoggerConfiguration> getCurrentConfig)
    {
        this.name = name;
        this.getCurrentConfig = getCurrentConfig;
    }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => default!;
    public bool IsEnabled(LogLevel logLevel) => true;
    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
        {
            return;
        }
        var config = getCurrentConfig();
        if (config.EventId == 0 ||
            config.EventId == eventId.Id)
        {
            string line = $"{name} - {formatter(state, exception)}";
            string fileName = config.IsRollingFile ? RollingFileName : FullFileName;
            string fullPath = Path.Combine(config.PathFolderName, fileName);
            File.AppendAllLines(fullPath, new[] { line });
        }
    }
    private static string RollingFileName => $"log-{DateTime.UtcNow:yyyy-MM-dd}.txt";
    private const string FullFileName = "logs.txt";
}

public class FileLoggerProvider : ILoggerProvider
{
    private readonly IDisposable? onChangeToken;
    private FileLoggerConfiguration currentConfig;
    private readonly ConcurrentDictionary<string, FileLogger> _loggers = new();
    public FileLoggerProvider(IOptionsMonitor<FileLoggerConfiguration> config)
    {
        currentConfig = config.CurrentValue;
        CheckDirectory();
        onChangeToken = config.OnChange(updateConfig =>
        {
            currentConfig = updateConfig;
            CheckDirectory();
        });
    }
    public ILogger CreateLogger(string categoryName)
    {
        return _loggers.GetOrAdd(categoryName, name => new FileLogger(name, () => currentConfig));
    }
    public void Dispose()
    {
        _loggers.Clear();
        onChangeToken?.Dispose();
    }
    private void CheckDirectory()
    {
        if (!Directory.Exists(currentConfig.PathFolderName))
        {
            Directory.CreateDirectory(currentConfig.PathFolderName);
        }
    }
}

public static class FileLoggerExtensions
{
    public static ILoggingBuilder AddFile(this ILoggingBuilder builder)
    {
        builder.AddConfiguration();
        builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, FileLoggerProvider>());
        LoggerProviderOptions.RegisterProviderOptions<FileLoggerConfiguration, FileLoggerProvider>(builder.Services);
        return builder;
    }
    public static ILoggingBuilder AddFile(this ILoggingBuilder builder, Action<FileLoggerConfiguration> configure)
    {
        builder.AddFile();
        builder.Services.Configure(configure);
        return builder;
    }
}