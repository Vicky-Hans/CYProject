using System;
using System.Collections.Generic;
using DH.UIFramework.Paths;
using DH.UIFramework.Proxy.Sources.Object;
using Path = DH.UIFramework.Paths.Path;

namespace DH.UIFramework.Proxy.Sources.Expressions
{
    public class CompiledExpressionSourceDescription : SourceDescription
    {
        private readonly Func<object,object> expression;
        private readonly List<Path> paths;
        private readonly Type returnType;

        public CompiledExpressionSourceDescription(Func<object,object> func,Type type,List<Path> pathsList)
        {
            expression = func;
            returnType = type;
            paths = pathsList;
        }

        public Func<object,object> Expression
        {
            get { return this.expression; }
        }

        public Type ReturnType { get { return this.returnType; } }

        public List<Path> Paths
        {
            get => paths;
        }

        public override string ToString()
        {
            return this.expression == null ? "Expression:null" : "Expression:" + this.expression.ToString();
        }

        public virtual ISourceProxy CreateProxy(ISourceProxyFactory factory,object source)
        {
            List<ISourceProxy> list = new List<ISourceProxy>();
            List<Path> paths = this.Paths;
            foreach (Path path in paths)
            {
                if (!path.IsStatic)
                {
                    if (source == null)
                        continue;//ignore the path

                    MemberNode memberNode = path[0] as MemberNode;
                    if (memberNode != null && memberNode.MemberInfo != null && !memberNode.MemberInfo.DeclaringType.IsAssignableFrom(source.GetType()))
                        continue;//ignore the path
                }

                ISourceProxy innerProxy = factory.CreateProxy(source, new ObjectSourceDescription() { Path = path });
                if (innerProxy != null)
                    list.Add(innerProxy);
            }

            return new ExpressionSourceProxy(source, expression,returnType,list);
        }
    }
    
    public class CompiledExpressionSourceDescription<TSource, TResult> : CompiledExpressionSourceDescription
    {
        private readonly Func<TSource,TResult> expression;

        public CompiledExpressionSourceDescription(Func<TSource,TResult> func,List<Path> pathsList) : base(null,null,pathsList)
        {
            expression = func;
        }

        public new Func<TSource,TResult> Expression
        {
            get { return this.expression; }
        }

        public override string ToString()
        {
            return this.expression == null ? "Expression:null" : "Expression:" + this.expression.ToString();
        }

        public override ISourceProxy CreateProxy(ISourceProxyFactory factory,object source)
        {
            List<ISourceProxy> list = new List<ISourceProxy>();
            List<Path> paths = this.Paths;
            foreach (Path path in paths)
            {
                if (!path.IsStatic)
                {
                    if (source == null)
                        continue;//ignore the path

                    MemberNode memberNode = path[0] as MemberNode;
                    if (memberNode != null && memberNode.MemberInfo != null && !memberNode.MemberInfo.DeclaringType.IsAssignableFrom(source.GetType()))
                        continue;//ignore the path
                }

                ISourceProxy innerProxy = factory.CreateProxy(source, new ObjectSourceDescription() { Path = path });
                if (innerProxy != null)
                    list.Add(innerProxy);
            }

            return new ExpressionSourceProxy<TSource,TResult>((TSource)source, expression,list);
        }
    }
}