using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using TerrificNet.Mvc.Core;
using TerrificNet.Sample.Models;

namespace TerrificNet.Sample.Controllers
{
	public class TodoController
	{
		[HttpGet]
		public IActionResult Index()
		{
			var list = new List<TodoItem>
			{
				new TodoItem { Id = "1", Description = "test1" },
				new TodoItem { Id = "2", Description = "test2", IsDone = true },
				new TodoItem { Id = "3", Description = "test3" }
			};
			return new ViewResult("todo", list);
		}
	}
}
