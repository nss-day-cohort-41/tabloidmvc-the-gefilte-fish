﻿using System.Collections.Generic;
using TabloidMVC.Models;

namespace TabloidMVC.Repositories
{
    public interface ICategoryRepository
    {
        List<Category> GetAll();
        void Add(Category category);
        Category GetCategoryById(int id);
        void Delete(int id);
        void Update(Category category);
    }
}