using Newtonsoft.Json.Linq;
using Unity.VisualScripting;

namespace NetTest
{
    public class ErrorResponse
    {
        public ErrorResponse(int code, string message) : this()
        {
            this.code = code;
            this.message = message;
        }

        public ErrorResponse(ExceptionCode code, string message) : this()
        {
            this.code = (int)code;
            this.message = message;
        }
        public ErrorResponse(JObject jObject,string indicator = "not defined") : this()
        {
            this.message = jObject["message"]?.ToString()??"Error On" + indicator;
            this.code = int.Parse(jObject["code"]?.ToString()??"-1");
        }
        public int code;
        public string message;

        private ErrorResponse()
        {
            
        }


        public static ErrorResponse NotDefined(string indicator = "not defined")
        {
            return new ErrorResponse(ExceptionCode.NotDefinedFromClient, "예상치 못한 에러가 발생했습니다. whitefish822@gmail.com으로 발생 경위를 적어 연락해주세요. Indicator:"+indicator);
        }

        public static ErrorResponse JsonParseFailed(string indicator)
        {
            return new ErrorResponse(-600, "Json 오브젝트 파싱에 실패하였습니다. whitefish822@gmail.com으로 발생 경위를 적어 연락해주세요. Indicator:"+indicator);
        }

        public static ErrorResponse ServerTimeout = new ErrorResponse(-700, "서버와의 연결 시간이 초과되었습니다. 연결 상태를 확인하여주세요.");

        public static ErrorResponse ServerNotFound = new ErrorResponse(-800, "서버를 찾지 못했습니다.");

        public static ErrorResponse TokenExpired = new ErrorResponse(-900, "로그인 기한이 만료되었습니다. 다시 로그인하여주세요.");
    }

    public enum ExceptionCode {
        //From Client
        NotDefinedFromClient = -500,
        JsonParseFailed = -600,
        ServerTimeout = -700,
        ServerNotFound = -800,
        
        //From Server
        TestException = 1,
        SignInFailedException = 4001,
        AlreadyExistsIdException = 4002,
        PlayerNotFoundException = 4003,
        LoginFailedException = 4004,
        LoginNotRegisteredIdException = 4005,
        LoginPasswordNotMatchedException = 4006,
        //Match Error
        InvalidMatchException = 4010,
        //DedicatedServer Error
        DediSecretKeyNotMatchedException = 4060,
        //Token Error
        InvalidTokenException = 4070,
        InvalidJwtException = 4071,
        
        
        //Not Defined
        NotDefinedError = 4999,
    }
}