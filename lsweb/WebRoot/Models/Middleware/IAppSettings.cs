namespace LightStreamWeb.Models.Middleware
{
    public interface IAppSettings
    {
        string CdnBaseUrl { get; set; }
        int CustomerDataVerificationClientTimeoutMilliseconds { get; set; }
        Pagedefault PageDefault { get; set; }        
    }
}