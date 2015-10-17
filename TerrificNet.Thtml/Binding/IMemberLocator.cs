using System;
using System.Reflection;

namespace TerrificNet.Thtml.Binding
{
    public interface IMemberLocator
    {
        MemberInfo FindMember(Type modelType, string name, MemberTypes types);
    }
}