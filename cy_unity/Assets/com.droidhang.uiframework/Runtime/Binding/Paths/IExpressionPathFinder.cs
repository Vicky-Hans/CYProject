

using System.Collections.Generic;
using System.Linq.Expressions;

namespace DH.UIFramework.Paths
{
    public interface IExpressionPathFinder
    {
        List<Path> FindPaths(LambdaExpression expression);
        
    }
}
