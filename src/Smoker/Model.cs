using Newtonsoft.Json;
using System.Collections.Generic;

namespace Smoker
{
    public class Request
    {
        public int SortOrder { get; set; } = 100;
        public bool IsBaseTest { get; set; } = false;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? Index { get; set; }
        public string Verb { get; set; } = "GET";
        public string Url { get; set; } = string.Empty;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string ContentType { get; set; } = null;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Body { get; set; } = null;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<Header> Headers { get; set; } = null;
        public Validation Validation { get; set; } = new Validation();
    }

    public class Header
    {
        public string Key { get; set; }
        public string Value { get; set; }
    }

    public class Validation
    {
        public int Code { get; set; } = 200;
        public string ContentType { get; set; } = "application/json";
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? MinLength { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? MaxLength { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? MaxMilliseconds { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<Contain> Contains { get; set; } = new List<Contain>();

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public JsonArray JsonArray { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<JsonProperty> JsonObject { get; set; }
    }

    public class Contain
    {
        public string Value { get; set; } = string.Empty;
        public bool IsCaseSensitive { get; set; } = true;
    }

    public class JsonProperty
    {
        public string Field { get; set; }
        public object Value { get; set; }
    }

    public class JsonArray
    {
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public int MinCount { get; set; }
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public int MaxCount { get; set; }
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public int Count { get; set; }
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public bool CountIsZero { get; set; }
    }
}
