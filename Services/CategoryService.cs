using CommunityToolkit.Maui.Storage;
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
            if (parentFilePath == null || parentFilePath.Length == 0) { parentFilePath = @"X:\Downloads\test\TestCategories"; }
            categoryGroup.Id = parentFilePath.Split('\\').Length;
            categoryGroup.FilePath = parentFilePath;
            string[] files = Directory.GetDirectories(parentFilePath);
            for (int i = 0; i < files.Length; i++)
            {
                Category category = new Category();
                category.Name = Path.GetFileName(files[i]);
                category.FilePath = files[i];
                category.ColumnNumber = i % 4;
                category.RowNumber = Convert.ToInt32(Math.Floor(Convert.ToDecimal(i) / 4));
                categoryGroup.Categories.Add(category);
            }
            return categoryGroup;
        }

        public static Category AddCategory(CategoryGroup categoryGroup, string name, string filePath)
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
                    category.ColumnNumber = categoryGroup.Categories.Count % 4;
                    category.RowNumber = Convert.ToInt32(Math.Floor(Convert.ToDecimal(categoryGroup.Categories.Count) / 4));
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

        public async Task<FileDirectory> SetBaseFilePath(CancellationToken cancellationToken)
        {
            FileDirectory directory = new();
            try
            {
                var result = await FolderPicker.Default.PickAsync(cancellationToken);
                result.EnsureSuccess();
                directory.Path = result.Folder.Path;
                directory.Name = result.Folder.Name;
                directory.Type = "Category";
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
            return directory;
        }
    }
}
