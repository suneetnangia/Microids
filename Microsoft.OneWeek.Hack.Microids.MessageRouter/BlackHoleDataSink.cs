using System;
using Microsoft.OneWeek.Hack.Microids.Core;

namespace Microsoft.OneWeek.Hack.Microids.MessageRouter
{
    public class BlackHoleDataSink : IDataSink
    {

        public void WriteMessage(Message message)
        {
            Console.WriteLine(message.ToString());
        }

    }
}