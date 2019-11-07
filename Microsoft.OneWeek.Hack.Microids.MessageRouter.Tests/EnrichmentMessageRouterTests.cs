using Moq;
using Xunit;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.ApplicationInsights;
using Microsoft.OneWeek.Hack.Microids.Common;

namespace Microsoft.OneWeek.Hack.Microids.MessageRouter.Tests
{
    public class EnrichmentMessageRouterTests
    {
        [Fact]
        public async void PropertyRoutesAllMessages()
        {
            // Arrange
            var TOTAL_MESSAGES = 10;
            var deliveredMessages = 0;
            var receivedMessages = 0;
            var enrichedMessages = 0;

            var config = ConfigHelper.CreateConfig(new Dictionary<string, string>());

            var mockDataSink = new Mock<IDataSink>();
            var mockDataSource = new Mock<IDataSource>();
            var mockDataEnricher = new Mock<IIoTDeviceDataEnricher>();
            var mockTelemetryClient = new Mock<ITelemetryClient>();
            var mockLogger = new Mock<ILogger<EnrichmentMessageRouter>>();

            mockDataSink
                .Setup(x => x.WriteMessageAsync(It.IsAny<IMessage>()))
                .Callback(() => receivedMessages++);

            mockDataSource
                .Setup(x => x.ReadMessageAsync())
                .Callback(() => deliveredMessages++)
                .Returns(Task.FromResult((IMessage)new MessageTypeA(Id: "1", Desc: "Testing")));

            mockDataEnricher
                .Setup(x => x.GetMetadataAsync(It.IsAny<string>()))
                .Callback(() => enrichedMessages++)
                .Returns(Task.FromResult(new IoTDevice.DeviceMetadata()));

            var router = new EnrichmentMessageRouter(mockDataSource.Object,
                mockDataSink.Object,
                mockDataEnricher.Object,
                mockTelemetryClient.Object,
                config,
                mockLogger.Object);

            // Act
            var ct = new CancellationTokenSource();
            var task = router.Initiate(ct.Token);

            while (deliveredMessages < TOTAL_MESSAGES)
            {
                Debug.WriteLine($"Delivered {deliveredMessages} messages...waiting");
                await Task.Delay(1000);
            }

            ct.Cancel();

            // Assert
            Assert.Equal(TOTAL_MESSAGES, deliveredMessages);
            Assert.Equal(TOTAL_MESSAGES, receivedMessages);
            Assert.Equal(TOTAL_MESSAGES, enrichedMessages);
        }
    }
}