using UnityEngine;

namespace Lumencuit
{
    public class WireViewObject : ViewObject
    {
        [SerializeField] private SpriteRenderer center;

        [SerializeField] private SpriteRenderer leftPort;
        [SerializeField] private SpriteRenderer rightPort;
        [SerializeField] private SpriteRenderer upPort;
        [SerializeField] private SpriteRenderer downPort;

        public override void SetSignal(Signal signal)
        {
            center.color = signal.Color;
        }

        public override void SetPortSignal(Vector2Int dir, Signal signal)
        {
            if (dir == Vector2Int.left)
                leftPort.color = signal.Color;
            else if (dir == Vector2Int.right)
                rightPort.color = signal.Color;
            else if (dir == Vector2Int.up)
                upPort.color = signal.Color;
            else if (dir == Vector2Int.down)
                downPort.color = signal.Color;
        }

        public override void PortUpdate(Entity entity)
        {
            leftPort.gameObject.SetActive(entity.LeftPort != Entity.PortType.None);
            rightPort.gameObject.SetActive(entity.RightPort != Entity.PortType.None);
            upPort.gameObject.SetActive(entity.UpPort != Entity.PortType.None);
            downPort.gameObject.SetActive(entity.DownPort != Entity.PortType.None);
        }
    }
}