using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DH.Game
{
    public class ProcedureBase
    {
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class ProcedureDeepAttribute : Attribute
    {
        public ProcedureDeepAttribute(int value)
        {
        }
    }

    [ProcedureDeep(1)]
    public class MiningProcedure : ProcedureBase
    {

    }
}
