using System.Collections.Concurrent;
using System.Text.Json;
using System.IO;
using System;
using System.Threading;

namespace TP.ConcurrentProgramming.Infrastructure
{
    public interface ILogger
    {
        void Log(LogEntry entry);
        void Stop(); // zakończ i zapisz wszystko do pliku
    }

    public record LogEntry(
       string Source,    // np. "Data", "Logic"
       int BallId,
       double X,
       double Y,
       DateTime Timestamp
    );
    //internal class Logger : ILogger
    public class Logger : ILogger

    {
        private readonly BlockingCollection<LogEntry> _queue = new(200);
        private readonly Thread _workerThread;
        private readonly string _filePath = "diagnostics.json";

        public Logger()
        {
            _workerThread = new Thread(Consume) { IsBackground = true };
            _workerThread.Start();
        }

        //public void Log(LogEntry entry)
        //{
        //    if (!_queue.TryAdd(entry))
        //    {
        //        // Możesz dodać fallback lub logowanie awaryjne
        //    }
        //}

        public void Log(LogEntry entry)
        {
            Console.WriteLine($"LOG: {entry.Source} - {entry.BallId} at {entry.Timestamp}");
            if (!_queue.TryAdd(entry))
            {
                Console.WriteLine("Logger queue full or closed.");
            }
        }


        private void Consume()
        {
            using StreamWriter writer = new(_filePath, append: true);
            foreach (var entry in _queue.GetConsumingEnumerable())
            {
                string json = JsonSerializer.Serialize(entry);
                writer.WriteLine(json);
                writer.Flush(); // natychmiastowy zapis
            }
        }

        public void Stop()
        {
            _queue.CompleteAdding();
            _workerThread.Join(); // poczekaj aż wszystko zapisze
        }
    }


}