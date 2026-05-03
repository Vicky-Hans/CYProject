using System;
using System.Collections.Generic;
using DHFramework;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace DH.UIFramework
{
    public class ClipperMaterialManager : Singleton<ClipperMaterialManager>
    {
        public class MaterialKeyCompare : IEqualityComparer<MaterialKey>
        {
            public static readonly MaterialKeyCompare Default = new MaterialKeyCompare();
            
            public bool Equals(MaterialKey x, MaterialKey y)
            {
                return Equals(x.originalMaterial, y.originalMaterial) && Equals(x.clipper, y.clipper);
            }

            public int GetHashCode(MaterialKey obj)
            {
                return HashCode.Combine(obj.originalMaterial, obj.clipper);
            }
        }
        
        public struct MaterialKey
        {
            public Material originalMaterial;
            public IClipper clipper;
        }
        
        public class MaterialData
        {
            public Material newMaterial;
            public Material originalMat;
            public IClipper clipper;
            public int referenceCount;
        }

        private readonly Dictionary<MaterialKey, MaterialData> materialDatas = new Dictionary<MaterialKey, MaterialData>(MaterialKeyCompare.Default);
        private readonly Dictionary<Material, MaterialData> materialKeysMap = new Dictionary<Material, MaterialData>();

        public Material GetMaterial(Material original, IClipper parent)
        {
            var key = new MaterialKey()
            {
                originalMaterial = original,
                clipper = parent
            };

            if (materialDatas.TryGetValue(key, out var value))
            {
                value.referenceCount++;
                return value.newMaterial;
            }
            else
            {
                value = new MaterialData()
                {
                    newMaterial = Object.Instantiate(original),
                    referenceCount = 1,
                    originalMat = original,
                    clipper = parent,
                };
                materialKeysMap.Add(value.newMaterial,value);
                materialDatas.Add(key,value);
                return value.newMaterial;
            }
        }

        public Material ReleaseMaterial(Material newMaterial)
        {
            if (!materialKeysMap.TryGetValue(newMaterial, out var data))
            {
                return null;
            }

            data.referenceCount--;
            if (data.referenceCount <= 0)
            {
                Object.Destroy(data.newMaterial);
            }

            materialDatas.Remove(new MaterialKey() { clipper = data.clipper, originalMaterial = data.originalMat });
            return data.originalMat;
        }
    }
}