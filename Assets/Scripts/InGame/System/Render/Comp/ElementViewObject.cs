using UnityEngine;

namespace Lumencuit
{
    /// <summary>
    /// 작은 램프가 없고 와이어가 아닌 뷰 오브젝트입니다.
    /// </summary>
    public class ElementViewObject : ViewObject
    {
        [SerializeField] private Transform centerTransform;
        [SerializeField] private Transform leftTransform;
        [SerializeField] private Transform rightTransform;
        [SerializeField] private Transform upTransform;
        [SerializeField] private Transform downTransform;

        private ViewPart center, left, right, up, down;

        private void Awake()
        {
            center = new ViewPart(centerTransform);
            left = new ViewPart(leftTransform);
            right = new ViewPart(rightTransform);
            up = new ViewPart(upTransform);
            down = new ViewPart(downTransform);
        }

        private void Start()
        {
            Appear(center);
        }

        public override void SetSignal(QuantumSignal signal)
        {
            ApplySignal(signal, center);
        }

        public override void SetPortSignal(Vector2Int dir, QuantumSignal signal)
        {
            if (dir == Vector2Int.left)
                ApplySignal(signal, left);
            else if (dir == Vector2Int.right)
                ApplySignal(signal, right);
            else if (dir == Vector2Int.up)
                ApplySignal(signal, up);
            else if (dir == Vector2Int.down)
                ApplySignal(signal, down);
        }

        public override void PortUpdate(Entity entity)
        {
            PortUpdate(left, entity.LeftPort, Quaternion.Euler(0, 0, 90), Quaternion.Euler(0, 0, 270));
            PortUpdate(right, entity.RightPort, Quaternion.Euler(0, 0, 270), Quaternion.Euler(0, 0, 90));
            PortUpdate(up, entity.UpPort, Quaternion.Euler(0, 0, 0), Quaternion.Euler(0, 0, 180));
            PortUpdate(down, entity.DownPort, Quaternion.Euler(0, 0, 180), Quaternion.Euler(0, 0, 0));
        }

        public override void Destroy()
        {
            StopAllCoroutines();
            Disappear(left);
            Disappear(right);
            Disappear(up);
            Disappear(down);
            Disappear(center, true);
        }
    }
}