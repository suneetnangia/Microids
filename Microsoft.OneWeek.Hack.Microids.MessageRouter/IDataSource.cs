namespace Microsoft.OneWeek.Hack.Microids.MessageRouter
{

    public interface IDataSource
    {
        Message ReadMessage();
    }
}