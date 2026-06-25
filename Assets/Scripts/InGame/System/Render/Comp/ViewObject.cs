using System.Collections;
using UnityEngine;

namespace Lumencuit
{
    /// <summary>
    /// 실제 오브젝트의 렌더를 제어합니다.
    /// </summary>
    public abstract class ViewObject : MonoBehaviour
    {
        /// <summary>
        /// 뷰 오브젝트의 각 파트 정보를 저장합니다.
        /// </summary>
        protected class ViewPart
        {
            public Transform Transform;
            public Renderer Renderer;
            public bool IsActive = false;

            public ViewPart(Transform transform)
            {
                Transform = transform;
                Renderer = transform.GetComponent<Renderer>();
            }
        }

        protected static readonly int ProgressID = Shader.PropertyToID("_Progress");
        protected static readonly float AnimDuration = 0.2f;
        private MaterialPropertyBlock block = null;
        private MaterialPropertyBlock Block => (block == null) ? block = new MaterialPropertyBlock() : block;

        public abstract void SetSignal(QuantumSignal signal);
        public abstract void SetPortSignal(Vector2Int dir, QuantumSignal signal);
        public abstract void PortUpdate(Entity entity);

        public void SetPortSignal(QuantumSignal signal)
        {
            SetPortSignal(Vector2Int.left, signal);
            SetPortSignal(Vector2Int.right, signal);
            SetPortSignal(Vector2Int.up, signal);
            SetPortSignal(Vector2Int.down, signal);
        }

        public abstract void Destroy();

        protected void PortUpdate(ViewPart part, Entity.PortType portType, Quaternion output, Quaternion input)
        {
            if (portType == Entity.PortType.Output)
                part.Transform.rotation = output;
            else
                part.Transform.rotation = input;

            if (portType != Entity.PortType.None)
                Appear(part);
            else
                Disappear(part);
        }

        protected void Appear(ViewPart part)
        {
            if (part.IsActive)
                return;
            part.IsActive = true;
            StartCoroutine(Anim(0f, 1f, part.Renderer));
        }

        protected void Disappear(ViewPart part, bool destroy = false)
        {
            if (!part.IsActive)
                return;
            part.IsActive = false;
            StartCoroutine(Anim(1f, 0f, part.Renderer, destroy));
        }

        protected void ApplySignal(QuantumSignal signal, ViewPart part)
        {
            part.Renderer.GetPropertyBlock(Block);
            Block.SetInt("_Signal", signal.Mask);
            part.Renderer.SetPropertyBlock(Block);
        }

        protected IEnumerator Anim(float from, float to, Renderer renderer, bool destroy = false)
        {
            float elapsed = 0f;

            while (elapsed < AnimDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / AnimDuration);

                float value = Mathf.Lerp(from, to, t);

                renderer.GetPropertyBlock(Block);
                Block.SetFloat(ProgressID, value);
                renderer.SetPropertyBlock(Block);

                yield return null;
            }

            renderer.GetPropertyBlock(Block);
            Block.SetFloat(ProgressID, to);
            renderer.SetPropertyBlock(Block);

            if (destroy)
                Destroy(gameObject);
        }
    }
}