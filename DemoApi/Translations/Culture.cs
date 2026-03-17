using System.Globalization;

namespace Tenant.Api.Translations;

public static class Culture
{
  public static CultureInfo English => new CultureInfo("en");
  public static CultureInfo Dutch => new CultureInfo("nl");

  /// <summary>
  /// The default culture and language string, language string is important for culture behaviour pipeline, needs compile time constant
  /// </summary>
  public static CultureInfo Default => English;
  
  /// <summary>
  /// All the supported cultures
  /// </summary>
  public static IList<CultureInfo> SupportedCultures => new List<CultureInfo>
  {
    English,
    Dutch
  };
}
