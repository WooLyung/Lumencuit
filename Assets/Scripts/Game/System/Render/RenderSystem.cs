using UnityEngine;

namespace Lumencuit
{
    /// <summary>
    /// 월드 및 시뮬레이션과 독립적으로 작동하는 렌더링 시스템입니다.
    /// </summary>
    public sealed class RenderSystem : IEntityEventListener
    {
        private readonly Transform root;

        public RenderSystem(WorldSystem worldSystem, Transform root)
        {
            worldSystem.AddListener(this);
            this.root = root;
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