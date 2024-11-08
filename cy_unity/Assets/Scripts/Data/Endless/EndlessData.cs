using DH.Proto;
namespace DH.Data
{
    [ProtoWrap(typeof(Endless))]
    public partial class EndlessData : BaseData
    {
        public override void Init()
        {
            base.Init();
        }
        protected override void ClearData()
        {
            base.ClearData();
        }
    }
}