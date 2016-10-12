using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using TerrificNet.Mvc.Core;
using TerrificNet.Sample.Models;

namespace TerrificNet.Sample.Controllers
{
	public class TodoController
	{
		private static List<TodoItem> _model = new List<TodoItem>
			{
				new TodoItem { Id = "1", Description = "test1" },
				new TodoItem { Id = "2", Description = "test2", IsDone = true },
				new TodoItem { Id = "3", Description = "test3" }
			};

		[HttpGet]
		public IActionResult Index()
		{
			return new ViewResult("todo", _model);
		}

		[HttpPost]
		public IActionResult Add(string description)
		{
			_model.Add(new TodoItem {Description = description});
			return Index();
		}
	}
}
