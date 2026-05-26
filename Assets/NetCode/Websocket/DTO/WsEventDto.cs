using Codes.OutGame.PickCharacter.Dto;
using NetTest;

namespace NetCode
{
    public struct WsEventDto
    {
        public WsEventType Type;
        public object Message;

        public static WsEventDto EnqueueMatch(GameMode mode) =>
            new WsEventDto { Type = WsEventType.EnqueueMatch, Message = (mode) };

        public static WsEventDto CancelMatch() => new WsEventDto{Type = WsEventType.Cancel, Message = "" };

    public static WsEventDto Ping()=>new WsEventDto {Type = WsEventType.Ping, Message = "Ping"};
        
        public static WsEventDto SelectCharacterTemporary(TryCharacterPickDto dto)=> new WsEventDto{Type = WsEventType.PickCharacterTemporary, Message = dto};
        public static WsEventDto LockInCharacter(TryCharacterPickDto dto)=> new WsEventDto{Type = WsEventType.PickCharacter, Message = dto};
        
        public static WsEventDto JoinLobby()=> new WsEventDto {Type = WsEventType.JoinLobby, Message = "JoinLobby"};
    }
    
}