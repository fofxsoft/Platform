using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace PlatformOne.API.Controllers
{

    [Route("api")]
    [ApiController]
    [Authorize]
    public class ValuesController : ControllerBase
    {

        [Route("values")]
        [HttpGet]
        public ActionResult<IEnumerable<string>> Get()
        {
            return new string[]
            {
                "value1",
                "value2"
            };
        }

        [Route("value/{id}")]
        [HttpGet]
        public ActionResult<IEnumerable<string>> Get(int id)
        {
            return new string[]
            {
                "value" + id
            };
        }

        [Route("distance")]
        [HttpGet]
        public ActionResult<IEnumerable<double>> Distance(double x1, double y1, double x2, double y2)
        {
            return new double[]
            {
                Math.Sqrt(Math.Pow(Math.Abs(x1 - x2), 2) + Math.Pow(Math.Abs(y1 - y2), 2))
            };
        }

        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
