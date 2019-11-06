using Microsoft.Extensions.Configuration;
using System.Collections.Generic;

namespace Microsoft.OneWeek.Hack.Microids.MessageRouter.Tests
{
    public abstract class ConfigHelper
    {
        public static IConfiguration CreateConfig(Dictionary<string, string> configValues)
        {
            return new ConfigurationBuilder()
                .AddInMemoryCollection(configValues)
                .Build();
        }
    }
}
