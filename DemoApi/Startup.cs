using System.Diagnostics;
using DemoApi.Translations;
using Microsoft.AspNetCore.Http.Features;
using Tenant.Api.Translations;

namespace DemoApi;

public class Startup
{
  private IConfiguration Configuration { get; }

  public Startup(IConfiguration configuration)
  {
    Configuration = configuration;
  }
  
  /// <summary>
  /// Configures the services for the API, such as authorization, controllers, cors, mediatr, swagger, localization, etc
  /// </summary>
  /// <param name="services"></param>
  /// <exception cref="InvalidOperationException"></exception>
  public void ConfigureServices(IServiceCollection services)
  {
    services.AddHttpClient();
    services.AddHttpContextAccessor();

    services.AddEndpointsApiExplorer();
    services.AddProblemDetails();
    
    services.AddLocalization();

    services.Configure<RequestLocalizationOptions>(options =>
    {
      var supportedCultures = Culture.SupportedCultures
        .Select(c => c.TwoLetterISOLanguageName)
        .ToArray();

      options.SetDefaultCulture(Culture.Default.TwoLetterISOLanguageName)
        .AddSupportedCultures(supportedCultures)
        .AddSupportedUICultures(supportedCultures);
    });
    
    // Register the translator, just a basic IStringLocalizer wrapper with culture fallback to our default culture
    services.AddSingleton<ITranslator, Translator>();
    
    // custom exception handler for transforming a ResultException into ProblemDetails
    services.AddExceptionHandler<ResultExceptionHandler>();
    
    services.AddControllers();
  }

  /// <summary>
  /// Configures the API, such as adding the swagger, exception middleware, request localization, etc
  /// </summary>
  /// <param name="app"></param>
  /// <param name="environment"></param>
  public async Task ConfigureAsync(WebApplication app, string[] args)
  {
    app.UseCors();
    app.UseHttpsRedirection();
    
    // sets the culture correct per request
    app.UseRequestLocalization();
    
    app.UseExceptionHandler();
    app.UseStatusCodePages();
    app.UseRouting();
    
    app.UseAuthentication();
    app.UseAuthorization();
    
    app.MapControllers();
    
    app.Run();
  }
}
