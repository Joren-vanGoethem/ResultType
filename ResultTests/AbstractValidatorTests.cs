using JV.ResultUtilities;
using JV.ResultUtilities.Extensions;
using JV.ResultUtilities.FluentValidation;
using JV.ResultUtilities.ValidationMessage;

namespace ResultTests;

public class AbstractValidatorTests
{
    private record TestModel(string Name, string Email, int Age);

    private static readonly ValidationKeyDefinition CustomNameRequired =
        ValidationKeyDefinition.Create("Custom.NameRequired");

    private static readonly ValidationKeyDefinition CustomNameTooLong =
        ValidationKeyDefinition.Create("Custom.NameTooLong")
            .WithStringParameter("Name")
            .WithIntParameter("MaxLength");

    private static readonly ValidationKeyDefinition CustomEmailInvalid =
        ValidationKeyDefinition.Create("Custom.EmailInvalid")
            .WithStringParameter("Email");

    #region Basic validation

    private class BasicValidator : AbstractValidator<TestModel>
    {
        public BasicValidator()
        {
            RuleFor(x => x.Name).NotEmpty();
            RuleFor(x => x.Email).NotEmpty();
        }
    }

    [Fact]
    public void Validate_ValidModel_ReturnsSuccess()
    {
        var validator = new BasicValidator();
        var result = validator.Validate(new TestModel("John", "john@example.com", 25));

        Assert.True(result.IsSuccessful);
        Assert.Equal("John", result.Value.Name);
    }

    [Fact]
    public void Validate_InvalidModel_ReturnsAllErrors()
    {
        var validator = new BasicValidator();
        var result = validator.Validate(new TestModel("", "", 25));

        Assert.True(result.IsFailure);
        Assert.Equal(2, result.ValidationMessages.Count());
    }

    [Fact]
    public void Validate_PartiallyInvalid_ReturnsRelevantErrors()
    {
        var validator = new BasicValidator();
        var result = validator.Validate(new TestModel("John", "", 25));

        Assert.True(result.IsFailure);
        Assert.Single(result.ValidationMessages);
    }

    #endregion

    #region Built-in rules

    private class AllRulesValidator : AbstractValidator<TestModel>
    {
        public AllRulesValidator()
        {
            RuleFor(x => x.Name)
                .NotNull()
                .NotEmpty()
                .MinLength(2)
                .MaxLength(10);

            RuleFor(x => x.Age)
                .Must(age => age >= 18);
        }
    }

    [Fact]
    public void NotNull_NullValue_Fails()
    {
        var validator = new AllRulesValidator();
        var result = validator.Validate(new TestModel(null!, "test@test.com", 20));

        Assert.True(result.IsFailure);
        // NotNull fails, NotEmpty fails (null is empty), MinLength passes (null passes), MaxLength passes (null passes)
        var messages = result.ValidationMessages.ToList();
        Assert.True(messages.Count >= 2);
    }

    [Fact]
    public void NotEmpty_WhitespaceString_Fails()
    {
        var validator = new AllRulesValidator();
        var result = validator.Validate(new TestModel("   ", "test@test.com", 20));

        Assert.True(result.IsFailure);
    }

    [Fact]
    public void MinLength_TooShort_Fails()
    {
        var validator = new AllRulesValidator();
        var result = validator.Validate(new TestModel("A", "test@test.com", 20));

        Assert.True(result.IsFailure);
        Assert.Contains(result.ValidationMessages, m => m.TranslationKey == "Validation.MinLength");
    }

    [Fact]
    public void MaxLength_TooLong_Fails()
    {
        var validator = new AllRulesValidator();
        var result = validator.Validate(new TestModel("VeryLongNameExceeding", "test@test.com", 20));

        Assert.True(result.IsFailure);
        Assert.Contains(result.ValidationMessages, m => m.TranslationKey == "Validation.MaxLength");
    }

