using System;
using System.Collections.Generic;
using UnityEngine;

namespace Lumencuit
{
    /// <summary>
    /// 챕터의 정보를 포함하는 데이터 클래스입니다.
    /// </summary>
    [CreateAssetMenu(fileName = "ChapterData", menuName = "Lumencuit/Chapter Data")]
    public sealed class ChapterData : ScriptableObject
    {
        [Flags]
        public enum DirectionFlags
        {
            None = 0, Up = 1, Down = 2, Left = 4, Right = 8
        }

        [Serializable]
        public sealed class ChapterTileData
        {
            public bool Enabled;
            public int StageNumber;
            public DirectionFlags InputDirections;
        }

        [Serializable]
        public sealed class ChapterStageInfo
        {
            public int StageNumber;
            public StageData StageData;
            public bool IsHard;
        }

        public string ChapterId;

        public int Width = 1;
        public int Height = 1;

        [SerializeField] private ChapterTileData[] tiles = new ChapterTileData[1];
        public List<ChapterStageInfo> StageInfos = new();

        public ChapterTileData GetTile(int x, int y)
        {
            if (!IsInside(x, y))
                return null;

            return tiles[GetIndex(x, y)];
        }

        public bool IsEnabledTile(int x, int y)
        {
            ChapterTileData tile = GetTile(x, y);

            if (tile == null)
                return false;

            return tile.Enabled;
        }

        public void SetEnabledTile(int x, int y, bool value)
        {
            ChapterTileData tile = GetTile(x, y);

            if (tile == null)
                return;

            tile.Enabled = value;
        }

        public int GetStageNumber(int x, int y)
        {
            ChapterTileData tile = GetTile(x, y);

            if (tile == null)
                return 0;

            return tile.StageNumber;
        }

        public void SetStageNumber(int x, int y, int stageNumber)
        {
            ChapterTileData tile = GetTile(x, y);

            if (tile == null)
                return;

            tile.StageNumber = stageNumber;
        }

        public bool HasInput(int x, int y)
        {
            ChapterTileData tile = GetTile(x, y);

            if (tile == null)
                return false;

            return tile.InputDirections != 0;
        }

        public bool HasInputDirection(int x, int y, DirectionFlags direction)
        {
            ChapterTileData tile = GetTile(x, y);

            if (tile == null)
                return false;

            return (tile.InputDirections & direction) != 0;
        }

        public ChapterStageInfo GetStageInfo(int stageNumber)
        {
            foreach (ChapterStageInfo stageInfo in StageInfos)
            {
                if (stageInfo.StageNumber == stageNumber)
                    return stageInfo;
            }

            return null;
        }

        public void ResizeTiles()
        {
            int newSize = Width * Height;

            if (tiles == null || tiles.Length != newSize)
            {
                ChapterTileData[] oldTiles = tiles;
                ChapterTileData[] newTiles = new ChapterTileData[newSize];

                for (int i = 0; i < newTiles.Length; i++)
                    newTiles[i] = new ChapterTileData();

                if (oldTiles != null)
                {
                    int copyLength = Mathf.Min(oldTiles.Length, newTiles.Length);

                    for (int i = 0; i < copyLength; i++)
                        if (oldTiles[i] != null)
                            newTiles[i] = oldTiles[i];
                }

                tiles = newTiles;
            }
            else
            {
                for (int i = 0; i < tiles.Length; i++)
                    if (tiles[i] == null)
                        tiles[i] = new ChapterTileData();
            }
        }

        private bool IsInside(int x, int y)
        {
            return x >= 0 && x < Width && y >= 0 && y < Height;
        }

        private int GetIndex(int x, int y)
        {
            return y * Width + x;
        }
    }
}