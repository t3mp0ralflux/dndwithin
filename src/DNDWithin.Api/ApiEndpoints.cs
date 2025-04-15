namespace DNDWithin.Api;

public static class ApiEndpoints
{
    private const string ApiBase = "api";

    public static class Accounts
    {
        public const string Activate = $"{Base}/activate/{{username}}/{{activationcode}}";
        private const string Base = $"{ApiBase}/accounts";
        public const string Create = Base;
        public const string Delete = $"{Base}/{{id:guid}}";
        public const string Get = $"{Base}/{{id:guid}}";
        public const string GetAll = Base;
        public const string ResendActivation = $"{Base}/activate/{{username}}/{{activationcode}}/resend";
        public const string Update = Base;
    }

    public static class Auth
    {
        private const string Base = "auth";
        public const string Login = $"{Base}/login";
        public const string RequestPasswordReset = $"{Base}/passwordreset/{{email}}";
        public const string PasswordReset = $"{Base}/passwordreset";
    }

    public static class Characters
    {
        private const string Base = $"{ApiBase}/characters";
        public const string Create = Base;
        public const string Delete = $"{Base}/{{id:guid}}";
        public const string Get = $"{Base}/{{id:guid}}";
        public const string GetAll = Base;
        public const string Update = Base;
    }

    public static class GlobalSettings
    {
        private const string Base = $"{ApiBase}/globalsettings";
        public const string Create = Base;
        public const string Get = $"{Base}/{{name}}";
        public const string GetAll = Base;
    }
}