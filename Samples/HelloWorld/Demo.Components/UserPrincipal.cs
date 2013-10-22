using System.Security.Principal;

namespace Demo.Components
{
    internal class UserPrincipal : IPrincipal
    {
        private readonly UserIdentity _identity;

        public UserPrincipal(string name = null) {
            _identity = new UserIdentity(name);
        }

        public IIdentity Identity {
            get { return _identity; }
        }

        public bool IsInRole(string role) {
            return true;
        }
    }
}