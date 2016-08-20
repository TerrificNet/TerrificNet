using System;
using System.Threading.Tasks;

namespace TerrificNet.ViewEngine
{
    public interface IViewEngine
    {
        Task<IView> CreateViewAsync(TemplateInfo templateInfo, Type modelType, IModelBinder modelBinder);
    }

    public class StaticModelBinder : IModelBinder
    {
        public static IModelBinder Create(Type type)
        {
            return new StaticModelBinder();
        }
    }

   public interface IModelBinder
   {
   }

   public static class ViewEngineExtension
    {
        public static Task<IView> CreateViewAsync(this IViewEngine viewEngine, TemplateInfo templateInfo)
        {
            return viewEngine.CreateViewAsync(templateInfo, typeof (object), StaticModelBinder.Create(typeof(object)));
        }
    }
}