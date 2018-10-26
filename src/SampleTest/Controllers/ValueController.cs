using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;

namespace SampleTest.Controllers
{
    [Route("api/[controller]")]
    public class ValueController : Controller
    {
        [HttpGet]
        public ActionResult<IEnumerable<string>> Get()
        {
            return new string[] {"value1","value2"};
        }
    }
}