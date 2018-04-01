using System;

namespace Oauth.Models
{
    public class DesObject
    {
        public Guid GlobalId { get; set; }

        public string UserName { get; set; }

        public DateTime Expires { get; set; }
    }
}