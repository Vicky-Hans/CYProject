using System.Collections.Generic;
using FindingPath.Data;
#if USE_FP
    using DH.LockStep.Framework;
#else
    using FP = System.Single;
    using TSVector2 = UnityEngine.Vector2;
    using TSVector = UnityEngine.Vector3;
    using TSRect = UnityEngine.Rect;
    using TSMatrix4x4 = UnityEngine.Matrix4x4;
#endif

namespace FindingPath.Path
{
    public partial class AStarPathManager
    {
        private struct MapCellInfoTemp
        {
            public TSVector centerPosition;
            public NavMesh navMeshData;

            public MapCellInfoTemp(TSVector centerPosition, NavMesh navMeshData)
            {
                this.centerPosition = centerPosition;
                this.navMeshData = navMeshData;
            }

            public TSRect Rect
            {
                get
                {
                    var halfWidth = 256f / 2;
                    var halfHeight = halfWidth;
                    var minPosition = centerPosition - new TSVector(halfWidth, 0, halfWidth);
                    return new TSRect(minPosition.x, minPosition.z, 256f, 256f);
                }
            }
            
        }

        private readonly Dictionary<string, MapCellInfoTemp> mapCellNavMeshDic =
            new Dictionary<string, MapCellInfoTemp>();
        //
        // public void AddMapCell(string nameKey, MapCellPathDataRef navMeshDataRef)
        // {
        //     if (mapCellNavMeshDic.ContainsKey(nameKey))
        //     {
        //         return;
        //     }
        //
        //     MapCellInfoTemp mapCellInfoTemp =
        //         new MapCellInfoTemp(navMeshDataRef.Entity.Transform.position, navMeshDataRef.navMeshData);
        //     
        //     mapCellNavMeshDic.Add(nameKey, mapCellInfoTemp);
        // }
        //
        // public void RemoveMapCell(string nameKey)
        // {
        //     mapCellNavMeshDic.Remove(nameKey);
        // }
        //
        // private NavMesh FindNavMeshTile(TSVector position)
        // {
        //     NavMesh navMeshData = null;
        //     foreach (var item in mapCellNavMeshDic.Values)
        //     {
        //         if (item.Rect.Contains(new TSVector2(position.x, position.y)))
        //         {
        //             navMeshData = item.navMeshData;
        //         }
        //     }
        //
        //     return navMeshData;
        // }

    }
}