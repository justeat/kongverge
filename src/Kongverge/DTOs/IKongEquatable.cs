using System;
using JsonDiffPatchDotNet;
using Kongverge.Helpers;
using Newtonsoft.Json.Linq;

namespace Kongverge.DTOs
{
    public interface IKongEquatable<T> : IKongEquatable, IEquatable<T>
    {
        bool IsMatch(T other);
    }

    public interface IKongEquatable
    {
        object GetEqualityValues();
    }

    internal static class KongObjectExtensions
    {
        internal static bool KongEquals<T>(this IKongEquatable<T> instance, IKongEquatable<T> other) where T : KongObject
        {
            if (other == null)
                return false;

            if (ReferenceEquals(instance, other))
                return true;

            if (other.GetType() != instance.GetType())
                return false;

            var instanceSerialized = instance.GetEqualityValues().ToNormalizedJson();
            var otherSerialized = other.GetEqualityValues().ToNormalizedJson();

            return otherSerialized == instanceSerialized;
        }

        internal static bool KongEqualsObject<T>(this IKongEquatable<T> instance, object obj) where T : KongObject
        {
            if (obj == null)
                return false;

            if (ReferenceEquals(instance, obj))
                return true;

            return obj is IKongEquatable<T> other && instance.KongEquals(other);
        }

        internal static int GetKongHashCode<T>(this IKongEquatable<T> instance) where T : KongObject =>
            instance.GetEqualityValues().ToNormalizedJson().GetHashCode();

        internal static JToken DifferencesFrom(this IKongEquatable instance, IKongEquatable other)
        {
            var instanceToken = instance.ToDifferenceToken();
            var otherToken = other.ToDifferenceToken();

            var options = new Options
            {
                ArrayDiff = ArrayDiffMode.Simple,
                TextDiff = TextDiffMode.Simple
            };
            return new JsonDiffPatch(options).Diff(otherToken, instanceToken);
        }

        private static JToken ToDifferenceToken(this IKongEquatable instance) =>
            JToken.FromObject(instance.GetEqualityValues()).Normalize();
    }
}
