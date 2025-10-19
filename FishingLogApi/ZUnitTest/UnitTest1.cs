using FishingLogApi.Controllers;
using FishingLogApi.DAL.Models;
using FishingLogApi.DAL.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;

namespace FishingLogApi.DAL;

[TestClass]
public class VeidiferdIDControllerTests
{
    [TestMethod]
    public void Get_ReturnsNextIdAsString()
    {
        var mockRepo = new Mock<TripRepository>();
        mockRepo.Setup(r => r.NextTripId()).Returns("42");

        var controller = new VeidiferdIDController(mockRepo.Object);
        var result = controller.Get();

        Assert.AreEqual("42", result);
    }
}
