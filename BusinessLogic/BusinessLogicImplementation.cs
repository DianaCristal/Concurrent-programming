﻿//____________________________________________________________________________________________________________________________________
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

        public BallController(Data.IBall ball, IPosition initialPosition, Action<IPosition, IBall> upperLayerHandler)
        {
            dataBall = ball;
            logicBall = new Ball(dataBall);

            upperLayerHandler(initialPosition, logicBall);

            logicBall.NewPositionNotification += OnNewPosition;
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
            if (changed) { dataBall.Velocity = new LogicVector(velocityX, velocityY); }
        }

    }
}