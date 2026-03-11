using TMPro;
using UnityEngine;

// NPC 世界提示 UI，显示在 NPC 上方，包含 NPC 名称和交互提示
public class NpcWorldHintView : MonoBehaviour
{
    [SerializeField] private GameObject _root;//提示 UI 根对象
    [SerializeField] private TMP_Text _nameText;//NPC 名称文本
    [SerializeField] private TMP_Text _interactHintText;//交互提示文本

    private Camera _mainCamera;

    private void Awake()
    {
        _mainCamera = Camera.main;
    }

    private void LateUpdate()
    {
        if (_root == null || !_root.activeSelf)
        {
            return;
        }
        transform.forward = _mainCamera.transform.forward;
    }

    public void Show(string npcName, string interactHint)
    {
        _root.SetActive(true);
        _nameText.text = npcName;
        _interactHintText.text = interactHint;
    }

    public void Hide()
    {
        _root.SetActive(false);
    }
}