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

            if (stageController.IsCleared)
                return;

            KeyboardUpdate();
            MouseUpdate();
        }

        private void MouseUpdate()
        {
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

            // Q: 삭제
            if (keyboard.qKey.wasPressedThisFrame)
                SetInputMode(InputMode.Remove);

            // W: 선 연결
            if (keyboard.wKey.wasPressedThisFrame)
                SetInputMode(InputMode.Wire);

            // Z: undo
            if (keyboard.zKey.wasPressedThisFrame)
                Undo();

            // X: redo
            if (keyboard.xKey.wasPressedThisFrame)
                Redo();

            if (keyboard.digit1Key.wasPressedThisFrame && stageData.Blueprints.Count >= 1)
                SelectBlueprint(stageData.Blueprints[0].Blueprint);
            if (keyboard.digit2Key.wasPressedThisFrame && stageData.Blueprints.Count >= 2)
                SelectBlueprint(stageData.Blueprints[1].Blueprint);
            if (keyboard.digit3Key.wasPressedThisFrame && stageData.Blueprints.Count >= 3)
                SelectBlueprint(stageData.Blueprints[2].Blueprint);
            if (keyboard.digit4Key.wasPressedThisFrame && stageData.Blueprints.Count >= 4)
                SelectBlueprint(stageData.Blueprints[3].Blueprint);
            if (keyboard.digit5Key.wasPressedThisFrame && stageData.Blueprints.Count >= 5)
                SelectBlueprint(stageData.Blueprints[4].Blueprint);
            if (keyboard.digit6Key.wasPressedThisFrame && stageData.Blueprints.Count >= 6)
                SelectBlueprint(stageData.Blueprints[5].Blueprint);
            if (keyboard.digit7Key.wasPressedThisFrame && stageData.Blueprints.Count >= 7)
                SelectBlueprint(stageData.Blueprints[6].Blueprint);
            if (keyboard.digit8Key.wasPressedThisFrame && stageData.Blueprints.Count >= 8)
                SelectBlueprint(stageData.Blueprints[7].Blueprint);
            if (keyboard.digit9Key.wasPressedThisFrame && stageData.Blueprints.Count >= 9)
                SelectBlueprint(stageData.Blueprints[8].Blueprint);
        }
    }
}