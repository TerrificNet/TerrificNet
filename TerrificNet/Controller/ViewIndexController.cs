﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using TerrificNet.Models;
using TerrificNet.ViewEngine;
using TerrificNet.ViewEngine.Config;

namespace TerrificNet.Controller
{
    public class ViewIndexController : TemplateControllerBase
    {
        private readonly IViewEngine _viewEngine;
        private readonly ITerrificNetConfig _configuration;
        private readonly ITerrificNetConfig[] _configurations;

        public ViewIndexController(IViewEngine viewEngine, ITerrificNetConfig configuration, ITerrificNetConfig[] configurations)
        {
            _viewEngine = viewEngine;
            _configuration = configuration;
            _configurations = configurations;
        }

        [HttpGet]
        public HttpResponseMessage Get()
        {
            var model = new ViewOverviewModel
            {
                Views = GetViews().ToList()
            };

            IView view;
            if (!_viewEngine.TryCreateViewFromPath("index.html", out view))
                return new HttpResponseMessage(HttpStatusCode.NotFound);

            return Render(view, model);
        }

        private IEnumerable<ViewItemModel> GetViews()
        {
            var applicationConfiguration = _configurations.First(c => c != _configuration);
            foreach (var file in Directory.GetFiles(applicationConfiguration.ViewPath))
            {
                yield return new ViewItemModel
                {
                    Text = Path.GetFileNameWithoutExtension(file),
                    Url = string.Format("/{0}", Path.GetFileName(file)),
                    EditUrl = string.Format("/web/edit.html?template={0}", Path.GetFileName(file)),
                    SchemaUrl = string.Format("/schema/{0}", Path.GetFileName(file))
                };
            }
        }
    }
}