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

        private Vector _position;
        private IVector _velocity;

        public IVector Position
        {
            get
            {
                return _position;
            }
        }

        public void SetVelocity(double new_dx, double new_dy)
        {
            _velocity = new Vector(new_dx, new_dy);
        }

        public IVector Velocity
        {
            get
            {
                return _velocity;
            }
            init
            {
                _velocity = value;
            }
        }
        public double Diameter { get; } = 20.0;

        private Task? movementTask;
        private CancellationTokenSource? movementTokenSource;

        #endregion IBall

        #region private

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
                    _position = new Vector(_position.x + _velocity.x, _position.y + _velocity.y);

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
        #endregion private
    }
}