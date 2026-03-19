using UnityEngine.SceneManagement;

//场景切换管理器
public class SceneLoaderManager
{
    private const string LoginAndRegistrationScene = "LoginAndRegistrationScene";
    private const string CharacterSelectionAndCreationScene = "CharacterSelectionAndCreationScene"; 

    public void LoadLoginAndRegistrationScene()
    {
        SceneManager.LoadScene(LoginAndRegistrationScene);
    }

    public void LoadCharacterSelectionAndCreationScene()
    {
        SceneManager.LoadScene(CharacterSelectionAndCreationScene);
    }
}
