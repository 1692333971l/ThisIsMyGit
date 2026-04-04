namespace MMOServer.Config
{
    public class MapPortalConfig
    {
        public int PortalId { get; set; }
        public string PortalName { get; set; }
        public int FromMapId { get; set; }
        public int ToMapId { get; set; }
        public float SpawnX { get; set; }
        public float SpawnY { get; set; }
        public float SpawnZ { get; set; }
    }
}