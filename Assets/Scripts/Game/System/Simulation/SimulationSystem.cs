namespace Lumencuit
{
    /// <summary>
    /// 게임의 핵심 로직을 시뮬레이션하는 시스템입니다.
    /// </summary>
    public sealed class SimulationSystem : IEntityEventListener
    {
        public SimulationSystem(WorldSystem worldSystem)
        {
            worldSystem.AddListener(this);
        }

        private void FlowAll()
        {

        }

        public void OnEntityCreated(IEntityEventListener.EntityCreatedEvent e)
        {
            FlowAll();
        }

        public void OnEntityRemoved(IEntityEventListener.EntityRemovedEvent e)
        {
            FlowAll();
        }
    }
}