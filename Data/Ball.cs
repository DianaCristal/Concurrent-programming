//____________________________________________________________________________________________________________________________________
//
//  Copyright (C) 2024, Mariusz Postol LODZ POLAND.
//
//  To be in touch join the community by pressing the `Watch` button and get started commenting using the discussion panel at
//
//  https://github.com/mpostol/TP/discussions/182
//
//_____________________________________________________________________________________________________________________________________

using System.Threading;

namespace TP.ConcurrentProgramming.Data
{
    internal class Ball : IBall
    {
        #region ctor

        private readonly int id;
        private readonly ILogger? logger;
        private Vector? _lastLoggedPosition = null;
        private DateTime _lastLogTime = DateTime.MinValue;
        private readonly TimeSpan _logInterval = TimeSpan.FromMilliseconds(100);

        internal Ball(int id, Vector initialPosition, Vector initialVelocity, ILogger? logger = null)
        {
            this.id = id;
            _position = initialPosition;
            Velocity = initialVelocity;
            this.logger = logger;
        }

        #endregion ctor

        #region IBall

        public event EventHandler<IVector>? NewPositionNotification;

        private readonly double Diameter = DataAbstractAPI.BallDiameter;
        private Vector _position;
        private IVector _velocity;

        public int BallId
        {
            get
            {
                return id;
            }
        }

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

        private Thread? movementThread;
        private volatile bool stopThread = false;

        #endregion IBall

        #region private

        private void RaiseNewPositionChangeNotification()
        {
            NewPositionNotification?.Invoke(this, _position);
        }

        internal Thread StartMoving(int id)
        {
            movementThread = new Thread(() => ProcessMovement(id));
            movementThread.IsBackground = true;
            movementThread.Start();
            return movementThread;
        }
        private void Move(Vector delta)
        {
            Vector newPosition = new Vector(Position.x + delta.x, Position.y + delta.y);

            if ((_lastLoggedPosition == null || !_lastLoggedPosition.Equals(newPosition)) &&
                (DateTime.Now - _lastLogTime) > _logInterval)
            {
                logger?.Log(new BallPositionLogEntry(id, newPosition.x, newPosition.y, DateTime.Now));
                _lastLoggedPosition = newPosition;
                _lastLogTime = DateTime.Now;
            }

            _position = newPosition;
            RaiseNewPositionChangeNotification();
        }



        internal void StopMove()
        {
            stopThread = true;
            movementThread?.Join();
        }

        private void ProcessMovement(int id)
        {
            int currentIntervalMs;
            double distance = 0.1 * (Diameter/2);

            while (!stopThread)
            {
                try
                {
                    Vector velocity;

                    velocity = (Vector)Velocity;

                    currentIntervalMs = (int)((distance / (Math.Sqrt(Math.Pow(velocity.x, 2) + Math.Pow(velocity.y, 2)))) * 1000);
                    double currentIntervalS = currentIntervalMs / 1000.0;

                    Vector newPosition = new Vector(Velocity.x * currentIntervalS, Velocity.y * currentIntervalS);
                    Move(newPosition);

                    Thread.Sleep(currentIntervalMs);
                }
                catch (Exception ex)
                {
                }
            }
        }
        #endregion private
    }
}