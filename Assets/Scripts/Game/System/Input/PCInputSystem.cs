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
        private readonly Camera camera;

        public PCInputSystem(WorldSystem worldSystem, Camera camera) : base(worldSystem)
        {
            this.camera = camera;
        }
        
        public override void Update()
        {
            MouseClick();
            KeyboardClick();
        }

        private void KeyboardClick()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null)
                return;

            if (keyboard.digit1Key.wasPressedThisFrame)
                SelectBlueprint(new EntityBlueprint(CircuitElement.CircuitElementType.Source, Signal.SignalColor.Red));
            if (keyboard.digit2Key.wasPressedThisFrame)
                SelectBlueprint(new EntityBlueprint(CircuitElement.CircuitElementType.Lamp, Signal.SignalColor.Black));
        }

        private void MouseClick()
        {
            Mouse mouse = Mouse.current;
            if (mouse == null)
                return;

            if (!mouse.leftButton.wasPressedThisFrame)
                return;
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                return;

            Vector2 mousePosition = mouse.position.ReadValue();
            Ray ray = camera.ScreenPointToRay(mousePosition);
            if (!Physics.Raycast(ray, out RaycastHit hit))
                return;
            if (!hit.collider.TryGetComponent(out GridTilePos gridTilePos))
                return;

            PlaceBlueprint(gridTilePos.Pos.x, gridTilePos.Pos.y);
        }
    }
}