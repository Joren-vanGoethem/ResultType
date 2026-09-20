# JV.ResultUtilities.AspNetCore

The ASP.NET Core bridge for [JV.ResultUtilities](https://www.nuget.org/packages/JV.ResultUtilities): one
exception handler that turns a thrown `ResultException` into an RFC 9457 problem body, with the HTTP
status coming from the failing key, structured `errors` / `validation` extensions for clients, and
request validation through `AbstractValidator<T>`.

```csharp
// Program.cs
builder.Services.AddControllers();
builder.Services.AddResultProblemDetails<ResxResultMessageTranslator>(options =>
{
    options.MapStatus(ValidationKeys.Legacy.Locked, 423);   // for keys you do not own
});
builder.Services.AddResultValidation(typeof(Program).Assembly);

var app = builder.Build();
app.UseExceptionHandler();
app.MapControllers();
```

```csharp
// Domain: the key says what it is over HTTP
public static readonly ValidationKeyDefinition NotFound =
    ValidationKeyDefinition.Create("User.NotFound").WithGuidParameter("id")
        .WithHttpStatusCode(HttpStatusCode.NotFound);   // core helper; WithHttpStatus(404) is the int form

// Controller: no HasError ladder
var (result, user) = await service.GetAsync(id, ct);
result.ThrowIfFailure();            // → 404 application/problem+json
return Ok(UserResponse.Map(user));
```

The body:

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.5",
  "title": "Not Found",
  "status": 404,
  "detail": "User 3f2c… was not found.",
  "instance": "GET /api/users/3f2c…",
  "traceId": "…", "requestId": "…",
  "errors":     { "": ["User 3f2c… was not found."] },
  "validation": [{ "key": "User.NotFound", "field": null, "message": "…", "parameters": { "id": "3f2c…" } }]
}
```

- `errors` uses ASP.NET Core's own `ValidationProblemDetails` shape (field → messages), so a client's
  existing model-binding error path handles domain failures too.
- `validation` is lossless: stable key to branch on, field, translated message, named raw parameters.
- The handler answers **only** `ResultException`. Anything else returns `false` and the framework's
  default handling logs it and writes a 500 with no exception message — so this package never leaks
  one.
- Status per message: `WithHttpStatusCode` (or the int `WithHttpStatus`) on the key, else `options.MapStatus`, else `DefaultStatusCode`
  (400). Several messages: all equal → that status; otherwise any 5xx → the highest, else the default.
- Validation failures are logged at `Information` (configurable). They are client errors.

`IResultMessageTranslator` is the one seam you implement: `string Translate(ValidationMessage)`. The
fallback returns the key. `ValidateModelFilter` runs every registered `IValidator` against matching
action arguments before the action and throws the same `ResultException`, so request-shape and
business-rule failures share one wire shape.
