using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace Lumencuit
{
    /// <summary>
    /// 회로 요소를 그리드 기반으로 저장하는 객체입니다.
    /// </summary>
    public sealed class WorldGrid
    {
        public readonly int Width;
        public readonly int Height;
        private readonly Entity[,] grid;
        private readonly bool[,] enabledTiles;

        public WorldGrid(StageData stageData)
        {
            Width = stageData.Width;
            Height = stageData.Height;

            grid = new Entity[Width, Height];
            enabledTiles = new bool[Width, Height];

            for (int x = 0; x < Width; x++)
                for (int y = 0; y < Height; y++)
                    enabledTiles[x, y] = stageData.IsEnabledTile(x, y);
        }

        public WorldGrid(int width, int height, Entity[,] grid, bool[,] enabledTiles)
        {
            Width = width;
            Height = height;
            this.grid = grid;
            this.enabledTiles = enabledTiles;
        }

        public bool IsInside(int x, int y) => x >= 0 && x < Width && y >= 0 && y < Height;
        public bool IsEnabledTile(int x, int y) => IsInside(x, y) && enabledTiles[x, y];
        public bool HasEntityAt(int x, int y) => IsEnabledTile(x, y) && grid[x, y] != null;
        public Entity GetEntityAt(int x, int y) => IsEnabledTile(x, y) ? grid[x, y] : null;

        public bool TryGetEntityAt(int x, int y, out Entity entity)
        {
            entity = null;
            if (!HasEntityAt(x, y))
                return false;
            entity = grid[x, y];
            return true;
        }

        public IEnumerable<Vector2Int> GetAllSourcePositions()
        {
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    if (!HasEntityAt(x, y))
                        continue;
                    if (grid[x, y].Element.Type == CircuitElement.CircuitElementType.Source)
                        yield return new Vector2Int(x, y);
                }
            }
        }

        public IEnumerable<Vector2Int> GetAllGoalPositions()
        {
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    if (!HasEntityAt(x, y))
                        continue;
                    if (grid[x, y].Element.IsGoal)
                        yield return new Vector2Int(x, y);
                }
            }
        }

        public void SetEntityAt(Entity entity, int x, int y)
        {
            if (IsEnabledTile(x, y))
                grid[x, y] = entity;
        }

        public void RemoveEntityAt(int x, int y)
        {
            if (IsEnabledTile(x, y))
                grid[x, y] = null;
        }

        public WorldGrid Clone()
        {
            Entity[,] grid = new Entity[Width, Height];
            bool[,] enabledTiles = new bool[Width, Height];

            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    if (this.grid[x, y] == null)
                        grid[x, y] = null;
                    else
                        grid[x, y] = this.grid[x, y].Clone();
                    enabledTiles[x, y] = this.enabledTiles[x, y];
                }
            }

            return new WorldGrid(Width, Height, grid, enabledTiles);
        }

        public IEnumerable<Vector2Int> GetAllEntityPositions()
        {
            for (int x = 0; x < Width; x++)
                for (int y = 0; y < Height; y++)
                    if (HasEntityAt(x, y))
                        yield return new Vector2Int(x, y);
        }
    }
}