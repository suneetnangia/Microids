
namespace Microsoft.OneWeek.Hack.Microids.MessageRouter
{
    using Microsoft.OneWeek.Hack.Microids.Core;

    public interface IDataSink
    {
        void WriteMessage(Message message);
    }
}