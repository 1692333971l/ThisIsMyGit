using TMPro;
using UnityEngine;
using UnityEngine.UI;

// NPC 对话面板，显示 NPC 名称和对话内容，并提供关闭按钮
public class NpcDialogPanel : MonoBehaviour
{
    [SerializeField] private GameObject _panelRoot;//对话面板根对象
    [SerializeField] private TMP_Text _npcNameText;//NPC 名称文本
    [SerializeField] private TMP_Text _dialogContentText;//对话内容文本
    [SerializeField] private Button _closeButton;

    private void Awake()
    {
        _closeButton.onClick.RemoveAllListeners();
        _closeButton.onClick.AddListener(Hide);
    }

    public void Show(string npcName, string dialogContent)
    {
        _panelRoot.SetActive(true);
        _npcNameText.text = npcName;
        _dialogContentText.text = dialogContent;
    }

    public void Hide()
    {
        _panelRoot.SetActive(false);
    }

    public bool IsShowing()
    {
        return _panelRoot != null && _panelRoot.activeSelf;
    }
}