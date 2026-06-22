using UnityEngine;

namespace Lumencuit
{
    public class ElementViewObject : ViewObject
    {
        [SerializeField] private Transform center;
        [SerializeField] private Transform leftPort;
        [SerializeField] private Transform rightPort;
        [SerializeField] private Transform upPort;
        [SerializeField] private Transform downPort;

        private new Renderer renderer;
        private Renderer leftRenderer;
        private Renderer rightRenderer;
        private Renderer upRenderer;
        private Renderer downRenderer;

        private MaterialPropertyBlock block;

        private void Awake()
        {
            block = new MaterialPropertyBlock();

            renderer = center.GetComponent<Renderer>();
            leftRenderer = leftPort.GetComponent<Renderer>();
            rightRenderer = rightPort.GetComponent<Renderer>();
            upRenderer = upPort.GetComponent<Renderer>();
            downRenderer = downPort.GetComponent<Renderer>();
        }

        public override void SetSignal(QuantumSignal signal)
        {
            renderer.GetPropertyBlock(block);
            block.SetColor("_BaseColor", signal.ToSignal()?.Color ?? Color.gray);
            renderer.SetPropertyBlock(block);
        }

        public override void SetPortSignal(Vector2Int dir, QuantumSignal signal)
        {
            Renderer targetRenderer = null;

            if (dir == Vector2Int.left)
                targetRenderer = leftRenderer;
            else if (dir == Vector2Int.right)
                targetRenderer = rightRenderer;
            else if (dir == Vector2Int.up)
                targetRenderer = upRenderer;
            else if (dir == Vector2Int.down)
                targetRenderer = downRenderer;

            targetRenderer?.GetPropertyBlock(block);
            block.SetColor("_BaseColor", signal.ToSignal()?.Color ?? Color.gray);
            targetRenderer?.SetPropertyBlock(block);
        }

        public override void PortUpdate(Entity entity)
        {
            leftPort.gameObject.SetActive(entity.LeftPort != Entity.PortType.None);
            if (entity.LeftPort == Entity.PortType.Output)
                leftPort.rotation = Quaternion.Euler(0, 0, 90);
            else
                leftPort.rotation = Quaternion.Euler(0, 0, 270);

            rightPort.gameObject.SetActive(entity.RightPort != Entity.PortType.None);
            if (entity.RightPort == Entity.PortType.Output)
                rightPort.rotation = Quaternion.Euler(0, 0, 270);
            else
                rightPort.rotation = Quaternion.Euler(0, 0, 90);

            upPort.gameObject.SetActive(entity.UpPort != Entity.PortType.None);
            if (entity.UpPort == Entity.PortType.Output)
                upPort.rotation = Quaternion.Euler(0, 0, 0);
            else
                upPort.rotation = Quaternion.Euler(0, 0, 180);

            downPort.gameObject.SetActive(entity.DownPort != Entity.PortType.None);
            if (entity.DownPort == Entity.PortType.Output)
                downPort.rotation = Quaternion.Euler(0, 0, 180);
            else
                downPort.rotation = Quaternion.Euler(0, 0, 0);
        }
    }
}