using System.Text.Json.Serialization;

namespace TP.ConcurrentProgramming.Data
{
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

    public enum WallSide
    {
        Left,
        Right,
        Top,
        Bottom
    }

    [JsonPolymorphic(TypeDiscriminatorPropertyName = "Type")]
    [JsonDerivedType(typeof(BallPositionLogEntry), "BallPosition")]
    [JsonDerivedType(typeof(BallCollisionLogEntry), "BallCollision")]
    [JsonDerivedType(typeof(WallCollisionLogEntry), "WallCollision")]
    [JsonDerivedType(typeof(BufferFullLogEntry), "BufferFull")]
    public abstract class LogEntry
    {
        public LogSource Source { get; init; }
        //public LogType Type { get; init; }
        public DateTime Timestamp { get; init; }
      
        protected LogEntry(LogSource source, LogType type, DateTime timestamp)
        {
            Source = source;
            //Type = type;
            Timestamp = timestamp;
        }
    }
    public class BallPositionLogEntry : LogEntry
    {
        public int BallId { get; init; }
        public double X { get; init; }
        public double Y { get; init; }

        public BallPositionLogEntry(int BallId, double X, double Y, DateTime timestamp)
            : base(LogSource.Data, LogType.BallPosition, timestamp)
        {
            this.BallId = BallId;
            this.X = X;
            this.Y = Y;
        }
    }
    public class BallCollisionLogEntry : LogEntry
    {
        public int BallId1 { get; init; }
        public double X1 { get; init; }
        public double Y1 { get; init; }
        public int BallId2 { get; init; }
        public double X2 { get; init; }
        public double Y2 { get; init; }

        public BallCollisionLogEntry(
            int ballId1, double x1, double y1,
            int ballId2, double x2, double y2,
            DateTime timestamp
        )
            : base(LogSource.Logic, LogType.BallCollision, timestamp)
        {
            BallId1 = ballId1;
            X1 = x1;
            Y1 = y1;
            BallId2 = ballId2;
            X2 = x2;
            Y2 = y2;
        }
    }
    public class WallCollisionLogEntry : LogEntry
    {
        public int BallId { get; init; }
        public double X { get; init; }
        public double Y { get; init; }

        public WallSide Wall { get; init; } 


        public WallCollisionLogEntry(int ballId, double x, double y, WallSide wall, DateTime timestamp)
            : base(LogSource.Logic, LogType.WallCollision, timestamp)
        {
            BallId = ballId;
            X = x;
            Y = y;
            Wall = wall;
        }
    }
    public class BufferFullLogEntry : LogEntry
    {
        public int DroppedCount { get; init; }

        public BufferFullLogEntry(int count, DateTime timestamp)
            : base(LogSource.Data, LogType.BufferFull, timestamp)
        {
            DroppedCount = count;
        }
    }
}