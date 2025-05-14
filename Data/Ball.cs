//____________________________________________________________________________________________________________________________________
//
//  Copyright (C) 2024, Mariusz Postol LODZ POLAND.
//
//  To be in touch join the community by pressing the `Watch` button and get started commenting using the discussion panel at
//
//  https://github.com/mpostol/TP/discussions/182
//
//_____________________________________________________________________________________________________________________________________

namespace TP.ConcurrentProgramming.Data
{
    internal class Ball : IBall
    {
        #region ctor

        internal Ball(Vector initialPosition, Vector initialVelocity)
        {
            _position = initialPosition;
            Velocity = initialVelocity;
        }

        #endregion ctor

        #region IBall

        public event EventHandler<IVector>? NewPositionNotification;

        public IVector Velocity { get; set; }

        public IVector Position => _position;

        public double Mass { get; } = 1.0; 
        public double Diameter { get; } = 20.0;

        private Task? movementTask;
        private CancellationTokenSource? movementTokenSource;

        #endregion IBall

        #region private

        private Vector _position;

        private void RaiseNewPositionChangeNotification()
        {
            NewPositionNotification?.Invoke(this, _position);
        }

        internal void StartMoving()
        {
            movementTokenSource = new CancellationTokenSource();
            movementTask = Task.Run(() => Move(movementTokenSource.Token));

        }
        private async Task Move(CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    _position = new Vector(_position.x + Velocity.x, _position.y + Velocity.y);
                    RaiseNewPositionChangeNotification();
                    await Task.Delay(20, token);
                }
            }
            catch (TaskCanceledException) { }
        }
        internal void StopMove()
        {
            movementTokenSource?.Cancel();
        }
        public void CorrectPosition(double dx, double dy)
        {
            _position = new Vector(_position.x + dx, _position.y + dy);
        }

        #endregion private
    }
}