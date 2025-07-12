using Plugins;
using UnityEngine;

/// <summary>
/// 로딩하고 ClientStatic에 저장하는 로딩 모노싱글톤
///
/// 
/// </summary>
public class UserInformationLoader : MonoSingleton<UserInformationLoader>
{
    protected override void Initialize()
    { }

    /// <summary>
    /// 로딩하고 ClientStatic에 저장함
    /// </summary>
    public void LoadStaticInformation()
    {
        
    }
}
