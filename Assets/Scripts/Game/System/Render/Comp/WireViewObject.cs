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

        public override void SetColor(Color color)
        {
            center.color = color;
        }

        public override void SetPortColor(Vector2Int dir, Color color)
        {
            if (dir == Vector2Int.left)
                leftPort.color = color;
            else if (dir == Vector2Int.right)
                rightPort.color = color;
            else if (dir == Vector2Int.up)
                upPort.color = color;
            else if (dir == Vector2Int.down)
                downPort.color = color;
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