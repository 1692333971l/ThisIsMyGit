//UI管理器
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField] private CharacterPanel _characterPanel;//角色面板（包含属性装备背包等）
    [SerializeField] private DialoguePanel _dialoguePanel;//对话面包
    [SerializeField] private ShopPanel _shopPanel;//商店面板
    [SerializeField] private PlayerInfoPanel _playerInfoPanel;//角色属性面板

    public void PlayerInfoPanelInit(Protocol.CharacterInfo characterInfo)
    {
        _playerInfoPanel.Init(characterInfo);
    }
    public void ShowShopPanel(int shopId)
    {
        _shopPanel.Show(shopId, GameApp.Instance.PlayerCharacterManager.GetCharacterInfo());
    }
    public void ShowDialoguePanel(NpcConfig npcConfig)
    {
        _dialoguePanel.Show(npcConfig);
    }
    public void SetCharacterPanelActive(bool flag)
    {
        _characterPanel.gameObject.SetActive(flag);
    }
    public void SetDialoguePanelActive(bool flag)
    {
        _dialoguePanel.gameObject.SetActive(flag);
    }
    public void SetShopPanelActive(bool flag)
    {
        _shopPanel.gameObject.SetActive(flag);
    }
    public void SetPlayerInfoPanelActive(bool flag)
    {
        _playerInfoPanel.gameObject.SetActive(flag);
    }
    public void CloseAllPanel()
    {
        _characterPanel.gameObject.SetActive(false);
        _dialoguePanel.gameObject.SetActive(false);
        _shopPanel.gameObject.SetActive(false);
        _playerInfoPanel.gameObject.SetActive(true);
    }
}
