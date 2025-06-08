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
using UnderneathLayerAPI = TP.ConcurrentProgramming.Data.DataAbstractAPI;

namespace TP.ConcurrentProgramming.BusinessLogic
{
    internal class BusinessLogicImplementation : BusinessLogicAbstractAPI
    {
        #region ctor

        private readonly Infrastructure.ILogger? logger;

        internal BusinessLogicImplementation(UnderneathLayerAPI? underneathLayer = null, Infrastructure.ILogger? logger = null)
        {
            this.logger = logger;
            layerBellow = underneathLayer == null ? UnderneathLayerAPI.GetDataLayer(logger) : underneathLayer;
        }

        public BusinessLogicImplementation() : this(null)
        { }

        //internal BusinessLogicImplementation(UnderneathLayerAPI? underneathLayer)
        //{
        //    layerBellow = underneathLayer == null ? UnderneathLayerAPI.GetDataLayer() : underneathLayer;
        //}

        #endregion ctor

        #region BusinessLogicAbstractAPI

        public override void Dispose()
        {
            if (Disposed)
                throw new ObjectDisposedException(nameof(BusinessLogicImplementation));
            layerBellow.Dispose();
            logger?.Stop();
            Disposed = true;
        }

        public override void Start(int numberOfBalls, Action<IPosition, IBall> upperLayerHandler)
        {
            if (Disposed)
                throw new ObjectDisposedException(nameof(BusinessLogicImplementation));
            if (upperLayerHandler == null)
                throw new ArgumentNullException(nameof(upperLayerHandler));
            layerBellow.Start(numberOfBalls, (startingPosition, databall) => upperLayerHandler(new Position(startingPosition.x, startingPosition.y), AddBall(new Ball(databall))));
        }

        #endregion BusinessLogicAbstractAPI

        #region private

        private bool Disposed = false;

        private readonly UnderneathLayerAPI layerBellow;

        private readonly BallListMonitor ballMonitor = new();

        private double tableWidth = BusinessLogicAbstractAPI.GetDimensions.TableWidth;
        private double tableHeight = BusinessLogicAbstractAPI.GetDimensions.TableHeight;
        private double radius = BusinessLogicAbstractAPI.GetDimensions.BallDimension / 2;

        private IBall AddBall(IBall newBall)
        {
            ballMonitor.AddBall(newBall);
            newBall.NewPositionNotification += OnNewPosition;
            return newBall;
        }

        #endregion private

        #region TestingInfrastructure

        [Conditional("DEBUG")]
        internal void CheckObjectDisposed(Action<bool> returnInstanceDisposed)
        {
            returnInstanceDisposed(Disposed);
        }

        #endregion TestingInfrastructure
        //private void OnNewPosition(object? sender, IPosition position)
        //{
        //    if (sender == null) { return; }

        //    IBall senderBall = (Ball)sender;

        //    double newX = position.x;
        //    double newY = position.y;

        //    double velocityX = senderBall.Velocity.x;
        //    double velocityY = senderBall.Velocity.y;

        //    bool changed = false;

        //    if (newX - radius < 0)
        //    {
        //        newX = radius;
        //        velocityX = -velocityX;
        //        changed = true;
        //    }
        //    else if (newX + radius > tableWidth)
        //    {
        //        newX = tableWidth - radius;
        //        velocityX = -velocityX;
        //        changed = true;
        //    }

        //    if (newY - radius < 0)
        //    {
        //        newY = radius;
        //        velocityY = -velocityY;
        //        changed = true;
        //    }
        //    else if (newY + radius > tableHeight)
        //    {
        //        newY = tableHeight - radius;
        //        velocityY = -velocityY;
        //        changed = true;
        //    }

        //    IPosition newVelocity = new Position(velocityX, velocityY);
        //    logger?.Log(new Infrastructure.LogEntry("BusinessLogic", ((Ball)senderBall).GetHashCode(), newX, newY, DateTime.UtcNow));


        //    // Zmiana prędkości jeżeli changed jest true (jeżeli nastąpiła kolizja ze ścianą)
        //    if (changed) { senderBall.Velocity = newVelocity; }

        //    foreach (Ball otherBall in ballMonitor.GetBallsSnapshot())
        //    {
        //        if (otherBall == senderBall) continue;

        //        IPosition otherPosition = otherBall.Position;

