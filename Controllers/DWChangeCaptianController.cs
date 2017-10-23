using System.Web.Http;
using Microsoft.Azure.Mobile.Server.Config;

namespace CloudBread.Controllers
{
    [MobileAppController]
    public class DWChangeCaptianController : ApiController
    {
        // GET api/DWChangeCaptian
        public string Get()
        {
            return "Hello from custom controller!";
        }
    }
}
