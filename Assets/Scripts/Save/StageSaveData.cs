using System.Collections.Generic;

namespace Lumencuit.Save
{
    /// <summary>
    /// 런타임 스테이지 세이브 데이터
    /// </summary>
    public sealed class StageSaveData
    {
        public string StageId { get; private set; }
        private readonly List<EntityFileDataV1> entities = new();

        public IReadOnlyList<EntityFileDataV1> Entities => entities;

        public StageSaveData(string stageId)
        {
            StageId = stageId;
        }

        public void AddEntity(EntityFileDataV1 entity)
        {
            entities.Add(entity);
        }

        public StageFileDataV1 ToFileData()
        {
            StageFileDataV1 fileData = new StageFileDataV1
            {
                StageId = StageId
            };

            fileData.Entities.AddRange(entities);

            return fileData;
        }

        public void LoadFromFileData(StageFileDataV1 fileData)
        {
            StageId = fileData.StageId;

            entities.Clear();

            if (fileData.Entities != null)
                entities.AddRange(fileData.Entities);
        }
    }
}