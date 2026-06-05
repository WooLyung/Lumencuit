using UnityEngine;

namespace Lumencuit
{
    /// <summary>
    /// 대응되지 않는 환경을 위한 미구현 인풋 시스템입니다.
    /// </summary>
    public sealed class NullInputSystem : InputSystem
    {
        public NullInputSystem(WorldSystem worldSystem, Camera camera) : base(worldSystem, camera)
        {
        }

        protected override bool IsPointerBlockedByUI() => false;
        protected override bool IsPointerPressed() => false;
        protected override bool IsPointerPressedThisFrame() => false;
        protected override bool IsPointerReleasedThisFrame() => false;

        protected override bool TryGetPointerTilePos(out Vector2Int pos)
        {
            pos = default;
            return false;
        }
    }
}