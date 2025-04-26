//____________________________________________________________________________________________________________________________________
//
//  Copyright (C) 2023, Mariusz Postol LODZ POLAND.
//
//  To be in touch join the community by pressing the `Watch` button and get started commenting using the discussion panel at
//
//  https://github.com/mpostol/TP/discussions/182
//
//  by introducing yourself and telling us what you do with this community.
//_____________________________________________________________________________________________________________________________________

using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using TP.ConcurrentProgramming.BusinessLogic;
using LogicIBall = TP.ConcurrentProgramming.BusinessLogic.IBall;

namespace TP.ConcurrentProgramming.Presentation.Model
{
  internal class ModelBall : IBall
  {
    public ModelBall(double top, double left, LogicIBall underneathBall)
    {
      TopBackingField = top;
      LeftBackingField = left;
      underneathBall.NewPositionNotification += NewPositionNotification;
    }

    #region IBall

    public double Top
    {
      get { return TopBackingField; }
      private set
      {
        if (TopBackingField == value)
          return;
        TopBackingField = value;
        RaisePropertyChanged();
      }
    }

    public double Left
    {
      get { return LeftBackingField; }
      private set
      {
        if (LeftBackingField == value)
          return;
        LeftBackingField = value;
        RaisePropertyChanged();
      }
    }

    public double Diameter { get; init; } = 0;

    #region INotifyPropertyChanged

    public event PropertyChangedEventHandler PropertyChanged;

    #endregion INotifyPropertyChanged

    #endregion IBall

    #region private

    private double TopBackingField;
    private double LeftBackingField;

        //private void NewPositionNotification(object sender, IPosition e)
        //{
        //    double radius = Diameter / 2;
        //    Top = e.y - radius; Left = e.x - radius;
        //}

        private void NewPositionNotification(object sender, IPosition e)
        {
            double radius = Diameter / 2;

            // Współrzędne środka kulki
            double centerX = e.x;
            double centerY = e.y;

            // Obliczamy Top i Left
            double top = centerY - radius;
            double left = centerX - radius;

            // Pobieramy wymiary Canvasu (trzeba jakoś je znać!)
            double canvasWidth = 800;  // <-- ustaw swoją faktyczną szerokość Canvasu
            double canvasHeight = 600; // <-- ustaw swoją faktyczną wysokość Canvasu

            // Ograniczenie: kulka nie może wyjść poza Canvas

            if (left < 0)
                left = 0;
            else if (left + Diameter > canvasWidth)
                left = canvasWidth - Diameter;

            if (top < 0)
                top = 0;
            else if (top + Diameter > canvasHeight)
                top = canvasHeight - Diameter;

            // Ustawiamy końcowe wartości
            Left = left;
            Top = top;
        }


        private void RaisePropertyChanged([CallerMemberName] string propertyName = "")
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    #endregion private

    #region testing instrumentation

    [Conditional("DEBUG")]
    internal void SetLeft(double x)
    { Left = x; }

    [Conditional("DEBUG")]
    internal void SettTop(double x)
    { Top = x; }

    #endregion testing instrumentation
  }
}