namespace DNDWithin.Api;

public static class ApiEndpoints
{
    private const string ApiBase = "api";

    public static class Accounts
    {
        private const string Base = $"{ApiBase}/accounts";
        public const string Create = Base;
        public const string Get = $"{Base}/{{id:guid}}";
        public const string GetAll = Base;
    }
}