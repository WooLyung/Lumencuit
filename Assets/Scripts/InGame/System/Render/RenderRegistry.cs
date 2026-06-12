using System.ComponentModel;
using UnityEngine;

namespace Lumencuit
{
    /// <summary>
    /// 렌더 프리팹를 가지는 컴포넌트입니다.
    /// </summary>
    public sealed class RenderRegistry : MonoBehaviour
    {
        [SerializeField] private RenderPrefab prefabs;

        public RenderPrefab Prefabs => prefabs;

        // [임시] 양자 신호 렌더링용
        private float time = 0;
        public static int N = 0;

        private void Update()
        {
            time += Time.deltaTime;
            if (time > 0.5f)
            {
                time = 0;
                N++;
            }
        }
    }
}
