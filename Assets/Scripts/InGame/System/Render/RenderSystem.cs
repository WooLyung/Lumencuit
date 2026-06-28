using System.Collections.Generic;
using UnityEngine;

namespace Lumencuit
{
    /// <summary>
    /// 월드 및 시뮬레이션과 독립적으로 작동하는 렌더링 시스템입니다.
    /// </summary>
    public sealed class RenderSystem : IEntityEventListener, ISimulationEventListener
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
        private readonly Mesh tileMesh;
        private readonly ViewRoot viewRoot;
        private readonly Dictionary<Vector2Int, View> views = new();

        public RenderSystem(WorldSystem worldSystem, SimulationSystem simulationSystem, RenderPrefab prefabs, Mesh tileMesh, ViewRoot viewRoot)
        {
            this.worldSystem = worldSystem;
            this.prefabs = prefabs;
            this.tileMesh = tileMesh;
            this.viewRoot = viewRoot;

            simulationSystem.AddListener(this);
            worldSystem.AddListener(this);
            RenderGrid();
        }

        private void RenderGridMesh()
        {
            List<CombineInstance> combines = new();

            for (int x = 0; x < worldSystem.Width; x++)
                for (int y = 0; y < worldSystem.Height; y++)
                    if (worldSystem.IsEnabledTile(x, y))
                        combines.Add(new CombineInstance { mesh = tileMesh, transform = Matrix4x4.TRS(new Vector3(x, y, 0), Quaternion.identity, Vector3.one)});

            Mesh combinedMesh = new Mesh { name = "GridMesh", indexFormat = UnityEngine.Rendering.IndexFormat.UInt32 };
            combinedMesh.CombineMeshes(combines.ToArray(), mergeSubMeshes: true, useMatrices: true);
            combinedMesh.RecalculateBounds();

            viewRoot.GridMesh.GetComponent<MeshFilter>().sharedMesh = combinedMesh;
        }

        private void RenderGridCollider()
        {
            for (int x = 0; x < worldSystem.Width; x++)
            {
                for (int y = 0; y < worldSystem.Height; y++)
                {
                    if (!worldSystem.IsEnabledTile(x, y))
                        continue;

                    GameObject gridCollider = Object.Instantiate(prefabs.GridCollider, viewRoot.GridColliders);
                    gridCollider.transform.position = new Vector3(x, y, 0);
                    gridCollider.name = $"GridCollider[{x}][{y}]";
                    gridCollider.GetComponent<GridTilePos>().Pos = new Vector2Int(x, y);
                }
            }
        }

        private void RenderGrid()
        {
            RenderGridMesh();
            RenderGridCollider();

            viewRoot.transform.localPosition = new Vector3(-(worldSystem.Width - 1) * 0.5f, -(worldSystem.Height - 1) * 0.5f, 0);
        }

        public void OnEntityCreated(IEntityEventListener.EntityCreatedEvent e)
        {
            GameObject prefab = prefabs.GetCircuitElement(e.Entity.Element.Type);
            if (prefab != null)
            {
                GameObject view = Object.Instantiate(prefab, viewRoot.Entities);
                view.transform.localPosition = new Vector3(e.Pos.x, e.Pos.y, 0);
                view.name = $"Entity[{e.Pos.x}][{e.Pos.y}]";

                ViewObject viewObject = view.GetComponent<ViewObject>();
                viewObject.PortUpdate(e.Entity.GetPorts());
                viewObject.SetSignal(QuantumSignal.Null);
                viewObject.SetPortSignal(QuantumSignal.Null);

                views.Add(e.Pos, new View(view, viewObject));
            }
        }

        public void OnEntityRemoved(IEntityEventListener.EntityRemovedEvent e)
        {
            if (views.TryGetValue(e.Pos, out View view))
            {
                view.ViewObject.Destroy();
                views.Remove(e.Pos);
            }
        }

        public void OnEntityPortUpdated(IEntityEventListener.EntityPortUpdatedEvent e)
        {
            if (views.TryGetValue(e.Pos, out View view))
            {
                view.ViewObject.PortUpdate(e.Entity.GetPorts());
                view.ViewObject.SetPortSignal(QuantumSignal.Null);
            }
        }

        public void OnSignalUpdated(ISimulationEventListener.SignalUpdatedEvent e)
        {
            if (views.TryGetValue(e.Pos, out View view))
                view.ViewObject.SetSignal(e.Signal);
        }

        public void OnPortSignalUpdated(ISimulationEventListener.PortSignalUpdatedEvent e)
        {
            if (views.TryGetValue(e.Pos, out View view))
                view.ViewObject.SetPortSignal(e.Dir, e.Signal);
        }
    }
}