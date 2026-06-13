using System.Collections.Generic;
using System.Linq;

namespace Lumencuit
{
    /// <summary>
    /// 런타임 글로벌 세이브 데이터
    /// </summary>
    public class GlobalSaveData
    {
        private readonly HashSet<string> clearedStageIds = new();

        public IReadOnlyCollection<string> ClearedStageIds => clearedStageIds;

        public bool IsStageCleared(string stageId)
        {
            return !string.IsNullOrEmpty(stageId) && clearedStageIds.Contains(stageId);
        }

        public void MarkStageCleared(string stageId)
        {
            if (string.IsNullOrEmpty(stageId))
            {
                Logger.Warning("Tried to mark empty stage id as cleared.", "GlobalSaveData");
                return;
            }

            clearedStageIds.Add(stageId);
        }

        public void LoadFromFileData(GlobalFileDataV1 fileData)
        {
            clearedStageIds.Clear();

            if (fileData?.ClearedStageIds == null)
                return;

            foreach (string stageId in fileData.ClearedStageIds)
            {
                if (!string.IsNullOrEmpty(stageId))
                    clearedStageIds.Add(stageId);
            }
        }

        public GlobalFileDataV1 ToFileData()
        {
            return new GlobalFileDataV1
            {
                ClearedStageIds = clearedStageIds.ToList()
            };
        }
    }
}