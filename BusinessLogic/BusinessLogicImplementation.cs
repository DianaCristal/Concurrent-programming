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
      layerBellow.Start(numberOfBalls, (startingPosition, databall) => upperLayerHandler(new Position(startingPosition.x, startingPosition.x), new Ball(databall)));
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
        private readonly Data.IBall dataBall;
        private readonly Action<IPosition, IBall> upperLayerHandler;
        private readonly IBall wrappedBall;
        private IPosition currentPosition;

        private readonly double radius = BusinessLogicAbstractAPI.GetDimensions.BallDimension / 2;
        private readonly double tableWidth = BusinessLogicAbstractAPI.GetDimensions.TableWidth;
        private readonly double tableHeight = BusinessLogicAbstractAPI.GetDimensions.TableHeight;

        public BallController(Data.IBall ball, IPosition initialPosition, Action<IPosition, IBall> handler)
        {
            dataBall = ball;
            currentPosition = initialPosition;
            upperLayerHandler = handler;

            wrappedBall = new Ball(dataBall); // wrap w IBall z warstwy logic
            dataBall.NewPositionNotification += OnPositionUpdate;

            // Pierwsza pozycja (np. do View)
            upperLayerHandler(currentPosition, wrappedBall);
        }

        private void OnPositionUpdate(object? sender, Data.IVector newDelta)
        {
            // Nowa pozycja = aktualna + delta
            double newX = currentPosition.x + newDelta.x;
            double newY = currentPosition.y + newDelta.y;

            // Sprawdź odbicia
            double adjustedX = newX;
            double adjustedY = newY;

            double velocityX = dataBall.Velocity.x;
            double velocityY = dataBall.Velocity.y;

            if (adjustedX - radius < 0 || adjustedX + radius > tableWidth)
            {
                velocityX = -velocityX;
                adjustedX = currentPosition.x + velocityX;
            }

            if (adjustedY - radius < 0 || adjustedY + radius > tableHeight)
            {
                velocityY = -velocityY;
                adjustedY = currentPosition.y + velocityY;
            }

            // Zapisz nową prędkość (odbita)
            dataBall.Velocity = new LogicVector(velocityX, velocityY);

            // Zapisz nową pozycję
            currentPosition = new Position(adjustedX, adjustedY);

            // Powiadom prezentację
            upperLayerHandler(currentPosition, wrappedBall);
        }
    }

}