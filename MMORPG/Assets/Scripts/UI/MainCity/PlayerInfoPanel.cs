using Protocol;
using TMPro;
using UnityEngine;

//玩家信息面板
public class PlayerInfoPanel : MonoBehaviour
{
    [SerializeField] private RectTransform _maxHp;
    [SerializeField] private RectTransform _hp;
    [SerializeField] private RectTransform _maxMp;
    [SerializeField] private RectTransform _mp;
    [SerializeField] private RectTransform _exp;
    [SerializeField] private TMP_Text _gold;
    [SerializeField] private TMP_Text _lv;

    private void Start()
    {
        Init();
        GameApp.Instance.InventoryService.OnUseItemResponse += HandleUseItemResponse;
    }
    private void OnDestroy()
    {
        GameApp.Instance.InventoryService.OnUseItemResponse -= HandleUseItemResponse;
    }
    private void HandleUseItemResponse(UseItemResponse response)
    {
        Init();
    }
    public void Init()
    {
        Protocol.CharacterInfo characterInfo = GameApp.Instance.PlayerCharacterManager.GetCharacterInfo();
        _maxHp.sizeDelta = new Vector2(characterInfo.MaxHp * 2, _maxHp.sizeDelta.y);
        _hp.sizeDelta = new Vector2(characterInfo.Hp * 2, _hp.sizeDelta.y);
        _maxMp.sizeDelta = new Vector2(characterInfo.MaxMp * 2, _maxMp.sizeDelta.y);
        _mp.sizeDelta = new Vector2(characterInfo.Mp * 2, _mp.sizeDelta.y);
        _exp.sizeDelta = new Vector2(2560/1000 * characterInfo.Exp, _exp.sizeDelta.y);//TODO:实际显示比例根据每个等级最大经验值计算
        _gold.text = $"金币：{characterInfo.Gold}";
        _lv.text = $"Lv.{characterInfo.Level}";
    }
}
