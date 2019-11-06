using System.Threading.Tasks;

namespace Microsoft.OneWeek.Hack.Microids.MessageRouter
{
    public class TestGeneratorDataSource : IDataSource
    {

        private int NextMessageId = 1;

        Task<IMessage> IDataSource.ReadMessageAsync()
        {
            var msg = new MessageTypeA(
                Id: NextMessageId.ToString().PadLeft(3, '0'),
                Desc: "Fake Message"
            );
            
            NextMessageId++;
            if (NextMessageId > 2) NextMessageId = 1;

            return Task.FromResult((IMessage)msg);
        }
    }
}