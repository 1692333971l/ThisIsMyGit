using Protocol;
using UnityEngine;

//背包面板
public class InventoryPanel : MonoBehaviour
{
    [SerializeField] private Transform _inventoryPanelRoot;//背包格子根节点
    [SerializeField] private InventorySlotItem _inventorySlotItem;//背包格子
    [SerializeField] private InventoryItemOperateMenu _inventoryItemOperateMenu;//道具操作菜单

    private int _maxSlotCount = 72;

    private void Awake()
    {
        //清空背包列表
        for (int i = _inventoryPanelRoot.childCount - 1; i >= 0; i--)
        {
            Destroy(_inventoryPanelRoot.GetChild(i).gameObject);
        }
        //生成列表项
        for (int i = 0; i < _maxSlotCount; i++)
        {
            InventorySlotItem item = Instantiate(_inventorySlotItem, _inventoryPanelRoot);
            item.OnClickCallback -= OnClickInventorySlotItem;
            item.OnClickCallback += OnClickInventorySlotItem;
        }
    }
    private void OnEnable()
    {
        Init();
    }
    //初始化数据
    public void Init()
    {
        //刷新数据
        for (int i = 0; i < _maxSlotCount; i++)
        {
            InventoryItemInfo inventoryItemInfo = GameApp.Instance.PlayerInventoryManager.GetPlayerInventoryBySlotIndex(i);
            InventorySlotItem item = _inventoryPanelRoot.GetChild(i).gameObject.GetComponent<InventorySlotItem>();
            
            if (inventoryItemInfo != null)
            {
                item.Init(inventoryItemInfo);
            }
            else
            {
                item.InitEmpty(i);
            }
        }
        OnClickInventorySlotItem(null);
    }
    //格子点击事件
    private void OnClickInventorySlotItem(InventoryItemInfo inventoryItemInfo)
    {
        if (inventoryItemInfo == null)
        {
            _inventoryItemOperateMenu.gameObject.SetActive(false);
            return;
        }
        _inventoryItemOperateMenu.gameObject.SetActive(true);
        _inventoryItemOperateMenu.Init(inventoryItemInfo);
    }
}
