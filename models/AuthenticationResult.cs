using System;

namespace Oauth.Models
{
    public class AuthenticationResult
    {
        public string AccessToken { get; set; }

        public string TokenType { get; set; }

        public DateTime Expires { get; set; }
    }
}