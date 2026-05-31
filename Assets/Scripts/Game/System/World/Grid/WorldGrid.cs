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
        private bool[,] enabledTiles;

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

        public bool IsInside(int x, int y) => x >= 0 && x < Width && y >= 0 && y < Height;
        public bool IsEnabledTile(int x, int y) => IsInside(x, y) && enabledTiles[x, y];
        public bool HasEntityAt(int x, int y) => IsEnabledTile(x, y) && grid[x, y] != null;
        public Entity GetEntityAt(int x, int y) => IsEnabledTile(x, y) ? grid[x, y] : null;

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
    }
}