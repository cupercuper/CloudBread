using System.Web.Http;
using Microsoft.Azure.Mobile.Server.Config;

namespace CloudBread.Controllers
{
    [MobileAppController]
    public class DWChangeUnitListController : ApiController
    {
        // GET api/DWChangeUnitList
        public string Get()
        {
            return "Hello from custom controller!";
        }
    }
}
