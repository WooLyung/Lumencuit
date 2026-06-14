using System.Collections.Generic;
using System.Xml.Serialization;

namespace Lumencuit.Save
{
    /// <summary>
    /// 스테이지 데이터 v1
    /// </summary>
    public class StageFileDataV1
    {
        public string StageId;

        [XmlArrayItem("Entity")]
        public List<EntityFileDataV1> Entities = new();
    }
}