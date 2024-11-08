using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using RVO;

#if USE_FP
    using DH.LockStep.Framework;
#else
using FP = System.Single;
using TSVector2 = UnityEngine.Vector2;
using TSVector = UnityEngine.Vector3;
using TSRect = UnityEngine.Rect;
using TSMatrix4x4 = UnityEngine.Matrix4x4;
#endif

namespace FindingPath.Data
{
    [Serializable]
    [JsonObject(MemberSerialization.OptIn)]
    public sealed class RvoObstacle
    {
        [JsonProperty]
        public List<Vector2> vertices;
    }
}