using JV.ResultUtilities.Demo.Infrastructure;
using JV.ResultUtilities.Demo.Orders;
using JV.ResultUtilities.Demo.Products;
using JV.ResultUtilities.Demo.Translations;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Localization
builder.Services.AddLocalization();
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supported = new[] { "en", "nl" };
    options.SetDefaultCulture("en");
    options.AddSupportedCultures(supported);
    options.AddSupportedUICultures(supported);
});

// Database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Exception handling — ResultExceptionHandler catches ResultException from ThrowIfFailure()
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ResultExceptionHandler>();

// Repositories
builder.Services.AddScoped<ProductRepository>();
builder.Services.AddScoped<OrderRepository>();

// Services
builder.Services.AddScoped<ProductService>();
builder.Services.AddScoped<OrderService>();

// Memoization cache — singleton because ConcurrentDictionary is thread-safe
builder.Services.AddSingleton<ProductCatalogCache>();

// Translation
builder.Services.AddScoped<ITranslator, Translator>();

// Controllers
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(
            new System.Text.Json.Serialization.JsonStringEnumConverter());
    });

// Convert model binding errors into the same 422 ValidationProblemDetails format
// that the Result validation pipeline produces
builder.Services.Configure<Microsoft.AspNetCore.Mvc.ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var problemDetails = new Microsoft.AspNetCore.Mvc.ValidationProblemDetails(context.ModelState)
        {
            Status = StatusCodes.Status422UnprocessableEntity,
            Title = "Validation Failed",
            Type = "https://tools.ietf.org/html/rfc9110#section-15.5.21"
        };
        return new Microsoft.AspNetCore.Mvc.UnprocessableEntityObjectResult(problemDetails);
    };
});

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Exception handler must come before routing
app.UseExceptionHandler();

// Request localization — reads Accept-Language header
app.UseRequestLocalization();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

// Ensure database is created (for demo purposes)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}

app.Run();
