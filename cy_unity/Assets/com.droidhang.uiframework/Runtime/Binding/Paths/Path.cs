

using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using DH.UIFramework.Proxy.Sources;
using DH.UIFramework.Proxy.Sources.Object;
using DH.UIFramework.Reflection;
using UIFramework.Binding;

namespace DH.UIFramework.Paths
{
    [Serializable]
    public class Path : IEnumerator<IPathNode>
    {
        private readonly object _lock = new object();
        private List<IPathNode> nodes = new List<IPathNode>();
        private PathToken token;

        public Path() : this(null)
        {
        }

        public Path(IPathNode root)
        {
            if (root != null)
                this.Prepend(root);
        }

        public IPathNode this[int index]
        {
            get { return this.nodes[index]; }
        }

        public int Count
        {
            get { return this.nodes.Count; }
        }

        public bool IsStatic { get { return nodes.Exists(n => n.IsStatic); } }

        public List<IPathNode> ToList()
        {
            return new List<IPathNode>(nodes);
        }

        public void Append(IPathNode node)
        {
            this.nodes.Add(node);
        }

        public void Prepend(IPathNode node)
        {
            this.nodes.Insert(0, node);
        }

        public void PrependIndexed<T,TValue>(string indexValue) where T : Dictionary<string,TValue>
        {
            this.Prepend(new StringIndexedNode<T,TValue>(indexValue));
        }

        public void PrependIndexed<T,TValue>(int indexValue,bool isDictionary = false) where T : ICollection
        {
            this.Prepend(new IntegerIndexedNode<T,TValue>(indexValue,isDictionary));
        }

        public void AppendIndexed<T,TValue>(string indexValue)where T : Dictionary<string,TValue>
        {
            this.Append(new StringIndexedNode<T,TValue>(indexValue));
        }

        public void AppendIndexed<T,TValue>(int indexValue,bool isDictionary = false) where T : ICollection
        {
            this.Append(new IntegerIndexedNode<T,TValue>(indexValue,isDictionary));
        }

        public PathToken AsPathToken()
        {
            if (this.token != null)
                return this.token;

            lock (_lock)
            {
                if (this.token != null)
                    return this.token;

                if (this.nodes.Count <= 0)
                    throw new InvalidOperationException("The path node is empty");

                this.token = new PathToken(this, 0);
                return this.token;
            }
        }

        public override string ToString()
        {
            StringBuilder buf = new StringBuilder();
            foreach (var node in this.nodes)
            {
                node.AppendTo(buf);
            }
            return buf.ToString();
        }

        #region IEnumerator<IPathNode> Support
        private int index = -1;
        public IPathNode Current
        {
            get { return this.nodes[index]; }
        }

        object IEnumerator.Current
        {
            get { return this.nodes[index]; }
        }

        public bool MoveNext()
        {
            this.index++;
            return this.index >= 0 && index < this.nodes.Count;
        }

        public void Reset()
        {
            this.index = -1;
        }
        #endregion

