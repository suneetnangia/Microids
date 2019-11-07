using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Microsoft.OneWeek.Hack.Microids.IoTDevice.DeviceMetadata.Restful.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MetadataController : ControllerBase
    {
        private IDeviceMetadataRepository deviceMetadataRepository;

        public MetadataController(IDeviceMetadataRepository deviceMetadataRepository)
        {
            this.deviceMetadataRepository = deviceMetadataRepository;
        }

        [HttpGet]
        public async Task<DeviceMetadata> Get(string deviceId)
        {
            var deviceMetadata = this.deviceMetadataRepository.GetMetadata(deviceId);
            return await Task.FromResult(new DeviceMetadata() {Id= deviceMetadata.Id, Fqdn = deviceMetadata.Fqdn, Capability = deviceMetadata.Capability });
        }
    }
}