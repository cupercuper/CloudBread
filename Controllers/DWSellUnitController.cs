using System.Web.Http;
using Microsoft.Azure.Mobile.Server.Config;

namespace CloudBread.Controllers
{
    [MobileAppController]
    public class DWSellUnitController : ApiController
    {
        // GET api/DWSellUnit
        public string Get()
        {
            return "Hello from custom controller!";
        }
    }
}
