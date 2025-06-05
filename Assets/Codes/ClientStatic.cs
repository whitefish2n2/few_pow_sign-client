using System;
using Codes.Util.Annotation;

namespace Codes
{
    public class ClientStatic
    {
        public static ClientStatic Instance{get;private set;} = new();
        
        public static NetTestStatic instance;
        public string baseUrl;
    
        public string authId;
        public string authPassword;
        public string authName;
    
        public int serverPort;
        [ReadOnly] public string jwt;
        [ReadOnly] public string refreshToken;
        [ReadOnly] public string username;
        [ReadOnly] public UInt64 userPrivateKey;
        [ReadOnly] public sbyte userPublicKey;
        [ReadOnly] public string sessionConnectToken;
        [ReadOnly] public string sessionKey;

        public string dedicatedBaseUrl;
        public string dedicatedPort;

    }
}
