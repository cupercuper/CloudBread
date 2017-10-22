﻿using System.Web.Http;
using Microsoft.Azure.Mobile.Server.Config;

namespace CloudBread.Controllers
{
    [MobileAppController]
    public class DefaultController : ApiController
    {
        // GET api/Default
        public string Get()
        {
            return "Hello from custom controller!";
        }
    }
}
