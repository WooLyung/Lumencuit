using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace Lumencuit
{
    /// <summary>
    /// PC 환경에서 작동하는 인풋 시스템입니다.
    /// </summary>
    public sealed class PCInputSystem : InputSystem
    {
        private readonly StageData stageData;

        public PCInputSystem(WorldSystem worldSystem, StageController stageController, Camera camera, StageData stageData) : base(worldSystem, stageController, camera)
        {
            this.stageData = stageData;
        }

        protected override bool TryGetPointerTilePos(out Vector2Int pos)
        {
            pos = default;

            Mouse mouse = Mouse.current;
            if (mouse == null)
                return false;

            Vector2 mousePosition = mouse.position.ReadValue();
            Ray ray = camera.ScreenPointToRay(mousePosition);

            if (!Physics.Raycast(ray, out RaycastHit hit))
                return false;
            
            if (!hit.collider.TryGetComponent(out GridTilePos gridTilePos))
                return false;

            pos = gridTilePos.Pos;
            return true;
        }
        
        protected override bool IsPointerPressedThisFrame()
        {
            Mouse mouse = Mouse.current;
            if (mouse == null)
                return false;
            return mouse.leftButton.wasPressedThisFrame;
        }

        protected override bool IsPointerPressed()
        {
            Mouse mouse = Mouse.current;
            if (mouse == null)
                return false;
            return !mouse.leftButton.wasPressedThisFrame && !mouse.leftButton.wasReleasedThisFrame && mouse.leftButton.isPressed;
        }

        protected override bool IsPointerReleasedThisFrame()
        {
            Mouse mouse = Mouse.current;
            if (mouse == null)
                return false;
            return mouse.leftButton.wasReleasedThisFrame;
        }

        protected override bool IsPointerBlockedByUI()
        {
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                return true;
            return false;
        }

        public override void Update()
        {
            base.Update();
            KeyboardUpdate();
            MouseUpdate();
        }

        private void MouseUpdate()
        {
            if (stageController.IsCleared)
                return;
            if (IsPointerBlockedByUI())
                return;
            if (!TryGetPointerTilePos(out Vector2Int pos))
                return;

            Mouse mouse = Mouse.current;
            if (mouse == null)
                return;

            if (mouse.rightButton.wasPressedThisFrame)
                RemoveEntity(pos.x, pos.y);
        }

        private void KeyboardUpdate()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null)
                return;

            // ESC: 스테이지 선택으로 돌아가기
            if (keyboard.escapeKey.wasPressedThisFrame)
                Back();

            if (stageController.IsCleared)
                return;

            // Q: 삭제
            if (keyboard.qKey.wasPressedThisFrame)
                SetInputMode(InputMode.Remove);

            // W: 선 연결
            if (keyboard.wKey.wasPressedThisFrame)
                SetInputMode(InputMode.Wire);

            // R: 초기화
            if (keyboard.rKey.wasPressedThisFrame)
                Reset();

            // Z: undo
            if (keyboard.zKey.wasPressedThisFrame)
                Undo();

            // X: redo
            if (keyboard.xKey.wasPressedThisFrame)
                Redo();
        }
    }
}