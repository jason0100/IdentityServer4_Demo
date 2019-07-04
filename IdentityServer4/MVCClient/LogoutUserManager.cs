using System.Collections.Generic;
using System.Linq;

namespace MVCClient {
    public class LogoutUserManager {
        // yes - that needs to be thread-safe, distributed etc (it's a sample)
        List<User> _User = new List<User>();

        public void Add(string sub, string sid) {
            _User.Add(new User { Sub = sub, Sid = sid });
        }

        public bool IsLoggedOut(string sub, string sid) {
            var matches = _User.Any(s => s.IsMatch(sub, sid));
            return matches;
        }

        private class User {
            public string Sub { get; set; }
            public string Sid { get; set; }

            public bool IsMatch(string sub, string sid) {
                return (Sid == sid && Sub == sub) ||
                       (Sid == sid && Sub == null) ||
                       (Sid == null && Sub == sub);
            }
        }
    }
}