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
        private double canvasWidth;
        private double canvasHeight;

        public ModelBall(double top, double left, LogicIBall underneathBall, double canvasWidth, double canvasHeight)
        {
            TopBackingField = top;
            LeftBackingField = left;
            this.canvasWidth = canvasWidth;
            this.canvasHeight = canvasHeight;
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

            // Obliczamy Top i Left dla środka kulki
            double top = e.y - radius;
            double left = e.x - radius;

            // Ograniczenie do obszaru Canvas (dynamiczne!)
            if (left < 0)
                left = 0;
            else if (left + Diameter > canvasWidth)
                left = canvasWidth - Diameter;

            if (top < 0)
                top = 0;
            else if (top + Diameter > canvasHeight)
                top = canvasHeight - Diameter;

            // Ustawiamy właściwości
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