        //        double dx = newX - otherPosition.x;
        //        double dy = newY - otherPosition.y;
        //        double distanceSquared = dx * dx + dy * dy;
        //        double radiusSum = Data.DataAbstractAPI.BallDiameter;

        //        if (distanceSquared < radiusSum * radiusSum)
        //        {
        //            HandleCollision(senderBall, otherBall, position, otherPosition);
        //            logger?.Log(new Infrastructure.LogEntry("BusinessLogic", ((Ball)senderBall).GetHashCode(), newX, newY, DateTime.UtcNow));


        //        }

        //    }

        //}


        private void OnNewPosition(object? sender, IPosition position)
        {
            if (sender == null) return;

            IBall senderBall = (Ball)sender;

            double newX = position.x;
            double newY = position.y;

            double velocityX = senderBall.Velocity.x;
            double velocityY = senderBall.Velocity.y;

            bool hitWall = false;

            // Odbicie od ścian
            if (newX - radius < 0)
            {
                newX = radius;
                velocityX = -velocityX;
                hitWall = true;
            }
            else if (newX + radius > tableWidth)
            {
                newX = tableWidth - radius;
                velocityX = -velocityX;
                hitWall = true;
            }

            if (newY - radius < 0)
            {
                newY = radius;
                velocityY = -velocityY;
                hitWall = true;
            }
            else if (newY + radius > tableHeight)
            {
                newY = tableHeight - radius;
                velocityY = -velocityY;
                hitWall = true;
            }

            if (hitWall)
            {
                logger?.Log(new Infrastructure.LogEntry("BusinessLogic", ((Ball)senderBall).GetHashCode(), newX, newY, DateTime.Now));
            }

            IPosition newVelocity = new Position(velocityX, velocityY);

            if (hitWall)
            {
                senderBall.Velocity = newVelocity;
            }

            foreach (Ball otherBall in ballMonitor.GetBallsSnapshot())
            {
                if (otherBall == senderBall) continue;

                IPosition otherPosition = otherBall.Position;

                double dx = newX - otherPosition.x;
                double dy = newY - otherPosition.y;
                double distanceSquared = dx * dx + dy * dy;
                double radiusSum = Data.DataAbstractAPI.BallDiameter;

                if (distanceSquared < radiusSum * radiusSum)
                {
                    HandleCollision(senderBall, otherBall, position, otherPosition);

                    // Loguj TYLKO jeśli naprawdę była kolizja
                    logger?.Log(new Infrastructure.LogEntry("BusinessLogic", ((Ball)senderBall).GetHashCode(), newX, newY, DateTime.Now));
                }
            }
        }


        private void HandleCollision(IBall b1, IBall b2, IPosition b1_position, IPosition b2_position)
        {
            double dx = b1_position.x - b2_position.x;
            double dy = b1_position.y - b2_position.y;

            IPosition this_vel = b1.Velocity;
            IPosition other_vel = b2.Velocity;

            double distSquared = dx * dx + dy * dy;
            double radiusSum = Data.DataAbstractAPI.BallDiameter;

            if (distSquared >= radiusSum * radiusSum)
                return;

            double dvx = this_vel.x - other_vel.x;
            double dvy = this_vel.y - other_vel.y;

            double dotProduct = dvx * dx + dvy * dy;
            if (dotProduct >= 0)
                return;

            double collisionScale = dotProduct / distSquared;
            double fx = collisionScale * dx;
            double fy = collisionScale * dy;

            IPosition newVelocityb1 = new Position(this_vel.x - fx, this_vel.y - fy);
            IPosition newVelocityb2 = new Position(other_vel.x + fx, other_vel.y + fy);

            b1.Velocity = newVelocityb1;
            b2.Velocity = newVelocityb2;
        }

        internal class BallListMonitor : HoareMonitor
        {
            private readonly List<IBall> balls = new();

            public void AddBall(IBall newBall)
            {
                EnterMonitor();
                balls.Add(newBall);
                ExitMonitor();
            }

            public List<IBall> GetBallsSnapshot()
            {
                EnterMonitor();
                List <IBall> listOfBalls = balls.ToList();
                ExitMonitor();
                return listOfBalls;
            }

            protected override ISignal CreateSignal()
            {
                throw new NotImplementedException("CreateSignal not used in this monitor.");
            }
        }
    }
}