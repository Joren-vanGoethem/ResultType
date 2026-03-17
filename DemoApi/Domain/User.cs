using System.ComponentModel.DataAnnotations;
using JV.ResultUtilities;
using JV.ResultUtilities.ValidationPipeline;

namespace DemoApi.Domain;


public class User
{
    public string Name { get; private set; }    
    public string Email { get; private set; }    

    private User (string name, string email)
    {
        Name = name;
        Email = email;
    }

    public static readonly ValidationPipeline<User> UserValidationPipeline = new ValidationPipeline<User>()
        // magic numbers should ofcourse be replaced with constants, and this one will probably also be used to set the max length on your dbcontext
        .AddRule(u => u.Name.Length <= 100
            ? Result.Ok()
            : Result.Error(ValidationKeys.User.NameTooLong, new
                {
                    Name = u.Name,
                    MaxLength = 100
                })
            )
        .AddRule(u => u.Email.Contains("@") // yes this is a stupid check but you get the idea
            ? Result.Ok()
            : Result.Error(ValidationKeys.User.EmailInvalid, u.Email));

    public static Result<User> Create(string name, string email)
    {
        return UserValidationPipeline.Validate(new User(name, email));
    }
}