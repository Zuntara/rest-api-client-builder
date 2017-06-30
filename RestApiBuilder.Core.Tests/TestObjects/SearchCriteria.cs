namespace RestApiClientBuilder.Core.Tests.TestObjects
{
    public class SearchCriteria
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public CriteriaDef SubObject { get; set; }
    }

    public class CriteriaDef
    {
        public string Value { get; set; }
        public string Condition { get; set; }
    }
}