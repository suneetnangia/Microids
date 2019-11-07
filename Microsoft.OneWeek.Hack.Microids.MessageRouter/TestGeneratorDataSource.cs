using System;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.OneWeek.Hack.Microids.MessageRouter
{
    public class TestGeneratorDataSource : IDataSource
    {

        private Random generator = new Random();

        Task<IMessage> IDataSource.ReadMessageAsync()
        {
            int id = generator.Next(1, 3);
            var msg = new MessageTypeA(
                Id: id.ToString().PadLeft(3, '0'),
                Desc: "Fake Message"
            );
            return Task.FromResult((IMessage)msg);
        }
    }
}