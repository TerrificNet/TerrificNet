using System;
using System.Collections.Generic;

namespace TerrificNet.Composition
{
    public class Offer
    {
        private readonly Type _type;

        public Offer(Type type)
        {
            _type = type;
        }

        public Type Type { get { return _type; } }

        public static Offer FromType(Type type)
        {
            if (type.IsInterface)
                throw new ArgumentException("Cannot use an interface as an offer.", "type");

            return new Offer(type);
        }

        public IEnumerable<Type> GetImplementingInferfaces()
        {
            return _type.GetInterfaces();
        }
    }
}