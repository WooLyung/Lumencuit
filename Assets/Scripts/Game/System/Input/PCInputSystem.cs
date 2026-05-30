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

        public PCInputSystem(Camera camera)
        {
            this.camera = camera;
        }
        
        public override void Update()
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

            Debug.Log(gridTilePos.Pos);
        }
    }
}