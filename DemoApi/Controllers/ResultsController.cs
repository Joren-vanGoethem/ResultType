using DemoApi.Domain;
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
        var (result, user) = Domain.User.Create(request.Name, request.Email);
        
        // this will get caught by the resultException handler,
        // problemdetails will be created with translations of the validation keys
        result.ThrowIfFailure(); 
        
        // save user with dbcontext etc.
        
        // if the result was succesful you get a 200 OK
        return Ok();
    }
}

public record CreateUserRequest (string Name, string Email);