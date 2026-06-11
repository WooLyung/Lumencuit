using System;

namespace Lumencuit
{
    public static class GameEventBus
    {
        public static event Action<GridUpdatedEvent> GridChanged;
        public static event Action<StageClearedEvent> StageCleared;

        public static void NotifyGridChanged(GridUpdatedEvent e)
        {
            GridChanged?.Invoke(e);
        }

        public static void NotifyStageCleared(StageClearedEvent e)
        {
            StageCleared?.Invoke(e);
        }

        public static void Clear()
        {
            GridChanged = null;
            StageCleared = null;
        }
    }
}
