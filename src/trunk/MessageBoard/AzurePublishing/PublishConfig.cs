using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzurePublishing
{
    class PublishConfig
    {
        public string WebRoleCsProjPath { get; set; }
        public string ConnectionString { get; set; }
        public string ServiceDefinitonPath { get; set; }
        public string WebRoleName { get; set; }
        public string SiteName { get; set; }
        public string CSPackPath { get; set; }
    }
}
