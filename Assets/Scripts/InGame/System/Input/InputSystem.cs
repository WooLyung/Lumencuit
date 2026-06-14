using System.Collections.Generic;
using UnityEngine;

namespace Lumencuit
{
    /// <summary>
    /// 유니티의 입력을 가공하여 제공하는 시스템입니다.
    /// 구동 환경에 따라 서로 다른 입력을 정의합니다.
    /// </summary>
    public abstract class InputSystem
    {
        /// <summary>
        /// 선택된 입력 모드입니다.
        /// </summary>
        public enum InputMode
        {
            None, Place, Wire, Remove
        }

        /// <summary>
        /// 드래그 상태입니다.
        /// </summary>
        public enum DragState
        {
            None,
            Dragging
        }

        protected readonly WorldSystem worldSystem;
        protected readonly StageController stageController;
        protected readonly Camera camera;

        private InputMode inputMode = InputMode.None;
        private DragState dragState = DragState.None;

        private EntityBlueprint selectedBlueprint = null;
        private Vector2Int dragStartPos;
        private Vector2Int prevDragPos = new(-1, -1);
        private List<Vector2Int> path = new();

        public InputSystem(WorldSystem worldSystem, StageController stageController, Camera camera)
        {
            this.worldSystem = worldSystem;
            this.stageController = stageController;
            this.camera = camera;
        }

        /// <summary>
        /// 포인터가 위치한 타일을 구합니다.
        /// </summary>
        protected abstract bool TryGetPointerTilePos(out Vector2Int pos);

        /// <summary>
        /// 포인터 프레스를 검사합니다.
        /// </summary>
        protected abstract bool IsPointerPressedThisFrame();

        /// <summary>
        /// 포인터 클릭를 검사합니다.
        /// </summary>
        protected abstract bool IsPointerPressed();

        /// <summary>
        /// 포인터 릴리즈를 검사합니다.
        /// </summary>
        protected abstract bool IsPointerReleasedThisFrame();

        /// <summary>
        /// 포인터가 UI에 의해 막혔는지 검사합니다.
        /// </summary>
        protected abstract bool IsPointerBlockedByUI();

        public virtual void Update()
        {
            if (stageController.IsCleared)
            {
                CancelDrag();
                return;
            }

            UpdatePointerDrag();
            if (inputMode == InputMode.Place)
                CheckPointerPressed();
        }

        private void CheckPointerPressed()
        {
            if (IsPointerBlockedByUI())
                return;
            if (!TryGetPointerTilePos(out Vector2Int pos))
                return;
            if (!IsPointerPressedThisFrame())
                return;

            switch (inputMode)
            {
                case InputMode.Place:
                    {
                        PlaceOrReplaceBlueprint(pos.x, pos.y);
                        break;
                    }
            }
        }

        private void UpdatePointerDrag()
        {
            if (IsPointerBlockedByUI())
                CancelDrag();
            else if (!TryGetPointerTilePos(out Vector2Int pos))
                CancelDrag();
            else if (IsPointerPressedThisFrame())
                BeginDrag(pos);
            else if (IsPointerReleasedThisFrame())
                EndDrag(pos);
            else if (IsPointerPressed() && dragState == DragState.Dragging && prevDragPos != pos)
            {
                ContinueDrag(pos);
                prevDragPos = pos;
            }
        }

        private void BeginDrag(Vector2Int pos)
        {
            if (stageController.IsCleared)
                return;

            dragState = DragState.Dragging;
            dragStartPos = pos;
            prevDragPos = pos;

            switch (inputMode)
            {
                case InputMode.Wire:
                    {
                        BeginWirePath(pos.x, pos.y);
                        break;
                    }
            }
        }

        private void ContinueDrag(Vector2Int pos)
        {
            if (stageController.IsCleared)
                return;

            switch (inputMode)
            {
                case InputMode.Wire:
                    {
                        ContinueWirePath(pos.x, pos.y);
                        break;
                    }
            }
        }

        private void EndDrag(Vector2Int pos)
        {
            if (stageController.IsCleared)
                return;

            if (dragState != DragState.Dragging)
                return;

            switch (inputMode)
            {
                case InputMode.Wire:
                    {
                        EndWirePath();
                        break;
                    }
                case InputMode.Remove:
                    {
                        RemoveEntityRange(dragStartPos, pos);
                        break;
                    }
            }

            dragState = DragState.None;
            prevDragPos = new Vector2Int(-1, -1);
        }

        private void CancelDrag()
        {
            dragState = DragState.None;
            prevDragPos = new Vector2Int(-1, -1);
            path.Clear();
        }

        /// <summary>
        /// 선택된 입력 상태를 변경합니다.
        /// </summary>
        protected void SetInputMode(InputMode inputState)
        {
            if (inputState == inputMode)
                return;

            CancelDrag();
            path.Clear();
            selectedBlueprint = null;
            inputMode = inputState;
            dragState = DragState.None;
        }

        /// <summary>
        /// 청사진을 선택합니다.
        /// </summary>
        protected void SelectBlueprint(EntityBlueprint blueprint)
        {
            if (stageController.IsCleared)
                return;

            SetInputMode(blueprint == null ? InputMode.None : InputMode.Place);
            selectedBlueprint = blueprint;
        }

        protected void PlaceOrReplaceBlueprint(int x, int y)
        {
            if (stageController.IsCleared)
                return;

            if (selectedBlueprint == null)
                return;

            EntityRequestResult result = worldSystem.TryCreateEntity(selectedBlueprint, x, y);
            if (result == EntityRequestResult.AlreadyExist)
                worldSystem.TryReplaceEntity(selectedBlueprint, x, y);
        }

        protected void RemoveEntity(int x, int y)
        {
            if (stageController.IsCleared)
                return;

            worldSystem.TryRemoveEntity(x, y);
        }

        protected void Undo()
        {
            if (stageController.IsCleared)
                return;

            CancelDrag();
            path.Clear();
            dragState = DragState.None;

            worldSystem.TryUndo();
        }

        protected void Redo()
        {
            if (stageController.IsCleared)
                return;

            CancelDrag();
            path.Clear();
            dragState = DragState.None;

            worldSystem.TryRedo();
        }

        private void RemoveEntityRange(Vector2Int start, Vector2Int end)
        {
            if (stageController.IsCleared)
                return;

            worldSystem.TryRemoveEntityRange(start, end);
        }

        private void BeginWirePath(int x, int y)
        {
            if (stageController.IsCleared)
                return;

            if (!worldSystem.TryGetEntityAt(x, y, out Entity entity))
                return;

            if (entity.OutPortCount >= entity.Element.OutSignalCount)
                return;

            path.Clear();
            path.Add(new Vector2Int(x, y));
        }

        private void ContinueWirePath(int x, int y)
        {
            if (stageController.IsCleared)
                return;

            Vector2Int next = new Vector2Int(x, y);
            if (path.Count == 0)
            {
                path.Add(next);
                return;
            }

            Vector2Int last = path[^1];
            if (last == next)
                return;

            int existingIndex = path.IndexOf(next);
            if (existingIndex >= 1)
            {
                path.RemoveRange(existingIndex + 1, path.Count - existingIndex - 1);
                return;
            }

            path.Add(next);
        }

        private void EndWirePath()
        {
            if (stageController.IsCleared)
                return;

            if (path.Count == 1)
                worldSystem.TryReplaceEntity(new EntityBlueprint(CircuitElement.CircuitElementType.Wire), path[0].x, path[0].y);
            if (path.Count >= 3)
                worldSystem.TryCreateWire(path);

            path.Clear();
        }
    }
}