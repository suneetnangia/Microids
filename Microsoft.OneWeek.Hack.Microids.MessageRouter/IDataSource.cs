using System.Threading.Tasks;

namespace Microsoft.OneWeek.Hack.Microids.MessageRouter
{
    public interface IDataSource
    {
        Task<IMessage> ReadMessageAsync();
    }
}