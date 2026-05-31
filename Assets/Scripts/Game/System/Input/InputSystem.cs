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
        public enum InputState
        {
            Drag, None
        }
        
        protected WorldSystem worldSystem;
        private EntityBlueprint selectedBlueprint = null;
        private InputState currInputState = InputState.None;
        private List<Vector2Int> path = new();

        public InputState CurrInputState => currInputState;

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
            worldSystem.TryCreateEntity(selectedBlueprint, x, y);
            selectedBlueprint = null;
        }

        protected void StartPath(int x, int y)
        {
            if (currInputState != InputState.None)
                return;

            Entity entity = worldSystem.GetEntityAt(x, y);
            if (entity == null)
                return;
            if (entity.OutPortCount >= entity.Element.OutSignalCount)
                return;

            currInputState = InputState.Drag;
            path.Clear();
            path.Add(new Vector2Int(x, y));
        }

        protected void NextPath(int x, int y)
        {
            if (currInputState != InputState.Drag)
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
            if (existingIndex >= 0)
            {
                path.RemoveRange(existingIndex + 1, path.Count - existingIndex - 1);
                return;
            }

            path.Add(next);
        }

        protected void EndPath()
        {
            if (currInputState != InputState.Drag)
            {
                currInputState = InputState.None;
                return;
            }

            if (path.Count >= 2)
                worldSystem.TryCreateWire(path);

            path.Clear();
            currInputState = InputState.None;
        }
    }
}