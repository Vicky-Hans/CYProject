using UnityEngine;
using UnityEngine.UI;

namespace DH.UIFramework
{
    [RequireComponent(typeof(CanvasRenderer))]
    public class NonDrawingGraphic : MaskableGraphic
    {
        public override void SetMaterialDirty()
        {
            return;
        }

        public override void SetVerticesDirty()
        {
            return;
        }

        protected NonDrawingGraphic()
        {
            useLegacyMeshGeneration = false;
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            return;
        }
    }
}