        #region IDisposable Support
        private bool disposed = false; // 要检测冗余调用

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    this.nodes.Clear();
                    this.index = -1;
                }
                disposed = true;
            }
        }

        ~Path()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }

    public interface IPathNode
    {
        bool IsStatic { get; }

        void AppendTo(StringBuilder output);
    }

    public interface ICompiledNode
    {
        IProxyPropertyInfo CreatePropertyProxy();
    }

    [Serializable]
    public class MemberNode : IPathNode
    {
        private readonly MemberInfo memberInfo;
        private readonly string name;
        private readonly Type type;
        private readonly bool isStatic;
        public MemberNode(string name) : this(null, name, false)
        {
        }

        public MemberNode(Type type, string name, bool isStatic)
        {
            this.name = name;
            this.type = type;
            this.isStatic = isStatic;
        }

        public MemberNode(MemberInfo memberInfo)
        {
            this.memberInfo = memberInfo;
            this.name = memberInfo.Name;
            this.type = memberInfo.DeclaringType;
            this.isStatic = memberInfo.IsStatic();
        }

        public bool IsStatic { get { return this.isStatic; } }

        public Type Type { get { return this.type; } }

        public string Name { get { return this.name; } }

        public MemberInfo MemberInfo { get { return this.memberInfo; } }

        public void AppendTo(StringBuilder output)
        {
            if (output.Length > 0)
                output.Append(".");
            if (IsStatic)
                output.Append(this.type.FullName).Append(".");
            output.Append(this.Name);
        }

        public override string ToString()
        {
            return "MemberNode:" + (this.Name == null ? "null" : this.Name);
        }
    }

    [Serializable]
    public class CompiledMemberNode<TSource,TValue> : IPathNode,ICompiledNode
    {
        private readonly Action<TSource,TValue> setter;
        private readonly Func<TSource,TValue> getter;
        private readonly string name;
        private readonly bool isStatic;

        public CompiledMemberNode(string name,Action<TSource,TValue> setter,Func<TSource,TValue> getter)
        {
            this.name = name;
            this.setter = setter;
            this.getter = getter;
        }

        public bool IsStatic { get { return this.isStatic; } }

        public string Name { get { return this.name; } }

        public IProxyPropertyInfo CreatePropertyProxy()
        {
            return new ProxyPropertyInfo<TSource, TValue>(null,name,getter,setter);
        }

        public void AppendTo(StringBuilder output)
        {
            if (output.Length > 0)
                output.Append(".");
            output.Append(this.Name);
        }

        public override string ToString()
        {
            return "MemberNode:" + (this.Name == null ? "null" : this.Name);
        }
    }
    
    [Serializable]
    public abstract class IndexedNode : IPathNode
    {
        private object _value;
        public IndexedNode(object value)
        {
            this._value = value;
        }

        public bool IsStatic { get { return false; } }

        public object Value
        {
            get { return this._value; }
            private set { this._value = value; }
        }

        public abstract void AppendTo(StringBuilder output);

        public override string ToString()
        {
            return "IndexedNode:" + (this._value == null ? "null" : this._value.ToString());
        }

        public virtual IProxyItemInfo CreateItemInfo()
        {
            return null;
        }

        public virtual ISourceProxy CreateSourceProxy(object source)
        {
           return null;
        } 
    }

    [Serializable]
    public abstract class IndexedNode<T> : IndexedNode, IPathNode
    {
        private T value;
        public IndexedNode(T value) : base(value)
        {
            this.value = value;
        }
        
        public IndexedNode(Func<T> func) : base(null)
        {
            value = func();
        }

        public new T Value => value;
        
        public override void AppendTo(StringBuilder output)
        {
            output.AppendFormat("[\"{0}\"]", this.Value);
        }
    }

    [Serializable]
    public class StringIndexedNode<T,TValue> : IndexedNode<string> where T : Dictionary<string,TValue>
    {
        public StringIndexedNode(string indexValue) : base(indexValue)
        {
        }

        public StringIndexedNode(Func<string> indexValue) : base(indexValue)
        {
        }
        
        public override void AppendTo(StringBuilder output)
        {
            output.AppendFormat("[\"{0}\"]", this.Value);
        }

        public override IProxyItemInfo CreateItemInfo()
        {
           return new DictionaryProxyItemInfo<T,string,TValue>(typeof(T),typeof(TValue));
        }

        public override ISourceProxy CreateSourceProxy(object source)
        {
           return new StringItemNodeProxy((ICollection)source, Value, CreateItemInfo());
        }
    }

    [Serializable]
    public class IntegerIndexedNode<T,TValue> : IndexedNode<int> where T : ICollection
    {
        private bool isDictionary;
        public IntegerIndexedNode(int indexValue,bool isDictionary) : base(indexValue)
        {
            this.isDictionary = isDictionary;
        }

        public IntegerIndexedNode(Func<int> indexValue,bool isDictionary) : base(indexValue)
        {
            this.isDictionary = isDictionary;
        }
        
        public override void AppendTo(StringBuilder output)
        {
            output.AppendFormat("[{0}]", this.Value);
        }
        
        public override IProxyItemInfo CreateItemInfo()
        {
            if (isDictionary)
            {
                return new DictionaryProxyItemInfo<IDictionary<int,TValue>,int,TValue>(typeof(T),typeof(TValue));
            }
            else
            {
                return new ListProxyItemInfo<IList<TValue>,TValue>(typeof(T),typeof(TValue));
            }
        }

        public override ISourceProxy CreateSourceProxy(object source)
        {
            return new IntItemNodeProxy((ICollection)source, Value, CreateItemInfo());
        }
    }
    
    [Serializable]
    public class LongIndexedNode<T,TValue> : IndexedNode<long> where T : ICollection
    {
        private bool isDictionary;
        public LongIndexedNode(long indexValue,bool isDictionary) : base(indexValue)
        {
            this.isDictionary = isDictionary;
        }

        public LongIndexedNode(Func<long> indexValue,bool isDictionary) : base(indexValue)
        {
            this.isDictionary = isDictionary;
        }
        
        public override void AppendTo(StringBuilder output)
        {
            output.AppendFormat("[{0}]", this.Value);
        }
        
        public override IProxyItemInfo CreateItemInfo()
        {
            if (isDictionary)
            {
                return new DictionaryProxyItemInfo<IDictionary<long,TValue>,long,TValue>(typeof(T),typeof(TValue));
            }
            else
            {
                return new ListProxyItemInfo<IList<TValue>,TValue>(typeof(T),typeof(TValue));
            }
        }

        public override ISourceProxy CreateSourceProxy(object source)
        {
            return new LongItemNodeProxy((ICollection)source, Value, CreateItemInfo());
        }
    }
}
