using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static Lumencuit.ISimulationEventListener;

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
        private readonly ViewRoot viewRoot;
        private readonly StageData stageData;
        private readonly Dictionary<Vector2Int, View> views = new();

        public RenderSystem(WorldSystem worldSystem, SimulationSystem simulationSystem, RenderPrefab prefabs, ViewRoot viewRoot, StageData stageData)
        {
            this.worldSystem = worldSystem;
            this.prefabs = prefabs;
            this.viewRoot = viewRoot;
            this.stageData = stageData;

            worldSystem.AddListener(this);
            simulationSystem.AddListener(this);

            var gui = GameObject.Find("StageData").GetComponent<TextMeshProUGUI>();
            gui.text += "<Blueprints>\n";
            foreach (EntityBlueprintStack entityBlueprintStack in stageData.Blueprints)
            {
                if (entityBlueprintStack.Blueprint is ColoredBlueprint coloredBlueprint)
                    gui.text += $"- {coloredBlueprint.Type}[{coloredBlueprint.Signal.Name}] * {entityBlueprintStack.Count}\n";
                else
                    gui.text += $"- {entityBlueprintStack.Blueprint.Type} * {entityBlueprintStack.Count}\n";
            }


            gui.text += "\n<Goals>\n";
            foreach (StageData.StageGoal goal in stageData.Goals)
                gui.text += $"- {goal.Signal.Name} * {goal.Count}\n";

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
                viewObject.SetSignal(Signal.Black);
                viewObject.SetPortSignal(Signal.Black);

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
                view.ViewObject.SetPortSignal(Signal.Black);
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

        public void OnCircuitResultEvent(CircuitResultEvent e)
        {
            CircuitResult result = e.Result;
            var gui = GameObject.Find("CircuitResult").GetComponent<TextMeshProUGUI>();

            if (result == CircuitResult.Fail)
                gui.text = "목표와 다른 회로 구성";
            else if (result == CircuitResult.IncompleteCircuit)
                gui.text = "완성되지 않은 회로";
            else if (result == CircuitResult.CantReach)
                gui.text = "도달 불가능하거나\n사이클인 요소가 있음";
            else if (result == CircuitResult.UnplacedBlueprint)
                gui.text = "설치되지 않은 청사진";
            else if (result == CircuitResult.Success)
                gui.text = "성공!";
        }
    }
}