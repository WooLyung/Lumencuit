using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static Lumencuit.ISimulationEventListener;

namespace Lumencuit
{
    /// <summary>
    /// 인게임 씬의 UI를 관리하고 인풋 시스템을 연결합니다.
    /// </summary>
    public class InGameUIAdaptor : MonoBehaviour, IEntityEventListener, ISimulationEventListener
    {
        // UI
        [SerializeField] private RectTransform blueprintView;
        [SerializeField] private TextMeshProUGUI circuitResult;
        [SerializeField] private TextMeshProUGUI goals;

        // 프리팹
        [SerializeField] private GameObject blueprint;

        private InputSystem inputSystem;

        public void Init(WorldSystem worldSystem, InputSystem inputSystem, SimulationSystem simulationSystem, StageData stageData)
        {
            this.inputSystem = inputSystem;
            worldSystem.AddListener(this);
            simulationSystem.AddListener(this);

            RefreshBlueprints(worldSystem.GetBlueprints());
            WriteGoals(stageData);
        }

        private void WriteGoals(StageData stageData)
        {
            goals.text = "<목표>\n";
            foreach (StageGoal goal in stageData.Goals)
                goals.text += $"- {goal.Signal} * {goal.Count}\n";
        }

        private void RefreshBlueprints(IEnumerable<EntityBlueprintStack> blueprints)
        {
            for (int j = blueprintView.childCount - 1; j >= 0; j--)
                Destroy(blueprintView.GetChild(j).gameObject);

            int i = 0;
            foreach (var bp in blueprints)
            {
                GameObject newBlueprint = Instantiate(blueprint, blueprintView);
                BlueprintUI ui = newBlueprint.GetComponent<BlueprintUI>();

                if (ui == null)
                {
                    Destroy(newBlueprint);
                    continue;
                }

                newBlueprint.name = "Blueprint";
                ui.RectTransform.anchoredPosition = new Vector2(300 * i++, ui.RectTransform.anchoredPosition.y);
                ui.Blueprint = bp.Blueprint.Clone();
                ui.SetInGameUIAdaptor(this);

                if (ui.Blueprint is ColoredBlueprint coloredBlueprint)
                    ui.Text.text = $"{coloredBlueprint.Type} {coloredBlueprint.Signal}\nx{bp.Count}\n";
                else
                    ui.Text.text = $"{ui.Blueprint.Type}\nx{bp.Count}";
            }

            blueprintView.sizeDelta = new Vector2(300 * i, blueprintView.sizeDelta.y);
        }

        public void OnGridUpdated(IEntityEventListener.GridUpdatedEvent e)
        {
            RefreshBlueprints(e.BlueprintsClone);
        }

        public void SelectBlueprint(EntityBlueprint blueprint)
        {
            inputSystem.SelectBlueprint(blueprint);
        }

        public void OnCircuitResultEvent(CircuitResultEvent e)
        {
            CircuitResult result = e.Result;
            if (result == CircuitResult.Fail)
                circuitResult.text = "목표와 다른 회로 구성";
            else if (result == CircuitResult.IncompleteCircuit)
                circuitResult.text = "완성되지 않은 회로";
            else if (result == CircuitResult.CantReach)
                circuitResult.text = "도달 불가능한 요소";
            else if (result == CircuitResult.HasCycle)
                circuitResult.text = "사이클이 있음";
            else if (result == CircuitResult.MultipleSignalGenerators)
                circuitResult.text = "너무 많은 신호 생성기";
            else if (result == CircuitResult.UnplacedBlueprint)
                circuitResult.text = "설치되지 않은 청사진";
            else if (result == CircuitResult.Success)
                circuitResult.text = "성공!";
        }
    }
}