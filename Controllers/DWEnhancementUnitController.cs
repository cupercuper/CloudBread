using System.Web.Http;
using Microsoft.Azure.Mobile.Server.Config;

namespace CloudBread.Controllers
{
    [MobileAppController]
    public class DWEnhancementUnitController : ApiController
    {
        // GET api/DWEnhancementUnit
        public string Get()
        {
            return "Hello from custom controller!";
        }
    }
}
