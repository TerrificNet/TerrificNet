using System;
using Microsoft.Extensions.Caching.Memory;

namespace TerrificNet.ViewEngine.Cache
{
	public class MemoryCacheProvider : ICacheProvider
	{
		private readonly IMemoryCache _cache;

	   public MemoryCacheProvider(IMemoryCache cache)
	   {
	      _cache = cache;
	   }

		public void Set<TValue>(string key, TValue value, DateTimeOffset offset)
		{
		   _cache.Set(key, value,
		      new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromHours(24))
		         .SetPriority(CacheItemPriority.NeverRemove));
		}

		public bool TryGet<TValue>(string key, out TValue value) where TValue : class
		{
			value = _cache.Get(key) as TValue;
			return value != null;
		}
	}
}