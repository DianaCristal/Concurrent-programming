//____________________________________________________________________________________________________________________________________
//
//  Copyright (C) 2024, Mariusz Postol LODZ POLAND.
//
//  To be in touch join the community by pressing the `Watch` button and get started commenting using the discussion panel at
//
//  https://github.com/mpostol/TP/discussions/182
//
//_____________________________________________________________________________________________________________________________________

using TP.ConcurrentProgramming.Data;

namespace TP.ConcurrentProgramming.BusinessLogic.Test
{
    [TestClass]
    public class BusinessLogicImplementationUnitTest
    {
        [TestMethod]
        public void ConstructorTestMethod()
        {
            using (BusinessLogicImplementation newInstance = new(new DataLayerConstructorFixcure()))
            {
                bool newInstanceDisposed = true;
                newInstance.CheckObjectDisposed(x => newInstanceDisposed = x);
                Assert.IsFalse(newInstanceDisposed);
            }
        }

        [TestMethod]
        public void DisposeTestMethod()
        {
            DataLayerDisposeFixcure dataLayerFixcure = new DataLayerDisposeFixcure();
            BusinessLogicImplementation newInstance = new(dataLayerFixcure);
            Assert.IsFalse(dataLayerFixcure.Disposed);
            bool newInstanceDisposed = true;
            newInstance.CheckObjectDisposed(x => newInstanceDisposed = x);
            Assert.IsFalse(newInstanceDisposed);
            newInstance.Dispose();
            newInstance.CheckObjectDisposed(x => newInstanceDisposed = x);
            Assert.IsTrue(newInstanceDisposed);
            Assert.ThrowsException<ObjectDisposedException>(() => newInstance.Dispose());
            Assert.ThrowsException<ObjectDisposedException>(() => newInstance.Start(0, (position, ball) => { }));
            Assert.IsTrue(dataLayerFixcure.Disposed);
        }

        [TestMethod]
        public void StartTestMethod()
        {
            DataLayerStartFixcure dataLayerFixcure = new();
            using (BusinessLogicImplementation newInstance = new(dataLayerFixcure))
            {
                int called = 0;
                int numberOfBalls2Create = 10;
                newInstance.Start(
                  numberOfBalls2Create,
                  (startingPosition, ball) =>
                  {
                      called++;
                      Assert.IsNotNull(startingPosition);
                      Assert.IsNotNull(ball);

                      // Symulujemy wywołanie eventu przez piłkę, żeby przetestować flow OnNewPosition:
                      if (ball is Ball logicBall)
                      {
                          // Zakładamy, że znamy DataBallFixture (testowe IBall)
                          DataLayerStartFixcure.DataBallFixture? privateDataBall = dataLayerFixcure.LastCreatedBall;
                          privateDataBall?.TriggerNewPosition();
                      }
                  });

                Assert.AreEqual(1, called);
                Assert.IsTrue(dataLayerFixcure.StartCalled);
                Assert.AreEqual(numberOfBalls2Create, dataLayerFixcure.NumberOfBallseCreated);
            }
        }


        #region testing instrumentation

        private class DataLayerConstructorFixcure : Data.DataAbstractAPI
        {
            public override void Dispose()
            { }

            public override void Start(int numberOfBalls, Action<IVector, Data.IBall> upperLayerHandler)
            {
                throw new NotImplementedException();
            }
        }

        private class DataLayerDisposeFixcure : Data.DataAbstractAPI
        {
            internal bool Disposed = false;

            public override void Dispose()
            {
                Disposed = true;
            }

            public override void Start(int numberOfBalls, Action<IVector, Data.IBall> upperLayerHandler)
            {
                throw new NotImplementedException();
            }
        }

        internal class DataLayerStartFixcure : Data.DataAbstractAPI
        {
            internal bool StartCalled = false;
            internal int NumberOfBallseCreated = -1;
            internal DataBallFixture? LastCreatedBall;

            public override void Dispose() { }

            public override void Start(int numberOfBalls, Action<IVector, Data.IBall> upperLayerHandler)
            {
                StartCalled = true;
                NumberOfBallseCreated = numberOfBalls;
                LastCreatedBall = new DataBallFixture();
                upperLayerHandler(new DataVectorFixture(), LastCreatedBall);
            }

            private record DataVectorFixture : Data.IVector
            {
                public double x { get; init; }
                public double y { get; init; }
            }

            internal class DataBallFixture : Data.IBall
            {
                public IVector Velocity { get; set; } = new DataVectorFixture { x = 1, y = 1 };

                public IVector Position { get; private set; } = new DataVectorFixture { x = 100, y = 100 };

                public double Mass => 1.0;

                public double Diameter => 10.0;

                public event EventHandler<IVector>? NewPositionNotification;

                public void TriggerNewPosition()
                {
                    NewPositionNotification?.Invoke(this, new DataVectorFixture { x = 100, y = 100 });
                }

                public void CorrectPosition(double dx, double dy)
                {
                    Position = new DataVectorFixture { x = Position.x + dx, y = Position.y + dy };
                }
            }
        }

        #endregion testing instrumentation
    }
}