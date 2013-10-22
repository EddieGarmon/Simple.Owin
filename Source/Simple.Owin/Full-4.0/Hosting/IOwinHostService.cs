namespace Simple.Owin.Hosting
{
    public interface IOwinHostService
    {
        void Configure(OwinHostContext context);
    }
}