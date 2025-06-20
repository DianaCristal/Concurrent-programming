﻿//__________________________________________________________________________________________
//
//  Copyright 2024 Mariusz Postol LODZ POLAND.
//
//  To be in touch join the community by pressing the `Watch` button and to get started
//  comment using the discussion panel at
//  https://github.com/mpostol/TP/discussions/182
//__________________________________________________________________________________________

using System;
using System.ComponentModel;
using TP.ConcurrentProgramming.BusinessLogic;

namespace TP.ConcurrentProgramming.Presentation.Model
{
  public interface IBall : INotifyPropertyChanged
  {
    double Top { get; }
    double Left { get; }
    double Diameter { get; }
  }

  public abstract class ModelAbstractApi : IObservable<IBall>, IDisposable
  {
    public static ModelAbstractApi CreateModel() => modelInstance.Value;

    public abstract double GetCanvasWidth();
    public abstract double GetCanvasHeight();
    public abstract double GetBallDimension();

    public abstract void Start(int numberOfBalls);

    #region IObservable

    public abstract IDisposable Subscribe(IObserver<IBall> observer);

    #endregion IObservable

    #region IDisposable

    public abstract void Dispose();

        #endregion IDisposable

        #region private

        //private static readonly Lazy<ModelAbstractApi> modelInstance = new Lazy<ModelAbstractApi>(() => new ModelImplementation());

        private static Lazy<ModelAbstractApi> modelInstance = new Lazy<ModelAbstractApi>(() => new ModelImplementation());

        #endregion private
    }
}