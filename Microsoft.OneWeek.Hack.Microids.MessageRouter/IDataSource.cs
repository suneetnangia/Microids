namespace Microsoft.OneWeek.Hack.Microids.MessageRouter
{
    using Microsoft.OneWeek.Hack.Microids.Core;

    public interface IDataSource
    {
        Message ReadMessage();
    }
}