using Codes.Util;

namespace Codes.InGame
{
    public class CharacterSummonPosition : ServerComponent
    {
        public int teamId;

        public override string Serialize()
        {
            return $"TeamId: {teamId}";
        }
    }
}
