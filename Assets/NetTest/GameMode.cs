using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace NetTest
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum GameMode
    {
        DeathMatch = 0,
        OneVsOne = 1,
        Solo = 2,
        Custom = -1,
        FiveVsFive = 3
    }
}