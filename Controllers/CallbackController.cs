using Microsoft.AspNetCore.Mvc;

namespace Eq.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CallbackController : ControllerBase
{
    [HttpGet]
    public ActionResult GetCallback([FromQuery] string code)
    {
        if (Program.MainForm == null)
            return new EmptyResult();

        Program.MainForm.Code = code;

        return new EmptyResult();
    }
}