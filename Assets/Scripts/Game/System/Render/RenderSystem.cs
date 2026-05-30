using UnityEngine;

namespace Lumencuit
{
    /// <summary>
    /// 월드 및 시뮬레이션과 독립적으로 작동하는 렌더링 시스템입니다.
    /// </summary>
    public sealed class RenderSystem : IEntityEventListener
    {
        private readonly WorldSystem worldSystem;
        private readonly RenderPrefab prefabs;
        private readonly Views views;

        public RenderSystem(WorldSystem worldSystem, RenderPrefab prefabs, Views views)
        {
            this.worldSystem = worldSystem;
            this.prefabs = prefabs;
            this.views = views;
            worldSystem.AddListener(this);

            RenderGrid();
        }

        private void RenderGrid()
        {
            for (int x = 0; x < worldSystem.Width; x++)
            {
                for (int y = 0; y < worldSystem.Height; y++)
                {
                    if (!worldSystem.IsEnabledTile(x, y))
                        continue;

                    GameObject tile = Object.Instantiate(prefabs.Tile, views.GridMesh);
                    tile.transform.position = new Vector3(x, y, 0);

                    GameObject gridCollider = Object.Instantiate(prefabs.GridCollider, views.GridColliders);
                    gridCollider.transform.position = new Vector3(x, y, 0);
                    gridCollider.name = $"GridCollider[{x}][{y}]";
                    gridCollider.GetComponent<GridTilePos>().Pos = new Vector2Int(x, y);
                }
            }
        }

        public void OnEntityCreated(IEntityEventListener.EntityCreatedEvent e)
        {
        }

        public void OnEntityRemoved(IEntityEventListener.EntityRemovedEvent e)
        {
        }
    }
}