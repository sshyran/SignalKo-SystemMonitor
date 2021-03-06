﻿using NUnit.Framework;

using SignalKo.SystemMonitor.Agent.Core.Collectors;

namespace Agent.Core.Tests.IntegrationTests.Collectors
{
    [TestFixture]
    public class EnvironmentMachineNameProviderTests
    {
        [Test]
        public void GetMachineName_ResultIsNotNullOrEmpty()
        {
            // Arrange
            var machineNameProvider = new EnvironmentMachineNameProvider();

            // Act
            var result = machineNameProvider.GetMachineName();

            // Assert
            Assert.IsNotNullOrEmpty(result);
        }
    }
}