

using System;
using DH.UIFramework.Paths;

namespace DH.UIFramework.Proxy.Sources.Object
{
    [Serializable]
    public class ObjectSourceDescription : SourceDescription
    {
        private Path path;

        public ObjectSourceDescription()
        {
            this.IsStatic = false;
        }

        public virtual Path Path
        {
            get { return this.path; }
            set
            {
                this.path = value;
                if (this.path != null)
                    this.IsStatic = this.path.IsStatic;
            }
        }

        public override string ToString()
        {
            return this.path == null ? "Path:null" : "Path:" + this.path.ToString();
        }
    }
}
