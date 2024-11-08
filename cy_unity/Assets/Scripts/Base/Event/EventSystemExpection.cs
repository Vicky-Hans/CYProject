using System;

namespace SP.Base
{
    public class AddListenerExpection : Exception
    {
        public AddListenerExpection(string msg) : base(msg) { }
    }

    public class RemoveListenerExpection : Exception
    {
        public RemoveListenerExpection(string msg) : base(msg) { }
    }

    public class InvalidTypeExpection : Exception
    {
        public InvalidTypeExpection(string msg) : base(msg) { }
    }
}