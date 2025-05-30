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

    public BusinessLogicImplementation() : this(null)
    { }

        internal BusinessLogicImplementation(UnderneathLayerAPI? underneathLayer)
        {
            layerBellow = underneathLayer == null ? UnderneathLayerAPI.GetDataLayer() : underneathLayer;
        }

        #endregion ctor

        #region BusinessLogicAbstractAPI

        public override void Dispose()
    {
      if (Disposed)
        throw new ObjectDisposedException(nameof(BusinessLogicImplementation));
      layerBellow.Dispose();
      Disposed = true;
    }

    public override void Start(int numberOfBalls, Action<IPosition, IBall> upperLayerHandler)
    {
        if (Disposed)
            throw new ObjectDisposedException(nameof(BusinessLogicImplementation));
        if (upperLayerHandler == null)
            throw new ArgumentNullException(nameof(upperLayerHandler));
        layerBellow.Start(numberOfBalls, (startingPosition, databall) =>
        {
            BallController controller = new BallController(databall, new Position(startingPosition.x, startingPosition.y), upperLayerHandler);
        });
    }

    #endregion BusinessLogicAbstractAPI

    #region private

    private bool Disposed = false;

    private readonly UnderneathLayerAPI layerBellow;

    #endregion private

    #region TestingInfrastructure

    [Conditional("DEBUG")]
    internal void CheckObjectDisposed(Action<bool> returnInstanceDisposed)
    {
      returnInstanceDisposed(Disposed);
    }

    #endregion TestingInfrastructure
  }

    internal class BallController
    {
        private readonly TP.ConcurrentProgramming.Data.IBall dataBall;
        private readonly TP.ConcurrentProgramming.BusinessLogic.IBall logicBall;
        private static List<BallController> allControllers = new();
        private static readonly object syncLock = new();
        private static BallCollisionMonitor monitor = new();


        public BallController(Data.IBall ball, IPosition initialPosition, Action<IPosition, IBall> upperLayerHandler)
        {
            dataBall = ball;
            logicBall = new Ball(dataBall);

            upperLayerHandler(initialPosition, logicBall);

            logicBall.NewPositionNotification += OnNewPosition;

            monitor.RegisterController(this);

        }
        private void OnNewPosition(object? sender, IPosition position)
        {
            double tableWidth = BusinessLogicAbstractAPI.GetDimensions.TableWidth;
            double tableHeight = BusinessLogicAbstractAPI.GetDimensions.TableHeight;
            double radius = BusinessLogicAbstractAPI.GetDimensions.BallDimension / 2;

            double newX = position.x;
            double newY = position.y;

            double velocityX = dataBall.Velocity.x;
            double velocityY = dataBall.Velocity.y;

            bool changed = false;

            if (newX - radius < 0)
            {
                newX = radius;
                velocityX = -velocityX;
                changed = true;
            }
            else if (newX + radius > tableWidth)
            {
                newX = tableWidth - radius;
                velocityX = -velocityX;
                changed = true;
            }

            if (newY - radius < 0)
            {
                newY = radius;
                velocityY = -velocityY;
                changed = true;
            }
            else if (newY + radius > tableHeight)
            {
                newY = tableHeight - radius;
                velocityY = -velocityY;
                changed = true;
            }

            // Zmiana prędkości jeżeli changed jest true (jeżeli nastąpiła kolizja ze ścianą)
            if (changed) { dataBall.SetVelocity(velocityX, velocityY); }

            foreach (BallController other in monitor.GetControllers())
              {
                if (other == this) continue;

                    IPosition other_position = other.logicBall.Position;

                    double dx = newX - other_position.x;
                    double dy = newY - other_position.y;
                    double distanceSquared = dx * dx + dy * dy;
                    double radiusSum = Data.DataAbstractAPI.BallDiameter;

                    if (distanceSquared < radiusSum * radiusSum)
                    {
                        HandleCollision(this.dataBall, other.dataBall, position, other_position);
                    }
                }
            }

        private void HandleCollision(Data.IBall b1, Data.IBall b2, IPosition b1_position, IPosition b2_position)
        {
            double dx = b1_position.x - b2_position.x;
            double dy = b1_position.y - b2_position.y;

            Data.IVector this_vel = b1.Velocity;
            Data.IVector other_vel = b2.Velocity;

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

            b1.SetVelocity(this_vel.x - fx, this_vel.y - fy);
            b2.SetVelocity(other_vel.x + fx, other_vel.y + fy);
        }
    }

    internal class BallCollisionMonitor : HoareMonitor
    {
        private readonly List<BallController> controllers = new();
        private readonly ICondition collisionCondition;

        public BallCollisionMonitor()
        {
            collisionCondition = CreateCondition();
        }

        public void RegisterController(BallController ctrl)
        {
            EnterMonitor();
            controllers.Add(ctrl);
            ExitMonitor();
        }

        public List<BallController> GetControllers()
        {
            EnterMonitor();
            List<BallController> copy = controllers.ToList();

            ExitMonitor();
            return copy;
        }

        protected override ISignal CreateSignal()
        {
            throw new NotImplementedException("CreateSignal not used in this monitor.");
        }

    }

}