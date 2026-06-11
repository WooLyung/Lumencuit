namespace Lumencuit
{
    public sealed class GridUpdatedEvent
    {
        public readonly WorldGrid WorldGrid;

        public GridUpdatedEvent(WorldGrid worldGrid)
        {
            WorldGrid = worldGrid;
        }
    }
}
