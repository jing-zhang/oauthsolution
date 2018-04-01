using System;
using Oauth.Wrapper;

namespace oauth2
{
    class Program
    {
        static void Main(string[] args)
        {
            OAuth2Wrapper oauth = new OAuth2Wrapper("http;//www.sample.com",
            "api/user/token", 
            "userId",
            "secretKey", 
            "user_key_");
            Endpoint endpoint = new Endpoint(oauth, 2);
            var obejct = endpoint.CallGet().Result;
        }
    }
}
