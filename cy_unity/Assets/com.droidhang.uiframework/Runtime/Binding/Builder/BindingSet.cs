

using System;
using System.Collections.Generic;
using DH.UIFramework.Contexts;
using DHFramework;
using UnityEngine;
#if UNITY_2019_1_OR_NEWER
using UnityEngine.UIElements;
#endif
namespace DH.UIFramework.Builder
{
    public abstract class BindingSetBase : IBindingBuilder
    {
        internal BaseView baseView;
        
        protected IBindingContext context;
        protected readonly List<IBindingBuilder> builders = new List<IBindingBuilder>();

        public BindingSetBase(IBindingContext context)
        {
            this.context = context;
        }

        public void Build()
        {
            // 若使用了代码生成，则调用代码生成，否则不做额外处理
            if (baseView)
            {
                baseView.InitializeBinding();
                baseView.bindingSetBase = null;
            }

            foreach (var builder in this.builders)
            {
                try
                {
                    builder.Build();
                }
                catch (Exception e)
                {
                    DHLog.Error("{0}", e);
                }
            }
            
            this.builders.Clear();
        }

        public BindingBuilderBase this[int index] => builders[index] as BindingBuilderBase;
    }

    public class BindingSet<TTarget, TSource> : BindingSetBase where TTarget : class
    {
        private TTarget target;
        public BindingSet(IBindingContext context, TTarget target) : base(context)
        {
            this.target = target;
        }

        public virtual BindingBuilder<TTarget, TSource> Bind()
        {
            var builder = new BindingBuilder<TTarget, TSource>(this.context, this.target);
            this.builders.Add(builder);
            return builder;
        }

        public virtual BindingBuilder<TChildTarget, TSource> Bind<TChildTarget>(TChildTarget target) where TChildTarget : class
        {
            var builder = BindingBuilder<TChildTarget, TSource>.Default;
            return builder;
        }
        
        /// <summary>
        /// 用于自动生成的代码调用，请勿手动调用
        /// </summary>
        /// <param name="target"></param>
        /// <typeparam name="TChildTarget"></typeparam>
        /// <returns></returns>
        public virtual BindingBuilder<TChildTarget, TSource> CompileCreate<TChildTarget>(TChildTarget target) where TChildTarget : class
        {
            var builder = new BindingBuilder<TChildTarget, TSource>(context, target);
            this.builders.Add(builder);
            return builder;
        }
    }

    public class BindingSet<TTarget> : BindingSetBase where TTarget : class
    {
        private TTarget target;
        public BindingSet(IBindingContext context, TTarget target) : base(context)
        {
            this.target = target;
        }

        public virtual BindingBuilder<TTarget> Bind()
        {
            var builder = new BindingBuilder<TTarget>(this.context, this.target);
            this.builders.Add(builder);
            return builder;
        }

        public virtual BindingBuilder<TChildTarget> Bind<TChildTarget>(TChildTarget target) where TChildTarget : class
        {
            var builder = new BindingBuilder<TChildTarget>(context, target);
            this.builders.Add(builder);
            return builder;
        }
    }

    public class BindingSet : BindingSetBase
    {
        private object target;
        public BindingSet(IBindingContext context, object target) : base(context)
        {
            this.target = target;
        }

        public virtual BindingBuilder Bind()
        {
            var builder = new BindingBuilder(this.context, this.target);
            this.builders.Add(builder);
            return builder;
        }

        public virtual BindingBuilder Bind(object target)
        {
            var builder = new BindingBuilder(context, target);
            this.builders.Add(builder);
            return builder;
        }
    }
}
