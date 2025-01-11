namespace Core.LocalizedProberty;

public class LocalizedProperty : Dictionary<string, string>
{
    public LocalizedProperty(IDictionary<string, string> dictionary) : base(dictionary)
    {
    }

    public LocalizedProperty() : base()
    {
    }
    public string GetByLocale(string? Locale)
    {
        if (string.IsNullOrWhiteSpace(Locale) || Locale.Length > 3)
            Locale = "ar";

        return this.GetValueOrDefault(Locale, "");
    }

    public override string ToString()
    {
        return $"{{{string.Join(", ", this.Select(x => $"{x.Key}:{x.Value}"))}}}";
    }

    public bool Contains(string search, string? lang = null)
    {
        if (lang != null)
        {
            return TryGetValue(lang, out var v) && v.Contains(search, StringComparison.CurrentCultureIgnoreCase);
        }
        foreach (var key in Keys)
        {
            if (this[key].Contains(search, StringComparison.CurrentCultureIgnoreCase)) return true;
        }
        return false;
    }
}

public static class CustomDbFunctions
{
    public static string JsonbGetter(Dictionary<string, string> jsonbValue, string lang)
    {
        return ((LocalizedProperty)jsonbValue).GetByLocale(lang);
    }
}