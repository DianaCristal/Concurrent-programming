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
    public void MoveTestMethod()
    {
        Vector initialPosition = new(10.0, 10.0);
        Ball newInstance = new(initialPosition, new Vector(0.0, 0.0));
        IVector curentPosition = new Vector(0.0, 0.0);
        int numberOfCallBackCalled = 0;
        newInstance.NewPositionNotification += (sender, position) => { Assert.IsNotNull(sender); curentPosition = position; numberOfCallBackCalled++; };
        newInstance.Move();
        Assert.AreEqual<int>(1, numberOfCallBackCalled);
        Assert.AreEqual<IVector>(initialPosition, curentPosition);

        Ball newerInstance = new(initialPosition, new Vector(10.0, 10.0));
        IVector currenterPosition = new Vector(0.0, 0.0);
        newerInstance.NewPositionNotification += (sender, position) => { Assert.IsNotNull(sender); currenterPosition = position; numberOfCallBackCalled++; };
        newerInstance.Move();
        Vector newPosition = new(20.0, 20.0);
        Assert.AreEqual<IVector>(currenterPosition, newPosition);
        Assert.AreEqual<int>(2, numberOfCallBackCalled);
        }
    }
}