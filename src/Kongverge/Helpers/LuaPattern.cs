using System.Linq;
using Neo.IronLua;

namespace Kongverge.Helpers
{
    public static class LuaPattern
    {
        public static bool IsMatch(string input, string pattern)
        {
            pattern = pattern?.Replace("%_", "_");
            return input.match(pattern).Values.Any();
        }
    }
}
