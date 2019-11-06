using System;
using System.Threading.Tasks;

namespace Microsoft.OneWeek.Hack.Microids.MessageRouter
{
    public class BlackHoleDataSink : IDataSink
    {
        public Task WriteMessageAsync(IMessage message)
        {
            Console.WriteLine(message.ToString());
            return Task.CompletedTask;
        }

    }
}