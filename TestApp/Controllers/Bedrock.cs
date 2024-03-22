using Microsoft.AspNetCore.Mvc;
using TestServices;

namespace TestApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BedrockController : ControllerBase
{
    [HttpGet]
    [Route("test")]
    public IEnumerable<string> Get()
    {
        return new string[] { "value1", "value2" };
    }

    [HttpPost]
    [Route("testquery")]
    public ActionResult TestQuery([FromBody]BedrockQueryRequest model)
    {
        return Ok("Your response is: " + model.Prompt);
    }

    [HttpPost]
    [Route("query")]
    public async Task<string> RunQuery(BedrockQueryRequest request)
    {
        TestServices.BedrockService service = new TestServices.BedrockService();
        string respons = await service.RunPrompt(request.Prompt);
        return respons;
    }

    [HttpPost]
    [Route("testknowledgebase")]
    public async Task<ActionResult> TestKnowledgeBase(BedrockQueryRequest request)
    {
        BedrockService service = new();
        var response = await service.RunPromptOnData(request.Prompt);
        return Ok(response);
    }

    [HttpPost]
    [Route("queryknowledgebase")]
    public async Task<ActionResult> RunQueryKnowledgeBase(BedrockQueryRequest request)
    {
        BedrockService service = new();
        var response = await service.RunRagPrompt(request.Prompt);
        return Ok(response);
    }
}