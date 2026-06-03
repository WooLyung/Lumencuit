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
        private readonly StageData stageData;
        private Vector2Int preTarget = new Vector2Int(-1, -1);

        public PCInputSystem(WorldSystem worldSystem, Camera camera, StageData stageData) : base(worldSystem)
        {
            this.camera = camera;
            this.stageData = stageData;
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
            {
                EndPath();
            }
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