using JV.ResultUtilities.Extensions;
using JV.ResultUtilities.ValidationMessage;

namespace DemoApi.Domain;

public static class ValidationKeys
{
    // the way you structure validationKeys in your codebase is up to you. this is just a simple example
    public static class User
    {
        public static readonly ValidationKeyDefinition NameTooLong = ValidationKeyDefinition.Create("User.NameTooLong")
            .WithStringParameter("Name")
            .WithIntParameter("MaxLength");

        public static readonly ValidationKeyDefinition NameCannotBeEmmpty = ValidationKeyDefinition
            .Create("User.NameCannotBeEmpty");
        
        public static readonly ValidationKeyDefinition EmailInvalid = ValidationKeyDefinition.Create("User.EmailInvalid")
        // do not use email parameter here, because we are expecting an INVALID email,
        // only use email parameter if you want to make sure the parameter passed here is a valid email.
            .WithStringParameter("Email");
    }
}