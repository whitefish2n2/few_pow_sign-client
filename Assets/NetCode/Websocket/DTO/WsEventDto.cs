using Codes.OutGame.PickCharacter.Dto;
using NetTest;

namespace NetCode
{
    public struct WsEventDto
    {
        public WsEventType Type; 
        public object Message;

        public static WsEventDto EnqueueMatch(GameMode mode) => new WsEventDto {Type = WsEventType.EnqueueMatch, Message = (mode)};
        public static WsEventDto Ping()=>new WsEventDto {Type = WsEventType.Ping, Message = "Ping"};
        
        public static WsEventDto SelectCharacterTemporary(TryCharacterPickDto dto)=> new WsEventDto{Type = WsEventType.PickCharacterTemporary, Message = dto};
    }
    
}