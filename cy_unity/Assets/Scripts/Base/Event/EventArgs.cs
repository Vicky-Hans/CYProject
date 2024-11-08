
using System.Text;

namespace SP.Base
{
    public class EventArgs
    {
        public object[] Args { get; private set; }

        public EventArgs(params object[] args)
        {
            Args = args;
        }

        public override string ToString()
        {
            if (Args.Length == 0)
            {
                return "no params";
            }
            if (Args.Length == 1)
            {
                return Args[0].ToString();
            }

            var sb = new StringBuilder();
            for (int i = 0; i < Args.Length; i++)
            {
                if (i == Args.Length - 1)
                {
                    sb.Append(Args[i]);
                }
                else
                {
                    sb.Append(Args[i].ToString() + ",");
                }
            }
            return sb.ToString();
        }
    }
}