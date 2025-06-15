//__________________________________________________________________________________________
//
//  Copyright 2024 Mariusz Postol LODZ POLAND.
//
//  To be in touch join the community by pressing the `Watch` button and to get started
//  comment using the discussion panel at
//  https://github.com/mpostol/TP/discussions/182
//__________________________________________________________________________________________

using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using TP.ConcurrentProgramming.Presentation.Model;
using TP.ConcurrentProgramming.Presentation.ViewModel.MVVMLight;
using ModelIBall = TP.ConcurrentProgramming.Presentation.Model.IBall;


namespace TP.ConcurrentProgramming.Presentation.ViewModel
{
  public class MainWindowViewModel : ViewModelBase, IDisposable
  {
    #region ctor
    public MainWindowViewModel() : this(ModelAbstractApi.CreateModel())
    {
        StartCommand = new RelayCommand(() =>
        {
            Balls.Clear();
            Start(BallsCount);
            CanStart = false;
        }, () => CanStart);
    }

    internal MainWindowViewModel(ModelAbstractApi modelLayerAPI)
    {
        ModelLayer = modelLayerAPI == null ? ModelAbstractApi.CreateModel() : modelLayerAPI;

        canvasWidth = ModelLayer.GetCanvasWidth();
        canvasHeight = ModelLayer.GetCanvasHeight();
        ballDimension = ModelLayer.GetBallDimension();
        Observer = ModelLayer.Subscribe<ModelIBall>(ball =>
        {
            Balls.Add(ball);
            BallViewModel vm = new BallViewModel(ball) { ScaleFactor = this.ScaleFactor };

            BallViewModels.Add(vm);
        });

    }

    public RelayCommand StartCommand { get; }

    private double canvasWidth;

    public double WidthProportions
    {
        get => canvasWidth;
    }
    public double CanvasWidth
    {
        get => canvasWidth * ScaleFactor + 20;
    }

    public double HeightProportions
    {
        get => canvasHeight;
    }

    private double canvasHeight;
    public double CanvasHeight
    {
        get => canvasHeight * ScaleFactor + 20;
    }

    private double ballDimension;
    public double BallDimension
    {
        get => ballDimension * ScaleFactor;
    }

    private int ballsCount = 5;
    public int BallsCount
    {
        get => ballsCount;
        set
        {
            if (ballsCount != value)
            {
                // Walidacja: tylko liczby w zakresie 1-10
                if (value < 1)
                    ballsCount = 1;
                else if (value > 10)
                    ballsCount = 10;
                else
                    ballsCount = value;

                RaisePropertyChanged();
            }
        }
    }

    private double scaleFactor = 1.0;
    public double ScaleFactor
    {
        get => scaleFactor;
        set
        {
            if (scaleFactor != value)
            {
                scaleFactor = value;
                RaisePropertyChanged(nameof(CanvasWidth));
                RaisePropertyChanged(nameof(CanvasHeight));
                RaisePropertyChanged(nameof(BallDimension));

                    foreach (BallViewModel ballViewModel in BallViewModels)
                    {
                        ballViewModel.ScaleFactor = scaleFactor;
                    }

                }
            }
    }

    private bool canStart = true;
    public bool CanStart
    {
        get => canStart;
        private set
        {
            if (canStart != value)
            {
                canStart = value;
                RaisePropertyChanged();
                StartCommand.RaiseCanExecuteChanged(); // bardzo ważne!
            }
        }
    }

    #endregion ctor

    #region public API

    public void Start(int numberOfBalls)
    {
      if (Disposed)
        throw new ObjectDisposedException(nameof(MainWindowViewModel));
      ModelLayer.Start(numberOfBalls);
    }

    public ObservableCollection<ModelIBall> Balls { get; } = new ObservableCollection<ModelIBall>();

    public ObservableCollection<BallViewModel> BallViewModels { get; } = new();

    #endregion public API

    #region IDisposable

    protected virtual void Dispose(bool disposing)
    {
      if (!Disposed)
      {
        if (disposing)
        {
          Balls.Clear();
          Observer.Dispose();
          ModelLayer.Dispose();
        }

        // TODO: free unmanaged resources (unmanaged objects) and override finalizer
        // TODO: set large fields to null
        Disposed = true;
      }
    }

    private string debugDimensions;
    public string DebugDimensions
    {
        get => debugDimensions;
        set
        {
            if (debugDimensions != value)
            {
                debugDimensions = value;
                RaisePropertyChanged();
            }
        }
    }
    public void UpdateDimensions(double windowWidth, double windowHeight)
    {
    }
    public void Dispose()
    {
      if (Disposed)
        throw new ObjectDisposedException(nameof(MainWindowViewModel));
      Dispose(disposing: true);
      GC.SuppressFinalize(this);
    }

    #endregion IDisposable

    #region private

    private IDisposable Observer = null;
    private ModelAbstractApi ModelLayer;
    private bool Disposed = false;

    #endregion private
  }

    public class BallViewModel : INotifyPropertyChanged
    {
        private readonly IBall ball;
        public BallViewModel(IBall ball) 
        {
            this.ball = ball;
            ball.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(ball.Left))
                    RaisePropertyChanged(nameof(ScaledLeft));
                else if (e.PropertyName == nameof(ball.Top))
                    RaisePropertyChanged(nameof(ScaledTop));
            };
        }

        public double Left => ball.Left;
        public double Top => ball.Top;
        public double Diameter => ball.Diameter;

        private double _scaleFactor = 1.0;
        public double ScaleFactor
        {
            get => _scaleFactor;
            set
            {
                _scaleFactor = value;
                RaisePropertyChanged(nameof(ScaledLeft));
                RaisePropertyChanged(nameof(ScaledTop));
                RaisePropertyChanged(nameof(ScaledDiameter));
            }
        }
        public double ScaledLeft => Left * ScaleFactor;
        public double ScaledTop => Top * ScaleFactor;
        public double ScaledDiameter => Diameter * ScaleFactor;

        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged(string propName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
    }

}