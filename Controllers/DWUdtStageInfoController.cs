using System.Web.Http;
using Microsoft.Azure.Mobile.Server.Config;

namespace CloudBread.Controllers
{
    [MobileAppController]
    public class DWUdtStageInfoController : ApiController
    {
        // GET api/DWUdtStageInfo
        public string Get()
        {
            return "Hello from custom controller!";
        }
    }
}
