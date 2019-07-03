using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Route("test")]
    [Authorize]
    [ApiController]
    public class TestController : ControllerBase
    {
         public IActionResult Test() {
            string message = "Test";

            return new JsonResult(message);
        }
    }
}