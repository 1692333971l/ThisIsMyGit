using Protocol;
using UnityEngine;

//装备控制器
public class EquipmentPanelController : MonoBehaviour
{
    [SerializeField] private InventoryPanel _inventoryPanel;//背包面板
    [SerializeField] private EquipmentPanel _equipmentPanel;//装备面板
    [SerializeField] private EquipmentItemOperateMenu _equipmentItemOperateMenu;//装备操作菜单

    private void Awake()
    {
        GameApp.Instance.EquipmentService.OnGetEquipmentResponse += HandleGetEquipmentResponse;
        GameApp.Instance.EquipmentService.OnEquipItemResponse += HandleEquipItemResponse;
        GameApp.Instance.EquipmentService.OnUnequipItemResponse += HandleUnequipItemResponse;

        if (_equipmentItemOperateMenu != null)
        {
            _equipmentItemOperateMenu.OnUnequipClicked += OnClickUnequipButton;
        }
    }

    private void OnDestroy()
    {
        if (GameApp.Instance != null)
        {
            GameApp.Instance.EquipmentService.OnGetEquipmentResponse -= HandleGetEquipmentResponse;
            GameApp.Instance.EquipmentService.OnEquipItemResponse -= HandleEquipItemResponse;
            GameApp.Instance.EquipmentService.OnUnequipItemResponse -= HandleUnequipItemResponse;
        }

        if (_equipmentItemOperateMenu != null)
        {
            _equipmentItemOperateMenu.OnUnequipClicked -= OnClickUnequipButton;
        }
    }

    private void HandleGetEquipmentResponse(GetEquipmentResponse response)
    {
        if ((ErrorCode)response.ErrorCode != ErrorCode.Success)
        {
            MessageHintWindowManger.Instance.ShowMessage("获取装备失败，" + response.Message);
            return;
        }

        RefreshEquipmentPanel();
    }

    private void HandleEquipItemResponse(EquipItemResponse response)
    {
        if ((ErrorCode)response.ErrorCode != ErrorCode.Success)
        {
            MessageHintWindowManger.Instance.ShowMessage("装备失败，" + response.Message);
            return;
        }

        _inventoryPanel.Init();
        RefreshEquipmentPanel();
        MessageHintWindowManger.Instance.ShowMessage("装备成功");
    }

    private void HandleUnequipItemResponse(UnequipItemResponse response)
    {
        if ((ErrorCode)response.ErrorCode != ErrorCode.Success)
        {
            MessageHintWindowManger.Instance.ShowMessage("卸下失败，" + response.Message);
            return;
        }

        RefreshEquipmentPanel();
        MessageHintWindowManger.Instance.ShowMessage("卸下成功");
    }

    private void OnClickUnequipButton(EquipmentItemInfo equipmentItemInfo)
    {
        if (equipmentItemInfo == null || equipmentItemInfo.ItemId <= 0)
        {
            return;
        }

        GameApp.Instance.EquipmentService.SendUnequipItemRequest(equipmentItemInfo.EquipSlotType);
    }

    private void RefreshEquipmentPanel()
    {
        if (_equipmentPanel == null)
        {
            return;
        }

        _equipmentPanel.Init();
    }
}