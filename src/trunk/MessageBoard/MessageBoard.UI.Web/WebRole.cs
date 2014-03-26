using System.Net;
using Microsoft.WindowsAzure.ServiceRuntime;

namespace MessageBoard.UI.Web
{
    public class WebRole : RoleEntryPoint
    {
        public override bool OnStart()
        {
            ServicePointManager.DefaultConnectionLimit = 300;
            return base.OnStart();
        }
    }
}
