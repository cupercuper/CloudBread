using System.Web.Http;
using Microsoft.Azure.Mobile.Server.Config;

namespace CloudBread.Controllers
{
    [MobileAppController]
    public class DWBuyUnitController : ApiController
    {
        // GET api/DWBuyUnit
        public string Get()
        {
            return "Hello from custom controller!";
        }
    }
}
