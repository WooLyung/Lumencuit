using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;

namespace Lumencuit
{
    /// <summary>
    /// 월드 및 시뮬레이션과 독립적으로 작동하는 렌더링 시스템입니다.
    /// </summary>
    public sealed class RenderSystem : IEntityEventListener, ISignalEventListener
    {
        /// <summary>
        /// 렌더링을 위해 생성된 게임 오브젝트와 정보입니다.
        /// </summary>
        private readonly struct View
        {
            public readonly GameObject GameObject;
            public readonly ViewObject ViewObject;

            public View(GameObject gameObject, ViewObject viewObject)
            {
                GameObject = gameObject;
                ViewObject = viewObject;
            }
        }

        private readonly WorldSystem worldSystem;
        private readonly RenderPrefab prefabs;
        private readonly ViewRoot viewRoot;
        private readonly Dictionary<Vector2Int, View> views = new();

        public RenderSystem(WorldSystem worldSystem, SimulationSystem simulationSystem, RenderPrefab prefabs, ViewRoot viewRoot)
        {
            this.worldSystem = worldSystem;
            this.prefabs = prefabs;
            this.viewRoot = viewRoot;
            worldSystem.AddListener(this);
            simulationSystem.AddListener(this);

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

                    GameObject tile = Object.Instantiate(prefabs.Tile, viewRoot.GridMesh);
                    tile.transform.position = new Vector3(x, y, 0);
                    tile.name = $"Tile[{x}][{y}]";

                    GameObject gridCollider = Object.Instantiate(prefabs.GridCollider, viewRoot.GridColliders);
                    gridCollider.transform.position = new Vector3(x, y, 0);
                    gridCollider.name = $"GridCollider[{x}][{y}]";
                    gridCollider.GetComponent<GridTilePos>().Pos = new Vector2Int(x, y);
                }
            }
        }

        public void OnEntityCreated(IEntityEventListener.EntityCreatedEvent e)
        {
            GameObject prefab = prefabs.GetCircuitElement(e.Entity.Element.Type);
            if (prefab != null)
            {
                GameObject view = Object.Instantiate(prefab, viewRoot.Entities);
                view.transform.position = new Vector3(e.Pos.x, e.Pos.y, -1);
                view.name = $"Entity[{e.Pos.x}][{e.Pos.y}]";

                ViewObject viewObject = view.GetComponent<ViewObject>();
                viewObject.PortUpdate(e.Entity);
                viewObject.SetColor(e.Entity.MadeBy.SignalColor.ToColor());
                viewObject.SetPortColor(Signal.SignalColor.Black.ToColor());

                views.Add(e.Pos, new View(view, viewObject));
            }
        }

        public void OnEntityRemoved(IEntityEventListener.EntityRemovedEvent e)
        {
            if (views.TryGetValue(e.Pos, out View view))
            {
                Object.Destroy(view.GameObject);
                views.Remove(e.Pos);
            }
        }

        public void OnEntityPortUpdated(IEntityEventListener.EntityPortUpdatedEvent e)
        {
            if (views.TryGetValue(e.Pos, out View view))
            {
                view.ViewObject.PortUpdate(e.Entity);
                view.ViewObject.SetPortColor(Signal.SignalColor.Black.ToColor());
            }
        }

        public void OnSignalUpdated(ISignalEventListener.SignalUpdatedEvent e)
        {
            if (views.TryGetValue(e.Pos, out View view))
                view.ViewObject.SetColor(e.Entity.CurrSignal.ToColor());
        }

        public void OnPortSignalUpdated(ISignalEventListener.PortSignalUpdatedEvent e)
        {
            if (views.TryGetValue(e.Pos, out View view))
                view.ViewObject.SetPortColor(e.Dir, e.Signal.ToColor());
        }
    }
}