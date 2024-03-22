using Microsoft.AspNetCore.Mvc;
using TestServices;

namespace TestApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TestController : ControllerBase
{
    [HttpGet]
    [Route("testdata")]
    public IEnumerable<string> Get()
    {
        return new string[] { "value1", "value2" };
    }

    [HttpPost]
    [Route("testpost")]
    public ActionResult TestPost([FromBody]BasicPostRequest model)
    {
        return Ok("Your ID is: " + model.Id);
    }
}