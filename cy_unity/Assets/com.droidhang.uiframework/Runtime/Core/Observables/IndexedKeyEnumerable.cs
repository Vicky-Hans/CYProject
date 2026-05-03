using System;
using System.Collections;

namespace DH.UIFramework.Observables
{
    public class IndexedKeyEnumerable : IndexedEnumerable
    {
        private object key;
        private int hashCode;
        private IList groupCollection;

        internal IndexedKeyEnumerable(object key, int hashCode, IList collection) : this(key, hashCode, collection, (Predicate<object>)null, (IComparer)null)
        {
        }

        internal IndexedKeyEnumerable(object key, int hashCode, IList collection, Predicate<object> filterCallback, IComparer comparer):
            base(collection, filterCallback, comparer)
        {
            this.key = key;
            this.hashCode = hashCode;
            this.groupCollection = collection;
        }
        
        public object Key => key;

        public int HashCode => hashCode;

        internal void Add(object element)
        {
            groupCollection.Add(element);
        }
    }
}