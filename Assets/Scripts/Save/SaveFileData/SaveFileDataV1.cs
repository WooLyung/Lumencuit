using System.Xml.Serialization;

namespace Lumencuit.Save
{
    /// <summary>
    /// 세이브 데이터 v1
    /// </summary>
    [XmlRoot("Save")]
    public class SaveFileDataV1
    {
        public int Version = 1;
        public GlobalFileDataV1 Global = new();
        public StageFileDataV1 CurrentStage = new();
    }
}