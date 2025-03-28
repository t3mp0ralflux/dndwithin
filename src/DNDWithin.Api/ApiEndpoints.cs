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
        public const string Update = Base;
        public const string Delete = $"{Base}/{{id:guid}}";
    }

    public static class Auth
    {
        private const string Base = "auth";
        public const string Login = $"{Base}/login";
    }

    public static class GlobalSettings
    {
        private const string Base = $"{ApiBase}/globalsettings";
        public const string Create = Base;
        public const string Get = $"{Base}/{{name}}";
        public const string GetAll = Base;
    }
}