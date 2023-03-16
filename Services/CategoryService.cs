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
            categoryGroup.Id = parentFilePath.Split('\\').Length;
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

        public Category AddCategory(CategoryGroup categoryGroup, string name, string filePath)
        {
            bool alreadyExists = false;
            try
            {
                categoryGroup.Categories.ForEach(category =>
                {
                    if (category.Name == name)
                    {
                        alreadyExists = true;
                    }
                });
                if (!alreadyExists)
                {
                    Category category = new Category();
                    category.Name = name;
                    category.FilePath = filePath;
                    Directory.CreateDirectory($"{filePath}\\{name}");
                    return category;
                }
                else
                {
                    return null;
                }
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
