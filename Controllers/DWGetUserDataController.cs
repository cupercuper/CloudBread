using System.Web.Http;
using Microsoft.Azure.Mobile.Server.Config;

namespace CloudBread.Controllers
{
    [MobileAppController]
    public class DWGetUserDataController : ApiController
    {
        // GET api/DWGetUserData
        public string Get()
        {
            return "Hello from custom controller!";
        }
    }
}
