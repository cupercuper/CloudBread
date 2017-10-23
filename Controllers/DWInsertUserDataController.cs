using System.Web.Http;
using Microsoft.Azure.Mobile.Server.Config;

namespace CloudBread.Controllers
{
    [MobileAppController]
    public class DWInsertUserDataController : ApiController
    {
        // GET api/DWInsertUserData
        public string Get()
        {
            return "Hello from custom controller!";
        }
    }
}
