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

        public WorldGrid(int width, int height)
        {
            Width = width;
            Height = height;
            grid = new Entity[Width, Height];
        }

        public bool HasEntityAt(int x, int y) => grid[x, y] != null;
        public Entity GetEntityAt(int x, int y) => grid[x, y];
        public void SetEntityAt(Entity entity, int x, int y) => grid[x, y] = entity;
    }
}