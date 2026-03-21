namespace ConfigExporter.Models
{
    public class ProfessionConfig
    {
        public int ProfessionId { get; set; }
        public string ProfessionName { get; set; }
        public string ModelPath { get; set; }

        public int Strength { get; set; }
        public int Agility { get; set; }
        public int Intelligence { get; set; }

        public decimal CritRate { get; set; }
        public decimal CritDamage { get; set; }
        public int Defense { get; set; }

        public int Hp { get; set; }
        public int Mp { get; set; }
        public int MaxHp { get; set; }
        public int MaxMp { get; set; }

        public int MapId { get; set; }
        public float PosX { get; set; }
        public float PosY { get; set; }
        public float PosZ { get; set; }
    }
}