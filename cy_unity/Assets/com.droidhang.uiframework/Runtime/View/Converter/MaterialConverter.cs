using System;
using System.Collections;
using System.Collections.Generic;
using DH.UIFramework.Converters;
using DH.UIFramework.Proxy;
using UnityEngine;

namespace DH.UIFramework
{
    public class MaterialConverter : AbstractConverter<string, Material>
    {
        private BaseView ownerView;
        
        public MaterialConverter(BaseView ownerView)
        {
            this.ownerView = ownerView;
        }
        
        public override void Convert(object value, IModifiable modifiable)
        {
            if (value == null)
            {
                modifiable?.SetValue(null);
            }
            else
            {
                Convert((string)value, modifiable);
            }
        }

        public override void Convert(string value,IModifiable modifiable)
        {
            ownerView.ConvertMaterial(value, modifiable);
        }

        public override string ConvertBack(Material value)
        {
            throw new NotImplementedException();
        }
    }
}
