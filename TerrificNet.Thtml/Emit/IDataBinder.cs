namespace TerrificNet.Thtml.Emit
{
    public interface IDataBinder
    {
        DataBinderResult Property(string propertyName);

        DataBinderResult Item();
        DataBinderResult Context();
    }

    public abstract class DataBinderResult : IDataBinder
    {
        public abstract bool TryCreateEvaluation<T>(out IEvaluater<T> evaluationFunc);

        public abstract DataBinderResult Property(string propertyName);

        public abstract DataBinderResult Item();
        public DataBinderResult Context()
        {
            return this;
        }
    }

    public interface IEvaluater<out T>
    {
        T Evaluate(IDataContext context);
    }
}