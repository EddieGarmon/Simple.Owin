using System.Security.Principal;

namespace Demo.Components
{
    internal class UserIdentity : IIdentity
    {
        public UserIdentity(string name = null) {
            IsAuthenticated = name != null;
            Name = name ?? "Anonymous";
        }

        public string AuthenticationType {
            get { return "Self"; }
        }

        public bool IsAuthenticated { get; private set; }

        public string Name { get; private set; }
    }
}