    [Fact]
    public void Must_PredicateFails_ReturnsError()
    {
        var validator = new AllRulesValidator();
        var result = validator.Validate(new TestModel("John", "test@test.com", 15));

        Assert.True(result.IsFailure);
        Assert.Contains(result.ValidationMessages, m => m.TranslationKey == "Validation.Predicate");
    }

    [Fact]
    public void AllRules_ValidModel_Succeeds()
    {
        var validator = new AllRulesValidator();
        var result = validator.Validate(new TestModel("John", "test@test.com", 20));

        Assert.True(result.IsSuccessful);
    }

    #endregion

    #region WithMessage override

    private class WithMessageValidator : AbstractValidator<TestModel>
    {
        public WithMessageValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                    .WithMessage(CustomNameRequired)
                .MaxLength(10)
                    .WithMessage(CustomNameTooLong, u => new { Name = u.Name, MaxLength = 10 });

            RuleFor(x => x.Email)
                .Must(email => email.Contains("@"))
                    .WithMessage(CustomEmailInvalid, u => u.Email);
        }
    }

    [Fact]
    public void WithMessage_OverridesKey_ForParameterlessKey()
    {
        var validator = new WithMessageValidator();
        var result = validator.Validate(new TestModel("", "test@test.com", 20));

        Assert.True(result.IsFailure);
        Assert.Contains(result.ValidationMessages, m => m.TranslationKey == "Custom.NameRequired");
    }

    [Fact]
    public void WithMessage_OverridesKeyAndParams_ForParameterizedKey()
    {
        var validator = new WithMessageValidator();
        var result = validator.Validate(new TestModel("VeryLongName", "test@test.com", 20));

        Assert.True(result.IsFailure);
        var msg = result.ValidationMessages.First(m => m.TranslationKey == "Custom.NameTooLong");
        Assert.Equal("VeryLongName", msg.Parameters[0]);
        Assert.Equal("10", msg.Parameters[1]);
    }

    [Fact]
    public void WithMessage_WithParamsFactory_PassesCorrectParams()
    {
        var validator = new WithMessageValidator();
        var result = validator.Validate(new TestModel("John", "bademail", 20));

        Assert.True(result.IsFailure);
        var msg = result.ValidationMessages.First(m => m.TranslationKey == "Custom.EmailInvalid");
        Assert.Equal("bademail", msg.Parameters[0]);
    }

    #endregion

    #region FieldName extraction

    [Fact]
    public void RuleFor_ExtractsFieldName_OnValidationMessage()
    {
        var validator = new BasicValidator();
        var result = validator.Validate(new TestModel("", "test@test.com", 20));

        var msg = result.ValidationMessages.First();
        Assert.Equal("Name", msg.KeyDefinition.FieldName);
    }

    [Fact]
    public void RuleFor_WithMessage_PreservesFieldName()
    {
        var validator = new WithMessageValidator();
        var result = validator.Validate(new TestModel("", "test@test.com", 20));

        var msg = result.ValidationMessages.First();
        Assert.Equal("Name", msg.KeyDefinition.FieldName);
    }

    [Fact]
    public void RuleFor_ValueType_ExtractsFieldName()
    {
        var validator = new AllRulesValidator();
        var result = validator.Validate(new TestModel("John", "test@test.com", 15));

        var msg = result.ValidationMessages.First(m => m.TranslationKey == "Validation.Predicate");
        Assert.Equal("Age", msg.KeyDefinition.FieldName);
    }

    #endregion

    #region ShortCircuit

    private class ShortCircuitValidator : AbstractValidator<TestModel>
    {
        public ShortCircuitValidator()
        {
            ShortCircuit = true;

            RuleFor(x => x.Name).NotEmpty();
            RuleFor(x => x.Email).NotEmpty();
        }
    }

    [Fact]
    public void ShortCircuit_StopsAtFirstFailure()
    {
        var validator = new ShortCircuitValidator();
        var result = validator.Validate(new TestModel("", "", 20));

        Assert.True(result.IsFailure);
        Assert.Single(result.ValidationMessages);
    }

    #endregion

    #region Async rules

    private class AsyncValidator : AbstractValidator<TestModel>
    {
        public AsyncValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .MustAsync(async name =>
                {
                    await Task.Delay(1);
                    return name.Length <= 50;
                });
        }
    }

    [Fact]
    public async Task ValidateAsync_WithAsyncRules_Works()
    {
        var validator = new AsyncValidator();
        var result = await validator.ValidateAsync(new TestModel("John", "test@test.com", 20));

        Assert.True(result.IsSuccessful);
    }

    [Fact]
    public async Task ValidateAsync_AsyncRuleFails_ReturnsError()
    {
        var validator = new AsyncValidator();
        var longName = new string('A', 51);
        var result = await validator.ValidateAsync(new TestModel(longName, "test@test.com", 20));

        Assert.True(result.IsFailure);
    }

    [Fact]
    public void Validate_WithAsyncRules_ThrowsInvalidOperationException()
    {
        var validator = new AsyncValidator();

        Assert.Throws<InvalidOperationException>(() =>
            validator.Validate(new TestModel("John", "test@test.com", 20)));
    }

    #endregion

    #region IValidator interface

    [Fact]
    public void IValidator_ValidatedType_ReturnsCorrectType()
    {
        IValidator validator = new BasicValidator();
        Assert.Equal(typeof(TestModel), validator.ValidatedType);
    }

    [Fact]
    public void IValidator_ValidateObject_Works()
    {
        IValidator validator = new BasicValidator();
        var result = validator.ValidateObject(new TestModel("John", "test@test.com", 20));

        Assert.True(result.IsSuccessful);
    }

    [Fact]
    public async Task IValidator_ValidateObjectAsync_Works()
    {
        IValidator validator = new BasicValidator();
        var result = await validator.ValidateObjectAsync(new TestModel("John", "test@test.com", 20));

        Assert.True(result.IsSuccessful);
    }

    #endregion

    #region Default key parameters

    [Fact]
    public void NotEmpty_DefaultKey_IncludesFieldNameParameter()
    {
        var validator = new BasicValidator();
        var result = validator.Validate(new TestModel("", "test@test.com", 20));

        var msg = result.ValidationMessages.First();
        Assert.Equal("Validation.NotEmpty", msg.TranslationKey);
        Assert.Equal("Name", msg.Parameters[0]);
    }

    [Fact]
    public void MaxLength_DefaultKey_IncludesFieldNameAndMaxParameters()
    {
        var validator = new AllRulesValidator();
        var result = validator.Validate(new TestModel("VeryLongNameExceeding", "test@test.com", 20));

        var msg = result.ValidationMessages.First(m => m.TranslationKey == "Validation.MaxLength");
        Assert.Equal("Name", msg.Parameters[0]);
        Assert.Equal("10", msg.Parameters[1]);
    }

    #endregion

    #region Expression edge cases

    [Fact]
    public void RuleFor_NestedProperty_ExtractsLeafName()
    {
        // x.Name.Length is a valid MemberExpression — extracts "Length"
        var validator = new NestedPropertyValidator();
        var result = validator.Validate(new TestModel("Hi", "test@test.com", 20));

        Assert.True(result.IsFailure);
        var msg = result.ValidationMessages.First();
        Assert.Equal("Length", msg.KeyDefinition.FieldName);
    }

    private class NestedPropertyValidator : AbstractValidator<TestModel>
    {
        public NestedPropertyValidator()
        {
            RuleFor(x => x.Name.Length).Must(len => len > 5);
        }
    }

    [Fact]
    public void RuleFor_MethodCallExpression_Throws()
    {
        Assert.Throws<ArgumentException>(() =>
        {
            new MethodCallExpressionValidator();
        });
    }

    private class MethodCallExpressionValidator : AbstractValidator<TestModel>
    {
        public MethodCallExpressionValidator()
        {
            RuleFor(x => x.Name.ToUpper());
        }
    }

    #endregion
}
