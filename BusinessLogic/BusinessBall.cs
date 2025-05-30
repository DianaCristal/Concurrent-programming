//____________________________________________________________________________________________________________________________________
//
//  Copyright (C) 2024, Mariusz Postol LODZ POLAND.
//
//  To be in touch join the community by pressing the `Watch` button and get started commenting using the discussion panel at
//
//  https://github.com/mpostol/TP/discussions/182
//
//_____________________________________________________________________________________________________________________________________

namespace TP.ConcurrentProgramming.BusinessLogic
{
  internal class Ball : IBall
  {
    public Ball(Data.IBall ball)
    {
            Position = new Position(0, 0);
            dataBall = ball;
            ball.NewPositionNotification += RaisePositionChangeEvent;
    }

    #region IBall

    public event EventHandler<IPosition>? NewPositionNotification;

    public IPosition Position { get; private set; }

    #endregion IBall

    #region private

    private readonly Data.IBall dataBall;

    private void RaisePositionChangeEvent(object? sender, Data.IVector e)
    {
            Position = new Position(e.x, e.y);
            NewPositionNotification?.Invoke(this, Position);
    }

    #endregion private
    }
}