using System.Collections.Generic;

namespace Smoker
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "can't be read-only")]
    public class Request
    {
        public int SortOrder { get; set; } = 100;
        public int? Index { get; set; }
        public string Verb { get; set; } = "GET";
        public string Url { get; set; }
        public string Body { get; set; } = null;

        public List<Header> Headers { get; set; } = null;
        public Validation Validation { get; set; } = new Validation();
    }

    public class Header
    {
        public string Key { get; set; }
        public string Value { get; set; }
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "can't be read-only")]
    public class Validation
    {
        public int Code { get; set; } = 200;
        public string ContentType { get; set; } = "application/json";
        public int? MinLength { get; set; }
        public int? MaxLength { get; set; }
        public int? MaxMilliseconds { get; set; }
        public List<Contain> Contains { get; set; } = new List<Contain>();
        public JsonArray JsonArray { get; set; }
        public List<JsonProperty> JsonObject { get; set; }
    }

    public class Contain
    {
        public string Value { get; set; }
        public bool IsCaseSensitive { get; set; } = true;
    }

    public class JsonProperty
    {
        public string Field { get; set; }
        public object Value { get; set; }
    }

    public class JsonArray
    {
        public int MinCount { get; set; }
        public int MaxCount { get; set; }
        public int Count { get; set; }
        public bool CountIsZero { get; set; }
    }
}
