using UnityEngine;

namespace Lumencuit
{
    /// <summary>
    /// 스테이지의 모든 정보를 포함하는 데이터 클래스입니다.
    /// </summary>
    [CreateAssetMenu(fileName = "StageData", menuName = "Lumencuit/Stage Data")]
    public sealed class StageData : ScriptableObject
    {
        public string StageName;
        [Min(1)] public int Width = 5;
        [Min(1)] public int Height = 5;
        [SerializeField] private bool[,] enabledTiles = new bool[1, 1];

        public bool IsEnabledTile(int x, int y)
        {
            if (x < 0 || x >= Width || y < 0 || y >= Height)
                return false;
            return enabledTiles[x, y];
        }

        public void SetEnabledTile(int x, int y, bool value)
        {
            enabledTiles[x, y] = value;
        }

        public void ResizeTiles()
        {
            int size = Width * Height;
            if (enabledTiles == null || enabledTiles.Length != Width * Height)
                enabledTiles = new bool[Width, Height];
        }
    }
}