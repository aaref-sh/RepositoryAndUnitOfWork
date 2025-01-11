using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Newtonsoft.Json;

namespace Core.LocalizedProberty;
#pragma warning disable CS8603 // Possible null reference return.
#pragma warning disable CS8604 // Possible null reference argument.
public class LocalizedPropComparer(string lang) : IComparer<LocalizedProperty>
{
    public int Compare(LocalizedProperty? x, LocalizedProperty? y) => string.Compare(x?.GetByLocale(lang), y?.GetByLocale(lang));
}

public class LocalizedPropertyConverter : ValueConverter<LocalizedProperty, string>
{
    public LocalizedPropertyConverter() : base(
        v => JsonConvert.SerializeObject(v),
        v => JsonConvert.DeserializeObject<LocalizedProperty>(v))
    {
    }
}
public class LocalizedPropertyComparer : ValueComparer<LocalizedProperty>
{
    public LocalizedPropertyComparer() : base(
        (c1, c2) => DictionaryEquals(c1, c2),
        c => GetDictionaryHashCode(c),
        c => CloneDictionary(c))
    {
    }

    private static bool DictionaryEquals(LocalizedProperty d1, LocalizedProperty d2)
    {
        if (d1 == null && d2 == null) return true;
        if (d1 == null || d2 == null) return false;
        if (d1.Count != d2.Count) return false;

        foreach (var pair in d1)
        {
            if (!d2.TryGetValue(pair.Key, out var value) || !pair.Value.Equals(value))
                return false;
        }
        return true;
    }

    private static int GetDictionaryHashCode(LocalizedProperty dictionary)
    {
        int hash = 0;
        foreach (var pair in dictionary)
        {
            hash ^= pair.Key.GetHashCode();
            hash ^= pair.Value.GetHashCode();
        }
        return hash;
    }

    private static LocalizedProperty CloneDictionary(LocalizedProperty dictionary)
    {
        return new(dictionary);
    }
}
#pragma warning restore CS8603 // Possible null reference return.
#pragma warning restore CS8604 // Possible null reference argument.
