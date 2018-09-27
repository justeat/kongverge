using Newtonsoft.Json;

namespace Kongverge.Tests
{
    public static class GenericExtensions
    {
        public static T Clone<T>(this T instance) =>
            (T)JsonConvert.DeserializeObject(JsonConvert.SerializeObject(instance), instance.GetType());
    }
}
