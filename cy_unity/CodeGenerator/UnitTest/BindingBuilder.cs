using System.Linq.Expressions;

namespace DH.UIFramework.Builder
{
    public class BindingBuilder<TTarget, TSource> where TTarget : class
    {
        private void DoSomething()
        {

        }

        public BindingBuilder<TTarget, TSource> To<TResult>(Expression<Func<TSource, TResult>> path)
        {
            DoSomething();
            return this;
        }

        public BindingBuilder<TTarget, TSource> ToExpression<TResult>(Expression<Func<TSource, TResult>> expression)
        {
            return this;
        }

        public BindingBuilder<TTarget, TSource> To<TParameter>(Expression<Func<TSource, Action<TParameter>>> path)
        {
            DoSomething();
            return this;
        }

        public BindingBuilder<TTarget, TSource> To(Expression<Func<TSource, Action>> path)
        {
            DoSomething();
            return this;
        }

        public BindingBuilder<TTarget, TSource> For<TResult>(Expression<Func<TTarget, TResult>> memberExpression)
        {
            DoSomething();
            return this;
        }

        public BindingBuilder<TTarget, TSource> CommandParameter<TParam>(Func<TParam> parameter)
        {
            return this;
        }

        public BindingBuilder<TTarget, TSource> Bind(TTarget target)
        {
            return this;
        }
    }
}
