using UnityEngine.SceneManagement;

// 场景加载器，负责管理游戏中的场景切换
public class SceneLoader
{
    private const string LoginSceneName = "Login";
    private const string SelectRoleSceneName = "SelectRole";
    private const string MainCitySceneName = "MainCity";

    public void LoadLoginScene()
    {
        SceneManager.LoadScene(LoginSceneName);//加载登录场景
    }
    public void LoadSelectRoleScene()
    {
        SceneManager.LoadScene(SelectRoleSceneName);//加载角色选择场景
    }
    public void LoadMainCityScene()
    {
        SceneManager.LoadScene(MainCitySceneName);//加载主城场景
    }
}
