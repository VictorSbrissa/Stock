using Microsoft.AspNetCore.Mvc;
using WebApplication1.Logic;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CategoryController : ControllerBase
    {


        private readonly ILogger<CategoryController> _logger;
        private readonly CategoryLogic _categoryLogic;

        public CategoryController(ILogger<CategoryController> logger, CategoryLogic categoryLogic)
        {
            _logger = logger;
            _categoryLogic = categoryLogic;
        }

        [HttpGet]
        public async Task<List<Category>> List()
        {
            return await _categoryLogic.List();
        }
    }
}
