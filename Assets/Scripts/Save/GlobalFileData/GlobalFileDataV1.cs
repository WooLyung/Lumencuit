using System.Collections.Generic;
using System.Xml.Serialization;

namespace Lumencuit
{
    /// <summary>
    /// 글로벌 데이터 v1
    /// </summary>
    public class GlobalFileDataV1
    {
        [XmlArrayItem("Id")]
        public List<string> ClearedStageIds = new();
    }
}