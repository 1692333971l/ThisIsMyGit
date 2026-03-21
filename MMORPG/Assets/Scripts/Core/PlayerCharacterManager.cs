using Protocol;

//玩家当前控制角色管理器
public class PlayerCharacterManager
{
    private CharacterInfo _selectedCharacter;//当前玩家信息

    //获取当前玩家信息
    public CharacterInfo GetCharacterInfo()
    {
        return _selectedCharacter; 
    }
    //设置当前玩家信息
    public void SetCharacterInfo(CharacterInfo characterInfo)
    {
        _selectedCharacter = characterInfo;
    }
}
