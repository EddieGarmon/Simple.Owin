namespace Simple.Owin.Hosting
{
    internal interface IOwinHostService
    {
        void Configure(IOwinHostContext context);
    }
}