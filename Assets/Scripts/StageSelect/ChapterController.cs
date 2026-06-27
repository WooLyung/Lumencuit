using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static Lumencuit.ChapterData;

namespace Lumencuit
{
    /// <summary>
    /// 챕터 선택 씬을 제어합니다.
    /// </summary>
    public class ChapterController : MonoBehaviour
    {
        [SerializeField] private ChapterData chapterData;
        [SerializeField] private ViewRoot viewRoot;
        [SerializeField] private Mesh tileMesh;
        [SerializeField] private StageContext stageContext;

        // 프리팹
        [SerializeField] private GameObject gridPrefab;
        [SerializeField] private GameObject stageNormal;
        [SerializeField] private GameObject stageHard;
        [SerializeField] private GameObject wire;

        // 색상
        private readonly QuantumSignal yellow = QuantumSignal.FromSignal(Signal.Yellow);
        private readonly QuantumSignal green = QuantumSignal.FromSignal(Signal.Green);
        private readonly QuantumSignal red = QuantumSignal.FromSignal(Signal.Red);

        private ViewObject[, ] views;
        private bool[,] accessible;

        private void Start()
        {
            RenderGrid();
            RenderEntities();
            AddColors();
        }

        private void Update()
        {
            Mouse mouse = Mouse.current;
            if (mouse == null)
                return;

            if (mouse.leftButton.wasPressedThisFrame)
            {
                if (TryGetPointerTilePos(out Vector2Int pos) && accessible[pos.x, pos.y])
                {
                    ChapterStageInfo info = chapterData.GetStageInfo(chapterData.GetStageNumber(pos.x, pos.y));
                    if (info == null || info.StageData == null)
                        return;
                    stageContext.StartStage(info.StageData);
                }
            }
        }

        private bool TryGetPointerTilePos(out Vector2Int pos)
        {
            pos = default;

            Mouse mouse = Mouse.current;
            if (mouse == null)
                return false;

            Vector2 mousePosition = mouse.position.ReadValue();
            Ray ray = Camera.main.ScreenPointToRay(mousePosition);

            if (!Physics.Raycast(ray, out RaycastHit hit))
                return false;

            if (!hit.collider.TryGetComponent(out GridTilePos gridTilePos))
                return false;

            pos = gridTilePos.Pos;
            return true;
        }

        private void AddColors()
        {
            int[,] remainedIn = new int[chapterData.Width, chapterData.Height];
            Queue<Vector2Int> q = new();

            for (int x = 0; x < chapterData.Width; x++)
            {
                for (int y = 0; y < chapterData.Height; y++)
                {
                    remainedIn[x, y] = 0;

                    if (!chapterData.HasInput(x, y))
                        q.Enqueue(new Vector2Int(x, y));
                    else if (chapterData.IsEnabledTile(x, y))
                    {
                        if (chapterData.HasInputDirection(x, y, DirectionFlags.Up))
                            remainedIn[x, y]++;
                        if (chapterData.HasInputDirection(x, y, DirectionFlags.Down))
                            remainedIn[x, y]++;
                        if (chapterData.HasInputDirection(x, y, DirectionFlags.Left))
                            remainedIn[x, y]++;
                        if (chapterData.HasInputDirection(x, y, DirectionFlags.Right))
                            remainedIn[x, y]++;
                    }
                }
            }

            while (q.Count > 0)
            {
                Vector2Int front = q.Dequeue();
                accessible[front.x, front.y] = true;
                ViewObject viewObject = views[front.x, front.y];
                if (viewObject == null)
                    continue;

                ChapterStageInfo info = chapterData.GetStageInfo(chapterData.GetStageNumber(front.x, front.y));
                if (info != null && info.StageData != null && !SaveManagement.GlobalData.IsStageCleared(info.StageData.StageId))
                {
                    viewObject.SetSignal(yellow);
                    if (viewObject is ElementLampViewObject v2)
                        v2.SetLampSignal(red);
                    continue;
                }
                else
                {
                    viewObject.SetSignal(green);
                    if (viewObject is ElementLampViewObject v2)
                        v2.SetLampSignal(red);
                }

                if (chapterData.HasInputDirection(front.x, front.y - 1, DirectionFlags.Up))
                {
                    viewObject.SetPortSignal(Vector2Int.down, green);
                    views[front.x, front.y - 1].SetPortSignal(Vector2Int.up, green);
                    if (--remainedIn[front.x, front.y - 1] == 0)
                        q.Enqueue(new Vector2Int(front.x, front.y - 1));
                }
                if (chapterData.HasInputDirection(front.x, front.y + 1, DirectionFlags.Down))
                {
                    viewObject.SetPortSignal(Vector2Int.up, green);
                    views[front.x, front.y + 1].SetPortSignal(Vector2Int.down, green);
                    if (--remainedIn[front.x, front.y + 1] == 0)
                        q.Enqueue(new Vector2Int(front.x, front.y + 1));
                }
                if (chapterData.HasInputDirection(front.x - 1, front.y, DirectionFlags.Right))
                {
                    viewObject.SetPortSignal(Vector2Int.left, green);
                    views[front.x - 1, front.y].SetPortSignal(Vector2Int.right, green);
                    if (--remainedIn[front.x - 1, front.y] == 0)
                        q.Enqueue(new Vector2Int(front.x - 1, front.y));
                }
                if (chapterData.HasInputDirection(front.x + 1, front.y, DirectionFlags.Left))
                {
                    viewObject.SetPortSignal(Vector2Int.right, green);
                    views[front.x + 1, front.y].SetPortSignal(Vector2Int.left, green);
                    if (--remainedIn[front.x + 1, front.y] == 0)
                        q.Enqueue(new Vector2Int(front.x + 1, front.y));
                }
            }
        }
        
