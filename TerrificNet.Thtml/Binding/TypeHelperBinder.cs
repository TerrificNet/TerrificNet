using System;
using System.Collections.Concurrent;

namespace TerrificNet.Thtml.Binding
{
    public static class TypeHelperBinder
    {
        private static ConcurrentDictionary<Tuple<Type, string>, Func<object, object>> lateBoundCache = new ConcurrentDictionary<Tuple<Type, string>, Func<object, object>>();

        public static Func<object, object> GetBinder(object model, string itemName)
        {
            return GetBinder(model, itemName, MemberLocator.Default);
        }

        public static Func<object, object> GetBinder(object model, string itemName, IMemberLocator memberLocator)
        {
            var binder = lateBoundCache.GetOrAdd(Tuple.Create(model.GetType(), itemName), pair =>
            {
                var type = pair.Item1;
                var name = pair.Item2;

                if (name.EndsWith("()"))
                {
                    var function =
                        memberLocator.FindMethod(type, name.Substring(0, name.Length - 2));
                    if (function != null) return DelegateBuilder.FunctionCall(type, function);
                }

                var property = memberLocator.FindProperty(type, name);
                if (property != null) return DelegateBuilder.Property(type, property);

                var field = memberLocator.FindField(type, name);
                if (field != null) return DelegateBuilder.Field(type, field);

                var dictionaryType = type.GetDictionaryTypeWithKey();
                if (dictionaryType != null) return DelegateBuilder.Dictionary(dictionaryType, name);

                return null;
            });
            return binder;
        }
    }
}