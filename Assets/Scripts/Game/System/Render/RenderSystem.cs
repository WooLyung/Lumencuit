using UnityEngine;

namespace Lumencuit
{
    /// <summary>
    /// 월드 및 시뮬레이션과 독립적으로 작동하는 렌더링 시스템입니다.
    /// </summary>
    public sealed class RenderSystem : IEntityEventListener
    {
        private readonly WorldSystem worldSystem;
        private readonly RenderPrefabRegistry prefabs;
        private readonly Transform root;

        public RenderSystem(WorldSystem worldSystem, RenderPrefabRegistry prefabs, Transform root)
        {
            this.worldSystem = worldSystem;
            this.prefabs = prefabs;
            this.root = root;
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

                    GameObject tile = Object.Instantiate(prefabs.Tile, root);
                    tile.transform.position = new Vector3(x, y, 0);
                }
            }
        }

        public void OnEntityCreated(IEntityEventListener.EntityCreatedEvent e)
        {
            Debug.Log(e.Entity.Element.Id);
        }

        public void OnEntityRemoved(IEntityEventListener.EntityRemovedEvent e)
        {
        }
    }
}