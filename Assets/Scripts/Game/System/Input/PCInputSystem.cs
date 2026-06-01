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
        private Vector2Int preTarget = new Vector2Int(-1, -1);

        public PCInputSystem(WorldSystem worldSystem, Camera camera) : base(worldSystem)
        {
            this.camera = camera;
        }
        
        public override void Update()
        {
            Mouse();
            Keyboard();
        }

        private bool TryGetTargetPos(out Vector2Int pos)
        {
            Mouse mouse = UnityEngine.InputSystem.Mouse.current;
            pos = Vector2Int.zero;

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

        private void Keyboard()
        {
            Keyboard keyboard = UnityEngine.InputSystem.Keyboard.current;
            if (keyboard == null)
                return;

            if (keyboard.digit1Key.wasPressedThisFrame)
                SelectBlueprint(new EntityBlueprint(CircuitElement.CircuitElementType.Source, Signal.SignalColor.Red));
            if (keyboard.digit2Key.wasPressedThisFrame)
                SelectBlueprint(new EntityBlueprint(CircuitElement.CircuitElementType.Source, Signal.SignalColor.Blue));
            if (keyboard.digit3Key.wasPressedThisFrame)
                SelectBlueprint(new EntityBlueprint(CircuitElement.CircuitElementType.Lamp, Signal.SignalColor.Black));
            if (keyboard.digit4Key.wasPressedThisFrame)
                SelectBlueprint(new EntityBlueprint(CircuitElement.CircuitElementType.OrGate, Signal.SignalColor.Black));
        }

        private void Mouse()
        {
            Mouse mouse = UnityEngine.InputSystem.Mouse.current;
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            {
                EndPath();
                return;
            }
            if (!TryGetTargetPos(out Vector2Int pos))
            {
                EndPath();
                return;
            }
            Entity entity = worldSystem.GetEntityAt(pos.x, pos.y);

            if (mouse.leftButton.wasPressedThisFrame)
            {
                if (entity == null)
                    PlaceBlueprint(pos.x, pos.y);
                else if (entity.OutPortCount < entity.Element.OutSignalCount)
                    StartPath(pos.x, pos.y);
            }
            else if (mouse.leftButton.wasReleasedThisFrame)
                EndPath();
            else if (mouse.leftButton.isPressed)
            {
                if (preTarget != pos)
                    NextPath(pos.x, pos.y);
            }

            if (mouse.rightButton.wasPressedThisFrame)
                worldSystem.TryRemoveEntity(pos.x, pos.y);

            preTarget = pos;
        }
    }
}