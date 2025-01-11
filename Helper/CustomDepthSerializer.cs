using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Helper;

public class CustomJsonTextWriter(TextWriter textWriter) : JsonTextWriter(textWriter)
{
    public int CurrentDepth { get; private set; }

    public override void WriteStartObject()
    {
        CurrentDepth++;
        base.WriteStartObject();
    }

    public override void WriteEndObject()
    {
        CurrentDepth--;
        base.WriteEndObject();
    }
}
public class CustomContractResolver(Func<bool> includeProperty) : DefaultContractResolver
{
    protected override JsonProperty CreateProperty(
        MemberInfo member, MemberSerialization memberSerialization)
    {
        var property = base.CreateProperty(member, memberSerialization);
        var shouldSerialize = property.ShouldSerialize;
        property.ShouldSerialize = obj => includeProperty() && (shouldSerialize == null || shouldSerialize(obj));
        return property;
    }
}

public class CustomDepthSerializer
{
    public static string SerializeObject(object obj)
    {
        using var strWriter = new StringWriter();
        using (var jsonWriter = new CustomJsonTextWriter(strWriter))
        {
            var resolver = new CustomContractResolver(() => jsonWriter.CurrentDepth <= 1);
            var serializer = new JsonSerializer { ContractResolver = resolver };
            serializer.Serialize(jsonWriter, obj);
        }
        return strWriter.ToString();
    }
}