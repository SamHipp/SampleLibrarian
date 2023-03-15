using Sample_Librarian.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sample_Librarian.Services
{
    public class CategoryService
    {
        List<CategoryGroup> categoryGroups = new List<CategoryGroup>();

        public static CategoryGroup GetCategoryGroup(string parentFilePath)
        {
            CategoryGroup categoryGroup = new CategoryGroup();
            categoryGroup.Categories = new List<Category>();
            if (parentFilePath.Length == 0) { parentFilePath = @"X:\Downloads\test\TestCategories"; }
            string[] files = Directory.GetDirectories(parentFilePath);
            foreach (string file in files)
            {
                Category category = new Category();
                category.Name = Path.GetFileName(file);
                category.FilePath = file;


                categoryGroup.Categories.Add(category);

            }
            return categoryGroup;
        }

        public Category AddCategory(string name, string filePath)
        {
            Category category = new Category();
            try
            {
                category.Name = name;
                category.FilePath = filePath;
                return category;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                Shell.Current.DisplayAlert("Error!", $"{ex.Message}", "OK");
                return null;
            }
        }
    }
}
