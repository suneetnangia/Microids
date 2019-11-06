
namespace Microsoft.OneWeek.Hack.Microids.MessageRouter
{

    public interface IDataSink
    {
        void WriteMessage(Message message);
    }
}