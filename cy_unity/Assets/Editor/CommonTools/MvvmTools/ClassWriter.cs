using System;
using System.Text;

public class ClassWriter : BaseCodeGenerator
{
    private Type classType;
        
    public ClassWriter(Type type, StringBuilder stringBuilder) : base(stringBuilder)
    {
        classType = type;
    }
        
    public override void Dispose()
    {
            
    }
}