using System.Collections.Generic;

namespace Smoker
{
    public class Request
    {
        public int SortOrder = 100;
        public int Index = 0;
        public string Verb = "GET";
        public string Url = string.Empty;
        public string ContentType = string.Empty;
        public string Body = string.Empty;

        public List<Header> Headers = new List<Header>();
        public Validation Validation = new Validation();
    }

    public class Header
    {
        public string Key;
        public string Value;
    }

    public class Validation
    {
        public int Code = 200;
        public string ContentType = "application/json";
        public int MinLength = 0;
        public int MaxLength = 0;
        public int MaxMilliseconds = 0;

        public List<Contain> Contains = new List<Contain>();

        public Json Json = new Json();
    }

    public class Contain
    {
        public string Value = string.Empty;
        public bool IsCaseSensitive = false;
    }

    public class Json
    {
        public int MinLength = 0;
        public int MaxLength = 0;
    }
}
