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
using TP.ConcurrentProgramming.Presentation.Model;
using TP.ConcurrentProgramming.Presentation.ViewModel.MVVMLight;
using ModelIBall = TP.ConcurrentProgramming.Presentation.Model.IBall;

namespace TP.ConcurrentProgramming.Presentation.ViewModel
{
  public class MainWindowViewModel : ViewModelBase, IDisposable
  {
    #region ctor

    public MainWindowViewModel() : this(null)
    {
            StartCommand = new RelayCommand(() =>
            {
                Balls.Clear();
                Start(BallsCount);
                CanStart = false; // Dezaktywuj przycisk po pierwszym kliknięciu
            }, () => CanStart);
        }

        internal MainWindowViewModel(ModelAbstractApi modelLayerAPI)
        {
            ModelLayer = modelLayerAPI == null ? ModelAbstractApi.CreateModel() : modelLayerAPI;
            Observer = ModelLayer.Subscribe<ModelIBall>(x => Balls.Add(x));
        }

        public RelayCommand StartCommand { get; }

        private int ballsCount = 5;
        public int BallsCount
        {
            get => ballsCount;
            set
            {
                if (ballsCount != value)
                {
                    ballsCount = value;
                    RaisePropertyChanged();
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
        public void UpdateTableSize(double width, double height)
        {
            if (Disposed)
                throw new ObjectDisposedException(nameof(MainWindowViewModel));
            ModelLayer.UpdateTableSize(width, height);

            DebugDimensions = $"Table Width: {width:F2}, Height: {height:F2}";
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
}