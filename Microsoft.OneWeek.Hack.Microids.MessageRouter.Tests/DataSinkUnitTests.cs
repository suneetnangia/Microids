﻿using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace Microsoft.OneWeek.Hack.Microids.MessageRouter.Tests
{
    public class DataSinkUnitTests
    {
        private string ConfigKey = "TestConfigSetting";

        [Fact]
        public async void VerifyMockDataSinkDoesNotThrow()
        {
            var configValues = new Dictionary<string, string>
            {
                {ConfigKey, "Test Config Value"},
            };

            var config = ConfigHelper.CreateConfig(configValues);

            var mockDataSink = new Mock<IDataSink>();
            mockDataSink.Setup(x => x.WriteMessageAsync(It.IsAny<IMessage>()));
            
            await mockDataSink.Object.WriteMessageAsync(new MessageTypeA(Id: config.GetValue<string>(ConfigKey), Desc: "Any description"));
        }
    }
}
