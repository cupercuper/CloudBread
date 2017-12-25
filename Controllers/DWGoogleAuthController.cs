using System.Web.Http;
using Microsoft.Azure.Mobile.Server.Config;

namespace CloudBread.Controllers
{
    [MobileAppController]
    public class DWGoogleAuthController : ApiController
    {
        // GET api/DWGoogleAuth
        public string Get()
        {
            return "Hello from custom controller!";
        }

        // GET api/DWGoogleAuth
        public IHttpActionResult Index(string code)
        {
            return null;
        }

        public string Post()
        {
            return "Hello from custom controller Post!";
        }


    }
}
