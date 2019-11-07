using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Nito.AsyncEx;

namespace Kongverge.DTOs
{
    public sealed class KongCertificate : KongObject, IKongEquatable<KongCertificate>, IValidatableObject
    {
        public static readonly string ObjectName = "certificate";

        protected override string[] ToStringSegments => throw new System.NotImplementedException();

        public bool IsMatch(KongCertificate other)
        {
            throw new System.NotImplementedException();
        }

        public override StringContent ToJsonStringContent()
        {
            throw new System.NotImplementedException();
        }

        public object GetEqualityValues()
        {
            throw new System.NotImplementedException();
        }

        public bool Equals(KongCertificate other)
        {
            throw new System.NotImplementedException();
        }

        public Task Validate(IDictionary<string, AsyncLazy<KongSchema>> schemas, ICollection<string> errorMessages, KongObject parent = null)
        {
            throw new System.NotImplementedException();
        }
    }
}
