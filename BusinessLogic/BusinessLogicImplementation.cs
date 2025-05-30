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

                    double dx = this.dataBall.Position.x - other.dataBall.Position.x;
                    double dy = this.dataBall.Position.y - other.dataBall.Position.y;
                    double distanceSquared = dx * dx + dy * dy;
                    double radiusSum = Data.DataAbstractAPI.BallDiameter;

                    if (distanceSquared < radiusSum * radiusSum)
                    {
                        HandleCollision(this.dataBall, other.dataBall);
                    }
                }
            }

        private void HandleCollision(Data.IBall b1, Data.IBall b2)
        {
            double dx = b1.Position.x - b2.Position.x;
            double dy = b1.Position.y - b2.Position.y;

            double distSquared = dx * dx + dy * dy;
            double radiusSum = Data.DataAbstractAPI.BallDiameter;

            if (distSquared >= radiusSum * radiusSum)
                return;

            double dvx = b1.Velocity.x - b2.Velocity.x;
            double dvy = b1.Velocity.y - b2.Velocity.y;

            double dotProduct = dvx * dx + dvy * dy;
            if (dotProduct >= 0)
                return;

            double collisionScale = dotProduct / distSquared;
            double fx = collisionScale * dx;
            double fy = collisionScale * dy;

            b1.SetVelocity(b1.Velocity.x - fx, b1.Velocity.y - fy);
            b2.SetVelocity(b2.Velocity.x + fx, b2.Velocity.y + fy);

            double overlap = (radiusSum - Math.Sqrt(distSquared)) / 2;
            double correctionX = (dx / Math.Sqrt(distSquared)) * overlap;
            double correctionY = (dy / Math.Sqrt(distSquared)) * overlap;

            b1.CorrectPosition(correctionX, correctionY);
            b2.CorrectPosition(-correctionX, -correctionY);

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