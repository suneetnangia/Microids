using System.Threading.Tasks;

namespace Microsoft.OneWeek.Hack.Microids.MessageRouter
{
    public interface IDataSink
    {
        Task WriteMessageAsync(IMessage message);
    }
}