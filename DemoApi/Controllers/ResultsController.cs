using Microsoft.AspNetCore.Mvc;

namespace DemoApi.Controllers;

[ApiController]
[Route("[controller]")]
public class ResultsController : ControllerBase
{
    [HttpPost]
    [EndpointName("CreateUser")]
    public async Task<ActionResult> CreateUser([FromBody] CreateUserRequest request, CancellationToken cancellationToken)
    {
        // Request is automatically validated by CreateUserRequestValidator before reaching this method.
        // If validation fails, the ValidateModelFilter throws a ResultException which is caught by
        // ResultExceptionHandler and returned as a translated ProblemDetails response.

        // save user with dbcontext etc.

        return Ok();
    }
}

public record CreateUserRequest(string Name, string Email);