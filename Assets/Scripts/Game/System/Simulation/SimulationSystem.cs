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

        public void OnEntityCreate(IEntityEventListener.EntityCreateEvent e)
        {
        }

        public void OnEntityRemove(IEntityEventListener.EntityRemoveEvent e)
        {
        }
    }
}