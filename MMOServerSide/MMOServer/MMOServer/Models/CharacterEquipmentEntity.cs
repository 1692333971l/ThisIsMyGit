namespace MMOServer.Models
{
    public class CharacterEquipmentEntity
    {
        public int Id { get; set; }
        public int CharacterId { get; set; }
        public int EquipSlotType { get; set; }
        public int ItemId { get; set; }
    }
}