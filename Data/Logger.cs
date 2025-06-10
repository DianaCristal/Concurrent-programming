using System.Collections.Concurrent;
using System.Text.Json;
using System.IO;
using System;
using System.Threading;
using System.Text.Json.Serialization;

namespace TP.ConcurrentProgramming.Infrastructure
{
    public interface ILogger
    {
        void Log(LogEntry entry);
        void Stop();
    }

    public enum LogSource
    {
        Data,
        Logic
    }

    public enum LogType
    {
        BallPosition,
        BallCollision,
        WallCollision,
        BufferFull,
        Error
    }

    public record LogEntry(
       LogSource Source,
       int BallId,
       double X,
       double Y,
       DateTime Timestamp,
       LogType Type
    );
    public class Logger : ILogger
    {
        private readonly BlockingCollection<LogEntry> _queue = new(500);
        private readonly Thread _writerThread;
        private readonly string _filePath;
        private static readonly object fileLock = new();

        public Logger()
        {
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            string projectRoot = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\.."));
            string logDirectory = Path.Combine(projectRoot, "Logs");
            Directory.CreateDirectory(logDirectory);
            _filePath = Path.Combine(logDirectory, $"diagnostics_{timestamp}.json");

            _writerThread = new Thread(Consume)
            {
                Name = "LoggerWriterThread"
            };
            _writerThread.Start();
        }

        public void Log(LogEntry entry)
        {
            if (!_queue.TryAdd(entry))
            {

                LogEntry overflowEntry = new LogEntry(
                    Source: LogSource.Data,
                    BallId: -1,
                    X: 0,
                    Y: 0,
                    Timestamp: DateTime.Now,
                    Type: LogType.BufferFull
                );

                try
                {
                    _queue.Add(overflowEntry);
                }
                catch
                {
                }
            }
        }

        private void Consume()
        {
            try
            {
                JsonSerializerOptions options = new JsonSerializerOptions
                {
                    Converters = { new JsonStringEnumConverter() },
                    WriteIndented = false
                };

                using StreamWriter writer = new StreamWriter(_filePath, append: true);
                foreach (LogEntry entry in _queue.GetConsumingEnumerable())
                {
                    string json = JsonSerializer.Serialize(entry, options);

                    writer.WriteLine(json);
                    writer.Flush();
                }
            }
            catch (Exception ex)
            {

            }
        }

        public void Stop()
        {
            _queue.CompleteAdding();
            _writerThread.Join();
        }
    }


}


