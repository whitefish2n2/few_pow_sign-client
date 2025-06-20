using System;
using Newtonsoft.Json;

namespace NetTest
{
    [JsonObject]
    public class ApiResponse<T>
    {
        public string msg;
        public ResponseCode code;
        public T data;
    }
    
    public enum ResponseCode{
        //gpt한테 해달라해버려
    }
}