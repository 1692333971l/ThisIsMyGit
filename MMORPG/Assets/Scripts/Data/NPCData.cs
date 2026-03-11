//NPC数据结构
public class NPCData
{
    private int npcId;
    private string name;
    private string interactHint;
    private string dialogContent;

    public int NpcId { get => npcId; set => npcId = value; }
    public string Name { get => name; set => name = value; }
    public string InteractHint { get => interactHint; set => interactHint = value; }
    public string DialogContent { get => dialogContent; set => dialogContent = value; }
}
