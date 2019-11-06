using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace Microsoft.OneWeek.Hack.Microids.MessageRouter.Tests
{
    public class DataSinkUnitTests
    {
        private string ConfigKey = "TestConfigSetting";

        [Fact]
        public void VerifyMockDataSinkDoesNotThrow()
        {
            var myConfiguration = new Dictionary<string, string>
            {
                {ConfigKey, "Test Config Value"},
            };

            var config = new ConfigurationBuilder()
                    .AddInMemoryCollection(myConfiguration)
                    .Build();

            var mockDataSink = new Mock<IDataSink>();
            mockDataSink.Setup(x => x.WriteMessage(It.IsAny<string>()));
            mockDataSink.Object.WriteMessage(config.GetValue<string>(ConfigKey));
        }
    }
}
