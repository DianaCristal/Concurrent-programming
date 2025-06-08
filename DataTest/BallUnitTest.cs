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
            //Vector testinVector = new Vector(0.0, 0.0);
            //Ball newInstance = new(testinVector, testinVector);

            Vector position = new Vector(0.0, 0.0);
            Vector velocity = new Vector(0.0, 0.0);
            Ball newInstance = new Ball(0, position, velocity, null);
            Assert.IsNotNull(newInstance);
        }

        [TestMethod]
        public async Task MoveTestMethodAsync()
        {
            Vector initialPosition = new Vector(10.0, 10.0);
            Vector initialVelocity = new Vector(5.0, 5.0);
            Ball newInstance = new Ball(0, initialPosition, initialVelocity, null);

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

            newInstance.StartMoving(0);

            bool eventReceived = positionUpdatedEvent.WaitOne(100);
            await Task.Delay(10);

            newInstance.StopMove();

            Assert.IsTrue(eventReceived);
            Assert.IsNotNull(currentPosition);
            Assert.AreEqual(1, numberOfCallBackCalled);
            Assert.AreNotEqual(initialPosition, currentPosition);
        }


        //[TestMethod]
        //public async Task MoveTestMethodAsync()
        //{
        //    Vector initialPosition = new(10.0, 10.0);
        //    Ball newInstance = new(initialPosition, new Vector(5.0, 5.0));

        //    IVector? currentPosition = initialPosition;
        //    int numberOfCallBackCalled = 0;
        //    AutoResetEvent positionUpdatedEvent = new(false);

        //    newInstance.NewPositionNotification += (sender, position) =>
        //    {
        //        Assert.IsNotNull(sender);
        //        currentPosition = position;
        //        numberOfCallBackCalled++;
        //        positionUpdatedEvent.Set();
        //    };

        //    Assert.AreEqual(initialPosition, currentPosition);

        //    newInstance.StartMoving(0);

        //    bool eventReceived = positionUpdatedEvent.WaitOne(100);
        //    await Task.Delay(10); // <- zapewnia asynchroniczność

        //    newInstance.StopMove();

        //    Assert.IsTrue(eventReceived);
        //    Assert.IsNotNull(currentPosition);
        //    Assert.AreEqual(1, numberOfCallBackCalled);
        //    Assert.AreNotEqual(initialPosition, currentPosition);
        //}



    }


}