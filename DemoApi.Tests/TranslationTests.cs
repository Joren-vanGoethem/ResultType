using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using DemoApi.Domain;
using JV.ResultUtilities.ValidationMessage;

namespace DemoApi.Tests;

public class TranslationTests
{
  private const string
    ResxFolderPath =
      @"../../../../DemoApi/Translations";

  [Fact]
  public void AllResxFilesShouldHaveSameKeys()
  {
    var resxFiles = Directory.GetFiles(ResxFolderPath, "*.resx");
    var resxKeySets = new Dictionary<string, HashSet<string>>();

    foreach (var file in resxFiles)
    {
      var keys = GetResourceKeys(file);
      resxKeySets[Path.GetFileName(file)] = keys;
    }

    var allKeys = resxKeySets.Values.SelectMany(k => k).ToHashSet();

    var missingKeysReport = new List<string>();

    foreach (var (fileName, keys) in resxKeySets)
    {
      var missingKeys = allKeys.Except(keys).ToList();
      if (missingKeys.Any())
      {
        missingKeysReport.Add(
          $"Language {fileName.Split('.')[1].ToUpper()} is missing keys: {string.Join(", ", missingKeys)}");
      }
    }

    Assert.True(missingKeysReport.Count == 0, string.Join("\n", missingKeysReport));
  }

  [Fact]
  public void AllResxFilesShouldContainAllValidationKeys()
  {
    var resxFiles = Directory.GetFiles(ResxFolderPath, "*.resx");
    var definedKeys = GetValidationKeysFromClass();
    var missingKeysReport = new List<string>();

    foreach (var file in resxFiles)
    {
      var keysInFile = GetResourceKeys(file);
      var missingKeys = definedKeys.Except(keysInFile).ToList();

      if (missingKeys.Any())
      {
        missingKeysReport.Add(
          $"Language {Path.GetFileName(file).Split('.')[1].ToUpper()} is missing keys: {string.Join(", ", missingKeys)}");
      }
    }

    Assert.True(missingKeysReport.Count == 0, string.Join("\n", missingKeysReport));
  }

  [Fact]
  public void ShouldNotHaveExtraKeysInTranslationFiles()
  {
    var allValidationKeys = GetValidationKeysFromClass();

    var resxFiles = Directory.GetFiles(ResxFolderPath, "*.resx");

    var extraKeysReport = new List<string>();

    foreach (var file in resxFiles)
    {
      var languageCode = Path.GetFileNameWithoutExtension(file).Split('.').Last();
      var keysInFile = GetResourceKeys(file);
      var extraKeys = keysInFile.Except(allValidationKeys).ToList();

      if (extraKeys.Any())
      {
        extraKeysReport.Add(
          $"The following keys are present in the {languageCode}.resx file but not in ValidationKeys: {string.Join(", ", extraKeys)}");
      }
    }

    Assert.True(extraKeysReport.Count == 0, string.Join("\n", extraKeysReport));
  }

  [Fact]
  public void TranslationParameterCountShouldMatchValidationKeyDefinitions()
  {
    var resxFiles = Directory.GetFiles(ResxFolderPath, "*.resx");
    var definitions = GetValidationKeyDefinitions(typeof(ValidationKeys))
      .ToDictionary(d => d.Key, d => d.Parameters.Count);

    var mismatchReport = new List<string>();

    foreach (var file in resxFiles)
    {
      var languageCode = Path.GetFileNameWithoutExtension(file).Split('.').Last();
      var entries = GetResourceEntries(file);

      foreach (var (key, value) in entries)
      {
        if (!definitions.TryGetValue(key, out var expectedCount))
          continue;

        var placeholders = Regex.Matches(value, @"\{(\d+)\}");
        var distinctIndices = placeholders.Select(m => int.Parse(m.Groups[1].Value)).Distinct().Count();

        if (distinctIndices != expectedCount)
        {
          mismatchReport.Add(
            $"[{languageCode}] Key '{key}': expected {expectedCount} parameter(s) but translation has {distinctIndices} placeholder(s)");
        }
      }
    }

    Assert.True(mismatchReport.Count == 0, string.Join("\n", mismatchReport));
  }

  private HashSet<string> GetValidationKeysFromClass()
  {
    return GetValidationKeyDefinitions(typeof(ValidationKeys))
      .Select(def => def.Key)
      .ToHashSet();
  }

  private static IEnumerable<ValidationKeyDefinition> GetValidationKeyDefinitions(Type type)
  {
    // Get fields from the current type
    foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.Static))
    {
      if (field.FieldType == typeof(ValidationKeyDefinition) && field.GetValue(null) is ValidationKeyDefinition def)
        yield return def;
    }

    // Recurse into nested classes
    foreach (var nested in type.GetNestedTypes(BindingFlags.Public | BindingFlags.NonPublic))
    {
      foreach (var def in GetValidationKeyDefinitions(nested))
        yield return def;
    }
  }

  private Dictionary<string, string> GetResourceEntries(string resxFilePath)
  {
    var xdoc = XDocument.Load(resxFilePath);
    return xdoc.Descendants("data")
      .Where(d => d.Attribute("name") != null)
      .ToDictionary(
        d => d.Attribute("name")!.Value,
        d => d.Element("value")?.Value ?? "");
  }

  private HashSet<string> GetResourceKeys(string resxFilePath)
  {
    var keys = new HashSet<string>();
    var xdoc = XDocument.Load(resxFilePath);

    foreach (var dataElement in xdoc.Descendants("data"))
    {
      var nameAttribute = dataElement.Attribute("name");
      if (nameAttribute != null && !string.IsNullOrWhiteSpace(nameAttribute.Value))
      {
        keys.Add(nameAttribute.Value);
      }
    }

    return keys;
  }
}
