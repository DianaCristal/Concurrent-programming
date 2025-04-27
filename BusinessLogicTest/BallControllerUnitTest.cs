using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Threading;
using TP.ConcurrentProgramming.BusinessLogic;

namespace TP.ConcurrentProgramming.BusinessLogic.Test
{
    [TestClass]
    public class BallControllerUnitTest
    {
        [TestMethod]
        public void BallMovesAndBouncesCorrectlyTest()
        {
            try
            {
                // Ustawiamy wymiary stołu na realistyczne wartości
                BusinessLogicAbstractAPI.GetDimensions = new Dimensions(10.0, 300.0, 400.0);

                var fakeBall = new FakeDataBall(new LogicVector(20, 0)); // Początkowa prędkość w prawo
                var initialPosition = new Position(5, 150);              // Startowa pozycja przy lewej ścianie
                List<IPosition> positions = new List<IPosition>();

                IBall? capturedBall = null;

                // Tworzymy BallController i przechwytujemy początkową pozycję
                BallController controller = new BallController(fakeBall, initialPosition, (pos, ball) =>
                {
                    positions.Add(pos);
                    capturedBall = ball;
                });

                // Subskrybujemy zmiany pozycji
                Assert.IsNotNull(capturedBall);
                capturedBall!.NewPositionNotification += (sender, position) =>
                {
                    positions.Add(position);
                };

                Thread.Sleep(500);

                // Sprawdzamy, czy mamy co najmniej dwie pozycje: startową i ruch
                Assert.IsTrue(positions.Count >= 2, $"Zebrano mniej niż 2 pozycje: {positions.Count}");

                var movedPosition = positions[1]; // Pozycja po pierwszym ruchu

                // Sprawdzamy czy kulka przesunęła się w prawo
                Assert.IsTrue(movedPosition.x > initialPosition.x, $"Kulki nie przesunęła się w prawo: {movedPosition.x} <= {initialPosition.x}");

                // Teraz zmieniamy velocity na ujemne (ruch w lewo)
                fakeBall.SetVelocity(new LogicVector(-50, 0));

                Thread.Sleep(500);

                // Sprawdzamy, czy po odbiciu velocity zmieniło znak (czyli znowu w prawo)
                Assert.IsTrue(fakeBall.Velocity.x > 0, $"Velocity po odbiciu nie jest dodatnie: {fakeBall.Velocity.x}");
            }
            finally
            {
                // Przywracamy domyślne wymiary stołu, żeby inne testy działały
                BusinessLogicAbstractAPI.GetDimensions = new Dimensions(10.0, 10.0, 10.0);
            }
        }

        #region testing instrumentation

        private class FakeDataBall : Data.IBall
        {
            private Data.IVector _velocity;

            public FakeDataBall(Data.IVector initialVelocity)
            {
                _velocity = initialVelocity;
            }

            public Data.IVector Velocity
            {
                get => _velocity;
                set => _velocity = value;
            }

            public event EventHandler<Data.IVector>? NewPositionNotification;

            public void SetVelocity(Data.IVector newVelocity)
            {
                _velocity = newVelocity;
            }
        }

        #endregion testing instrumentation
    }
}
