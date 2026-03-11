using System.Collections.Generic;

//模拟NPC服务，提供NPC数据的查询功能
public class MockNPCService : INPCService
{
    private readonly Dictionary<int, NPCData> _npcDataDict = new Dictionary<int, NPCData>();
    public MockNPCService()
    {
        _npcDataDict[1] = new NPCData
        {
            NpcId = 1,
            Name = "村长",
            InteractHint = "按F键与村长交谈",
            DialogContent = "欢迎来到你的到来，冒险者！我是这块的村长"
        };
        _npcDataDict[2] = new NPCData
        {
            NpcId = 2,
            Name = "商人",
            InteractHint = "按F键与商人交谈",
            DialogContent = "欢迎来到我的商店，冒险者！你想要买点什么"
        };
    }
    public NPCDataResponse GetNpc(int NPCId)
    {
        if (!_npcDataDict.ContainsKey(NPCId))
        {
            _npcDataDict[NPCId] = new NPCData();
        }

        return new NPCDataResponse
        {
            Success = true,
            Message = "获取角色列表成功",
            Data = _npcDataDict[NPCId],
        };
    }
}
