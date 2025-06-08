//____________________________________________________________________________________________________________________________________
//
//  Copyright (C) 2024, Mariusz Postol LODZ POLAND.
//
//  To be in touch join the community by pressing the `Watch` button and get started commenting using the discussion panel at
//
//  https://github.com/mpostol/TP/discussions/182
//
//_____________________________________________________________________________________________________________________________________

namespace TP.ConcurrentProgramming.BusinessLogic.Test
{
    [TestClass]
    public class BallUnitTest
    {
        [TestMethod]
        public void MoveTestMethod()
        {
            DataBallFixture dataBallFixture = new DataBallFixture();
            Ball newInstance = new(dataBallFixture);
            int numberOfCallBackCalled = 0;
            newInstance.NewPositionNotification += (sender, position) => { Assert.IsNotNull(sender); Assert.IsNotNull(position); numberOfCallBackCalled++; };
            dataBallFixture.Move();
            Assert.AreEqual<int>(1, numberOfCallBackCalled);
        }

        #region testing instrumentation

        private class DataBallFixture : Data.IBall
        {
            public int BallId => 123; // dowolny testowy identyfikator

            public Data.IVector Velocity { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

            public Data.IVector Position => throw new NotImplementedException();

            public double Mass => throw new NotImplementedException();

            public double Diameter => throw new NotImplementedException();

            private EventHandler<Data.IVector>? _newPositionNotification;

            event EventHandler<Data.IVector> Data.IBall.NewPositionNotification
            {
                add => _newPositionNotification += value;
                remove => _newPositionNotification -= value;
            }

            internal void Move()
            {
                _newPositionNotification?.Invoke(this, new VectorFixture(0.0, 0.0));
            }

            public void SetVelocity(double new_dx, double new_dy)
            {
                throw new NotImplementedException();
            }
        }

        private class VectorFixture : Data.IVector
        {
            internal VectorFixture(double X, double Y)
            {
                x = X; y = Y;
            }

            public double x { get; init; }
            public double y { get; init; }
        }

        #endregion testing instrumentation
    }
}