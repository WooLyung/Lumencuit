namespace Lumencuit.Save
{
    /// <summary>
    /// 엔티티 데이터 v1
    /// </summary>
    public class EntityFileDataV1
    {
        public int X;
        public int Y;
        public string BlueprintId;
        public int SignalMask;
        public PortFileDataV1 Ports = new();
    }
}