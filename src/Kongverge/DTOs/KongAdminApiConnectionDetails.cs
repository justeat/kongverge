using System.Net.Http.Headers;

namespace Kongverge.DTOs
{
    public class KongAdminApiConnectionDetails
    {
        public string Host { get; set; } = "localhost";
        public int Port { get; set; } = 8001;
        public AuthenticationHeaderValue AuthenticationHeader { get; set; }
    }
}