        private void RenderGrid()
        {
            RenderGridMesh();
            RenderGridCollider();

            viewRoot.transform.localPosition = new Vector3(-(chapterData.Width - 1) * 0.5f, -(chapterData.Height - 1) * 0.5f, 0);
        }

        private Entity.Ports GetPorts(int x, int y)
        {
            Entity.PortType left = Entity.PortType.None;
            Entity.PortType right = Entity.PortType.None;
            Entity.PortType up = Entity.PortType.None;
            Entity.PortType down = Entity.PortType.None;

            if (chapterData.HasInputDirection(x, y, DirectionFlags.Left))
                left = Entity.PortType.Input;
            else if (chapterData.HasInputDirection(x - 1, y, DirectionFlags.Right))
                left = Entity.PortType.Output;

            if (chapterData.HasInputDirection(x, y, DirectionFlags.Right))
                right = Entity.PortType.Input;
            else if (chapterData.HasInputDirection(x + 1, y, DirectionFlags.Left))
                right = Entity.PortType.Output;

            if (chapterData.HasInputDirection(x, y, DirectionFlags.Up))
                up = Entity.PortType.Input;
            else if (chapterData.HasInputDirection(x, y + 1, DirectionFlags.Down))
                up = Entity.PortType.Output;

            if (chapterData.HasInputDirection(x, y, DirectionFlags.Down))
                down = Entity.PortType.Input;
            else if (chapterData.HasInputDirection(x, y - 1, DirectionFlags.Up))
                down = Entity.PortType.Output;

            return new Entity.Ports(left, right, up, down);
        }

        private void RenderEntities()
        {
            views = new ViewObject[chapterData.Width, chapterData.Height];
            accessible = new bool[chapterData.Width, chapterData.Height];

            for (int x = 0; x < chapterData.Width; x++)
            {
                for (int y = 0; y < chapterData.Height; y++)
                {
                    if (!chapterData.IsEnabledTile(x, y))
                        continue;

                    int stageNumber = chapterData.GetStageNumber(x, y);
                    if (stageNumber >= 1)
                    {
                        ChapterStageInfo info = chapterData.GetStageInfo(stageNumber);
                        GameObject view;

                        if (info.IsHard)
                            view = Instantiate(stageHard, viewRoot.Entities);
                        else
                            view = Instantiate(stageNormal, viewRoot.Entities);
                        view.transform.localPosition = new Vector3(x, y, 0);
                        view.name = $"Stage[{stageNumber}]";

                        ViewObject viewObject = view.GetComponent<ViewObject>();
                        if (viewObject is ElementLampViewObject v2)
                            v2.SetLampSignal(red);
                        viewObject.PortUpdate(GetPorts(x, y));
                        views[x, y] = viewObject;
                    }
                    else if (chapterData.HasInput(x, y))
                    {
                        GameObject view = Instantiate(wire, viewRoot.Entities);
                        view.transform.localPosition = new Vector3(x, y, 0);
                        view.name = $"Wire[{x}][{y}]";

                        ViewObject viewObject = view.GetComponent<ViewObject>();
                        viewObject.PortUpdate(GetPorts(x, y));
                        views[x, y] = viewObject;
                    }
                }
            }
        }

        private void RenderGridMesh()
        {
            List<CombineInstance> combines = new();

            for (int x = 0; x < chapterData.Width; x++)
                for (int y = 0; y < chapterData.Height; y++)
                    if (chapterData.IsEnabledTile(x, y))
                        combines.Add(new CombineInstance { mesh = tileMesh, transform = Matrix4x4.TRS(new Vector3(x, y, 0), Quaternion.identity, Vector3.one) });

            Mesh combinedMesh = new Mesh { name = "GridMesh", indexFormat = UnityEngine.Rendering.IndexFormat.UInt32 };
            combinedMesh.CombineMeshes(combines.ToArray(), mergeSubMeshes: true, useMatrices: true);
            combinedMesh.RecalculateBounds();

            viewRoot.GridMesh.GetComponent<MeshFilter>().sharedMesh = combinedMesh;
        }

        private void RenderGridCollider()
        {
            for (int x = 0; x < chapterData.Width; x++)
            {
                for (int y = 0; y < chapterData.Height; y++)
                {
                    if (!chapterData.IsEnabledTile(x, y))
                        continue;

                    GameObject gridCollider = Instantiate(gridPrefab, viewRoot.GridColliders);
                    gridCollider.transform.position = new Vector3(x, y, 0);
                    gridCollider.name = $"GridCollider[{x}][{y}]";
                    gridCollider.GetComponent<GridTilePos>().Pos = new Vector2Int(x, y);
                }
            }
        }
    }
}