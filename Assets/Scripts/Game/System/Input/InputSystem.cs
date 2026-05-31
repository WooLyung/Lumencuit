using UnityEngine;

namespace Lumencuit
{
    /// <summary>
    /// 유니티의 입력을 가공하여 제공하는 시스템입니다.
    /// 구동 환경에 따라 서로 다른 입력을 정의합니다.
    /// </summary>
    public abstract class InputSystem
    {
        protected WorldSystem worldSystem;
        private EntityBlueprint selectedBlueprint = null;

        public InputSystem(WorldSystem worldSystem)
        {
            this.worldSystem = worldSystem;
        }

        public abstract void Update();

        protected void SelectBlueprint(EntityBlueprint blueprint)
        {
            selectedBlueprint = blueprint;
        }

        protected void PlaceBlueprint(int x, int y)
        {
            if (selectedBlueprint == null)
                return;
            worldSystem.TryCreateEntityByBlueprint(selectedBlueprint, x, y);
            selectedBlueprint = null;
        }
    }
}