using DemoApi.Domain;
using JV.ResultUtilities;
using JV.ResultUtilities.Extensions;
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

    [HttpGet("{id:guid}")]
    [EndpointName("GetUser")]
    public ActionResult GetUser(Guid id)
    {
        // The demo has no store, so every lookup fails. The key carries WithHttpStatus(404), so
        // ThrowIfFailure() alone produces a 404 problem body — no HasError(...) → NotFound() ladder.
        var result = Result.Error(ValidationKeys.User.NotFound, id);
        result.ThrowIfFailure();

        return Ok();
    }
}

public record CreateUserRequest(string Name, string Email);