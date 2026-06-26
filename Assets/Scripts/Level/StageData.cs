using System;
using System.Collections.Generic;
using UnityEngine;

namespace Lumencuit
{
    /// <summary>
    /// 스테이지의 모든 정보를 포함하는 데이터 클래스입니다.
    /// </summary>
    [CreateAssetMenu(fileName = "StageData", menuName = "Lumencuit/Stage Data")]
    public sealed class StageData : ScriptableObject
    {
        public string StageId;

        public int Width = 1;
        public int Height = 1;
        public List<PrePlacedBlueprint> PrePlacedBlueprints = new();
        public List<EntityBlueprintStack> Blueprints = new();
        public List<StageGoal> Goals = new();

        [SerializeField] private bool[] enabledTiles = new bool[1];

        public bool IsEnabledTile(int x, int y)
        {
            if (!IsInside(x, y))
                return false;
            return enabledTiles[GetIndex(x, y)];
        }

        public void SetEnabledTile(int x, int y, bool value)
        {
            if (!IsInside(x, y))
                return;
            enabledTiles[GetIndex(x, y)] = value;
        }

        public void ResizeTiles()
        {
            int newSize = Width * Height;

            if (enabledTiles != null && enabledTiles.Length == newSize)
                return;

            bool[] oldTiles = enabledTiles;
            bool[] newTiles = new bool[newSize];

            if (oldTiles != null)
            {
                int copyLength = Mathf.Min(oldTiles.Length, newTiles.Length);
                for (int i = 0; i < copyLength; i++)
                    newTiles[i] = oldTiles[i];
            }

            enabledTiles = newTiles;
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