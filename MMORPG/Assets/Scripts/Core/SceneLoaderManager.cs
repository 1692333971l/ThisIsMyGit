using UnityEngine.SceneManagement;

//场景切换管理器
public class SceneLoaderManager
{
    private const string LoginAndRegistrationScene = "LoginAndRegistrationScene";//登陆注册场景
    private const string CharacterSelectionAndCreationScene = "CharacterSelectionAndCreationScene";//角色选择创建场景 
    private const string MainCity = "MainCity";//主城
    private const string Wilderness = "Wilderness";//野外

    public void LoadLoginAndRegistrationScene()
    {
        SceneManager.LoadScene(LoginAndRegistrationScene);
    }
    public void LoadCharacterSelectionAndCreationScene()
    {
        SceneManager.LoadScene(CharacterSelectionAndCreationScene);
    }
    //根据地图ID加载场景
    public void LoadMapSceneByMapId(int mapId)
    {
        switch (mapId)
        {
            case 1:
                SceneManager.LoadScene(MainCity);
                break;
            case 2:
                SceneManager.LoadScene(Wilderness);
                break;
            default:
                MessageHintWindowManger.Instance.ShowMessage($"未知的地图ID: {mapId}");
                break;
        }
    }
}
