namespace DNDWithin.Application;

public static class WellKnownGlobalSettings
{
    public const string ACCOUNT_ACTIVATION_EXPIRATION_MINS = "account_activation_expiration_mins";
    
    /// <summary>
    /// 0: Activation Link
    /// 1: Resend Link
    /// 2: Time before expiration in minutes
    /// </summary>
    public const string ACTIVATION_EMAIL_FORMAT = "activation_email_format";
    
    public const string PASSWORD_REQUEST_DURATION_MINS = "password_request_duration_mins";
    public const string EMAIL_SEND_ATTEMPTS_MAX = "email_send_attempts_max";
    public const string EMAIL_SEND_BATCH_LIMIT = "email_send_batch_limit";
    public const string JWT_TOKEN_SECRET = "jwt_token_secret";
    public const string SERVICE_ACCOUNT_USERNAME = "service_account_username";
    
    /// <summary>
    /// 0: Reset code to enter
    /// </summary>
    public const string PASSWORD_RESET_EMAIL_FORMAT = "password_reset_email_format";
    
    public const string PASSWORD_RESET_REQUEST_EXPIRATION_MINS = "password_reset_request_expiration_mins";
}