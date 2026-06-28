using static Lumencuit.ISimulationEventListener;

namespace Lumencuit
{
    /// <summary>
    /// 현 스테이지의 정보를 관리합니다.
    /// </summary>
    public sealed class StageController : ISimulationEventListener
    {
        private bool isCleared;

        public bool IsCleared => isCleared;

        public StageController(SimulationSystem simulationSystem)
        {
            simulationSystem.AddListener(this);
        }

        public void OnCircuitResultEvent(CircuitResultEvent e)
        {
            if (isCleared)
                return;

            if (e.Result != CircuitResult.Success)
                return;
            
            isCleared = true;
            SaveManagement.ClearCurrentStage();
            SaveManagement.MarkStageCleared(e.StageData.StageId);
        }
    }
}