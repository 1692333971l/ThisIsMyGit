using UnityEngine.SceneManagement;

//场景切换管理器
public class SceneLoaderManager
{
    private const string LoginAndRegistrationScene = "LoginAndRegistrationScene";//登陆注册场景
    private const string CharacterSelectionAndCreationScene = "CharacterSelectionAndCreationScene";//角色选择创建场景 
    private const string MainCity = "MainCity";//主城

    public void LoadLoginAndRegistrationScene()
    {
        SceneManager.LoadScene(LoginAndRegistrationScene);
    }
    public void LoadCharacterSelectionAndCreationScene()
    {
        SceneManager.LoadScene(CharacterSelectionAndCreationScene);
    }
    public void LoadMainCity()
    {
        SceneManager.LoadScene(MainCity);
    }
}
