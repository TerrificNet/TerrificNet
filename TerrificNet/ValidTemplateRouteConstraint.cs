using System.Collections.Generic;
using System.Net.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using TerrificNet.ViewEngine;
using TerrificNet.ViewEngine.Config;
using TerrificNet.ViewEngine.IO;

namespace TerrificNet
{
	internal class ValidTemplateRouteConstraint : IRouteConstraint
	{
		private readonly ITemplateRepository _templateRepository;
		private readonly IFileSystem _fileSystem;
	    private readonly PathInfo _viewPathInfo;

	    public ValidTemplateRouteConstraint(ITemplateRepository templateRepository, IFileSystem fileSystem,
			ITerrificNetConfig configuration)
		{
	        _templateRepository = templateRepository;
			_fileSystem = fileSystem;
	        _viewPathInfo = configuration.ViewPath;
		}

		public bool Match(HttpContext httpContext, IRouter route, string routeKey, RouteValueDictionary values,
			RouteDirection routeDirection)
		{
			object pathObj;
			if (values.TryGetValue(routeKey, out pathObj))
			{
				var path = pathObj as string;
				if (!string.IsNullOrEmpty(path))
				{
                    // TODO Find async way
				    var templateInfo = _templateRepository.GetTemplateAsync(path).Result;
					if (templateInfo != null)
						return true;

                    var fileName = _fileSystem.Path.ChangeExtension(_fileSystem.Path.Combine(_viewPathInfo, PathInfo.Create(path)),
						"html.json");
					if (_fileSystem.FileExists(fileName))
						return true;
				}
			}

			return false;
		}
	}
}