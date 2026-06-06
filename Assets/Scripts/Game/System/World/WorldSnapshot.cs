using System.Collections.Generic;

namespace Lumencuit
{
    /// <summary>
    /// undo, redo 구현을 위한 월드 스냅샷입니다.
    /// </summary>
    public sealed class WorldSnapshot
    {
        public readonly WorldGrid WorldGrid;
        public readonly List<EntityBlueprintStack> Blueprints;

        public WorldSnapshot(WorldGrid worldGrid, List<EntityBlueprintStack> blueprints)
        {
            WorldGrid = worldGrid.Clone();
            Blueprints = new();
            foreach (var blueprint in blueprints)
                Blueprints.Add(blueprint.Clone());
        }
    }
}
