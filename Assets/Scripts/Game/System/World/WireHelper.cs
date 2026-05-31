using UnityEngine;

namespace Lumencuit
{
    public static class WireHelper
    {
        public static bool IsWire(Entity entity)
        {
            return entity != null && entity.Element.Type == CircuitElement.CircuitElementType.Wire;
        }
    
        public static Vector2Int? GetWireInDir(Entity wire)
        {
            if (wire == null)
                return null;
            if (wire.LeftPort == Entity.PortType.Input)
                return Vector2Int.left;
            if (wire.RightPort == Entity.PortType.Input)
                return Vector2Int.right;
            if (wire.UpPort == Entity.PortType.Input)
                return Vector2Int.up;
            if (wire.DownPort == Entity.PortType.Input)
                return Vector2Int.down;
            return null;
        }

        public static Vector2Int? GetWireOutDir(Entity wire)
        {
            if (wire == null)
                return null;
            if (wire.LeftPort == Entity.PortType.Output)
                return Vector2Int.left;
            if (wire.RightPort == Entity.PortType.Output)
                return Vector2Int.right;
            if (wire.UpPort == Entity.PortType.Output)
                return Vector2Int.up;
            if (wire.DownPort == Entity.PortType.Output)
                return Vector2Int.down;
            return null;
        }

        public static Vector2Int? GetWireIn(Entity wire, Vector2Int pos)
        {
            Vector2Int? dir = GetWireInDir(wire);
            if (dir == null)
                return null;
            return pos + dir;
        }

        public static Vector2Int? GetWireOut(Entity wire, Vector2Int pos)
        {
            Vector2Int? dir = GetWireOutDir(wire);
            if (dir == null)
                return null;
            return pos + dir;
        }
    }
}