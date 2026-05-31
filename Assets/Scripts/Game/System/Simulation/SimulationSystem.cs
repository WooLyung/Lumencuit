namespace Lumencuit
{
    /// <summary>
    /// 게임의 핵심 로직을 시뮬레이션하는 시스템입니다.
    /// </summary>
    public sealed class SimulationSystem : IEntityEventListener
    {
        private readonly StageData stageData;

        public SimulationSystem(WorldSystem worldSystem, StageData stageData)
        {
            worldSystem.AddListener(this);
            this.stageData = stageData;
        }

        private void FlowAll()
        {
        }

        public void OnEntityCreated(IEntityEventListener.EntityCreatedEvent e)
        {
        }

        public void OnEntityRemoved(IEntityEventListener.EntityRemovedEvent e)
        {
        }

        public void OnEntityPortUpdated(IEntityEventListener.EntityPortUpdatedEvent e)
        {
        }
    }
}