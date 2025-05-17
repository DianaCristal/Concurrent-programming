//____________________________________________________________________________________________________________________________________
//
//  Copyright (C) 2024, Mariusz Postol LODZ POLAND.
//
//  To be in touch join the community by pressing the `Watch` button and get started commenting using the discussion panel at
//
//  https://github.com/mpostol/TP/discussions/182
//
//_____________________________________________________________________________________________________________________________________

namespace TP.ConcurrentProgramming.Data.Test
{
    [TestClass]
    public class BallUnitTest
    {
        [TestMethod]
        public void ConstructorTestMethod()
        {
            Vector testinVector = new Vector(0.0, 0.0);
            Ball newInstance = new(testinVector, testinVector);
        }

        [TestMethod]
        public async Task MoveTestMethodAsync()
        {
            Vector initialPosition = new(10.0, 10.0);
            Ball newInstance = new(initialPosition, new Vector(5.0, 5.0));

            IVector? currentPosition = initialPosition;
            int numberOfCallBackCalled = 0;
            AutoResetEvent positionUpdatedEvent = new(false);

            newInstance.NewPositionNotification += (sender, position) =>
            {
                Assert.IsNotNull(sender);
                currentPosition = position;
                numberOfCallBackCalled++;
                positionUpdatedEvent.Set();
            };

            Assert.AreEqual(initialPosition, currentPosition);

            newInstance.StartMoving();

            bool eventReceived = positionUpdatedEvent.WaitOne(100);

            newInstance.StopMove();

            Assert.IsTrue(eventReceived);
            Assert.IsNotNull(currentPosition);
            Assert.AreEqual(1, numberOfCallBackCalled);
            Assert.AreNotEqual(initialPosition, currentPosition);
        }

    }
}