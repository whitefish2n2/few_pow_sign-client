using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace NetCode
{
    public class AnotherPlayerInfoDto
    {
        public string id;
        public string name;
        public int publicKey;
        public string characterId;
        public int team;
        public int kill;
        public int death;
        public bool isLockedIn;
        public UserPublicStaticInfo staticInfo;
        public UserDynamicInfo dynamicInfo;
    }

    public class UserPublicStaticInfo
    {
        public string userId;
        public string userName;
        public DateTime createdAt;
    }

    public class UserDynamicInfo
    {
        
    }
}