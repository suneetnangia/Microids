using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Microsoft.OneWeek.Hack.Microids.MessageRouter
{
    public class BlackHoleDataSink : IDataSink
    {
        public BlackHoleDataSink(ILogger<BlackHoleDataSink> logger)
        {
            this.Logger = logger;
        }

        private ILogger<BlackHoleDataSink> Logger { get; set; }

        public Task WriteMessageAsync(IMessage message)
        {
            Logger.LogInformation(message.ToString());
            return Task.CompletedTask;
        }

    }
}