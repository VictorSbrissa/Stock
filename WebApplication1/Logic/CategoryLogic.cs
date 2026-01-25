﻿using WebApplication1.Infrastructure;
using WebApplication1.Models;
using Microsoft.EntityFrameworkCore;

namespace WebApplication1.Logic
{
    public class CategoryLogic
    {
        private readonly SystemContext _systemContext;
        public CategoryLogic(SystemContext systemContext)
        {
            _systemContext = systemContext;
        }

        public async Task<List<Category>> List()
        {
            return await _systemContext.Category.ToListAsync();
        }
    }
}
