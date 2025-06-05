namespace NetTest
{
    public enum SocketEventType : byte
    {
        //player->server
        Assign = 0, // _R_
        Input = 1,
        Move = 2,

        //server->player
        Setup = 3, // _R_
        Update = 4,
        Hit = 5,   // _R_
        Swap = 6,  // _R_
        Generate = 7, // _R_

        Ping = 252,
        Pong = 253,

        Default = 254
    }

    public static class SocketEventTypeHelper
    {
        public static bool IsReliable(SocketEventType type)
        {
            switch (type)
            {
                case SocketEventType.Assign:
                case SocketEventType.Setup:
                case SocketEventType.Hit:
                case SocketEventType.Swap:
                case SocketEventType.Generate:
                    return true;
                default:
                    return false;
            }
        }
    }
    
        
    
}