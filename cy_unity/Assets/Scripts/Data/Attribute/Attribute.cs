using DH.Config;
using DH.UIFramework.Observables;

namespace DH.Data
{
    public class Attribute : ObservableObject
    {
        private float valueBase;
        private AttributeType attributeType;
        
        private decimal dValueBase;
        
        public float ValueBase
        {
            get => valueBase;
            set
            {
                if (!Set(ref valueBase, value))
                {
                    return;
                }
                
                RaisePropertyChanged(nameof(Value));
            }
        }
        public decimal DValueBase
        {
            get => dValueBase;
            set
            {
                if (!Set(ref dValueBase, value))
                {
                    return;
                }
                
                RaisePropertyChanged(nameof(Value));
            }
        }
        
        public float Value => CalculateAttr();

        public AttributeType AttrType => attributeType;

        private float CalculateAttr()
        {
            return valueBase;
        }

        public Attribute(AttributeType type)
        {
            attributeType = type;
        }

        public void Modify(AttributeType id, AttributeValueType vType, long val)
        {
            switch (vType)
            {
                case AttributeValueType.ValBase:
                    valueBase += val;
                    dValueBase += val;
                    break;
                
                case AttributeValueType.ValMul:
                    valueBase += val / 10000f;
                    dValueBase += val/10000m;
                    break;
                
                default:
                    return;
            }
        }

    }
}