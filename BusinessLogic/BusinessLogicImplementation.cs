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
                var controller = new BallController(databall, new Position(startingPosition.x, startingPosition.y), upperLayerHandler);
            });


        }

        public override void UpdateDimensions(double width, double height)
        {
            BusinessLogicAbstractAPI.GetDimensions = new Dimensions(10.0, 300.0, 400.0); // Stałe 400x300
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
        private readonly IBall wrappedBall;
        private IPosition currentPosition;
        private readonly Timer moveTimer;

        public BallController(Data.IBall ball, IPosition initialPosition, Action<IPosition, IBall> upperLayerHandler)
        {
            dataBall = ball;
            currentPosition = initialPosition;
            wrappedBall = new Ball(dataBall);

            // Pierwsze wywołanie do utworzenia ModelBall w warstwie Model
            upperLayerHandler(currentPosition, wrappedBall);

            moveTimer = new Timer(Move, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(20));
        }

        private void Move(object? state)
        {
            double tableWidth = BusinessLogicAbstractAPI.GetDimensions.TableWidth;
            double tableHeight = BusinessLogicAbstractAPI.GetDimensions.TableHeight;
            double radius = BusinessLogicAbstractAPI.GetDimensions.BallDimension / 2;

            double newX = currentPosition.x + dataBall.Velocity.x;
            double newY = currentPosition.y + dataBall.Velocity.y;

            double velocityX = dataBall.Velocity.x;
            double velocityY = dataBall.Velocity.y;

            // Odbicia od ścian
            if (newX - radius < 0 || newX + radius > tableWidth)
            {
                velocityX = -velocityX;
                newX = Clamp(newX, radius, tableWidth - radius);
            }

            if (newY - radius < 0 || newY + radius > tableHeight)
            {
                velocityY = -velocityY;
                newY = Clamp(newY, radius, tableHeight - radius);
            }

            // Zapis nowej prędkości
            dataBall.Velocity = new LogicVector(velocityX, velocityY);

            // Aktualizacja pozycji
            currentPosition = new Position(newX, newY);

            // AKTUALIZUJ widok — generuj event zmiany pozycji
            (wrappedBall as Ball)?.RaisePositionChangeEventManually(currentPosition);
        }

        private double Clamp(double value, double min, double max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }
    }

    }