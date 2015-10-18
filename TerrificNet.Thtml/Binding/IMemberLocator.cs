using System;
using System.Reflection;

namespace TerrificNet.Thtml.Binding
{
    public interface IMemberLocator
    {
        MethodInfo FindMethod(Type modelType, string name);

        PropertyInfo FindProperty(Type modelType, string expression);

        FieldInfo FindField(Type modelType, string expression);
    }
}