using Newtonsoft.Json.Linq;
using Unity.VisualScripting;

namespace NetTest
{
    public class ErrorResponse
    {
        public ErrorResponse(int code, string message)
        {
            this.code = code;
            this.message = message;
        }
        public ErrorResponse(JObject jObject,string indicator = "not defined")
        {
            this.message = jObject["message"]?.ToString()??"Error On" + indicator;
            this.code = int.Parse(jObject["code"]?.ToString()??"-1");
        }
        public int code;
        public string message;

        
        public static ErrorResponse NotDefined(string indicator = "not defined")
        {
            return new ErrorResponse(-500, "예상치 못한 에러가 발생했습니다. whitefish822@gmail.com으로 발생 경위를 적어 연락해주세요. Indicator:"+indicator);
        }

        public static ErrorResponse JsonParseFailed(string indicator)
        {
            return new ErrorResponse(-600, "Json 오브젝트 파싱에 실패하였습니다. whitefish822@gmail.com으로 발생 경위를 적어 연락해주세요. Indicator:"+indicator);
        }
    }
}