using System;
using System.Text;

public class BaseCodeGenerator : IDisposable
{
    protected StringBuilder stringBuilder;

    protected BaseCodeGenerator(StringBuilder writer)
    {
        stringBuilder = writer;
    }
        
    public virtual void Dispose()
    {
            
    }
}