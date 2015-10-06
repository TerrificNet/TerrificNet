using System;

namespace TerrificNet.Thtml.Emit
{
    public interface IDataBinder
    {
        DataBinderResult Evaluate(string propertyName);

        DataBinderResult Item();
        DataBinderResult Context();
    }

    public abstract class DataBinderResult : IDataBinder
    {
        public abstract Func<IDataContext, T> CreateEvaluation<T>();
        public Type ResultType { get; }

        public DataBinderResult(Type resultType)
        {
            ResultType = resultType;
        }

        public abstract DataBinderResult Evaluate(string propertyName);

        public abstract DataBinderResult Item();
        public DataBinderResult Context()
        {
            return this;
        }
    }
}