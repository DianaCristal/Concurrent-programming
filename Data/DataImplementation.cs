//____________________________________________________________________________________________________________________________________
//
//  Copyright (C) 2024, Mariusz Postol LODZ POLAND.
//
//  To be in touch join the community by pressing the `Watch` button and get started commenting using the discussion panel at
//
//  https://github.com/mpostol/TP/discussions/182
//
//_____________________________________________________________________________________________________________________________________

using System.Diagnostics;

namespace TP.ConcurrentProgramming.Data
{
  internal class DataImplementation : DataAbstractAPI
  {
        private readonly ILogger? logger;

        internal DataImplementation(ILogger? logger = null)
        {
            this.logger = logger ?? new Logger();
        }
        public override ILogger Logger => logger;

        public override void Start(int numberOfBalls, Action<IVector, IBall> upperLayerHandler)
        {
            if (Disposed)
                throw new ObjectDisposedException(nameof(DataImplementation));
            if (upperLayerHandler == null)
                throw new ArgumentNullException(nameof(upperLayerHandler));

            BallsList.Clear();
            Random random = new Random();
            for (int i = 0; i < numberOfBalls; i++)
            {
                Vector startingPosition = new(random.Next(100, 300 - 100), random.Next(100, 300 - 100));
                Ball newBall = new(i, startingPosition, new Vector((RandomGenerator.NextDouble() - 0.5) * 1000, (RandomGenerator.NextDouble() - 0.5) * 1000), logger);
                upperLayerHandler(startingPosition, newBall);
                BallsList.Add(newBall);
            }
            BeginBallMovement();
        }

    #region IDisposable

    protected virtual void Dispose(bool disposing)
    {
      if (!Disposed)
      {
        if (disposing)
        {
            Stop();
            BallsList.Clear();
            logger?.Stop();

        }
        Disposed = true;
      }
      else
        throw new ObjectDisposedException(nameof(DataImplementation));
    }

    public override void Dispose()
    {
      // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
      Dispose(disposing: true);
      GC.SuppressFinalize(this);
    }

    #endregion IDisposable

    #region private

    private bool Disposed = false;

    private Random RandomGenerator = new();
    private List<Ball> BallsList = [];

    #endregion private

    private void BeginBallMovement()
    {
        int i = 0;
        foreach (Ball ball in BallsList)
        {
                ball.StartMoving(i);
                i++;
        }
    }

    private void Stop()
    {
        foreach (Ball ball in BallsList)
        {
            ball.StopMove();
        }
    }

    #region TestingInfrastructure

    [Conditional("DEBUG")]
    internal void CheckBallsList(Action<IEnumerable<IBall>> returnBallsList)
    {
      returnBallsList(BallsList);
    }

    [Conditional("DEBUG")]
    internal void CheckNumberOfBalls(Action<int> returnNumberOfBalls)
    {
      returnNumberOfBalls(BallsList.Count);
    }

    [Conditional("DEBUG")]
    internal void CheckObjectDisposed(Action<bool> returnInstanceDisposed)
    {
      returnInstanceDisposed(Disposed);
    }

    #endregion TestingInfrastructure
  }
}