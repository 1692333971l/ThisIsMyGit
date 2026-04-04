using UnityEngine;

//地图传送门触发器
public class MapPortalTrigger : MonoBehaviour
{
    [SerializeField] private int _targetPortalId;//目标传送点ID

    private bool _isSending = false;

    private void OnTriggerEnter(Collider other)
    {
        if (_isSending)
        {
            return;
        }

        if (!other.CompareTag("Player"))
        {
            return;
        }

        _isSending = true;
        GameApp.Instance.WorldService.SendTeleportRequest(_targetPortalId);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        _isSending = false;
    }
}