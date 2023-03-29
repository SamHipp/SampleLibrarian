using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Sample_Librarian.Model;

namespace Sample_Librarian.Services
{
    public class DBService
    {
        static SQLiteAsyncConnection db;
        public static async Task Init()
        {
            if (db != null) { return; }
            var databasePath = Path.Combine(@"X:\Programming\Projects\0323\Sample-Librarian\Services", "SampleLibrarianDB.db");
            db = new SQLiteAsyncConnection(databasePath);
            await db.CreateTableAsync<FileDirectory>();
        }

        public static async Task<int> AddFileDirectory(string name, string path, string type)
        {
            try
            {
                await Init();
                FileDirectory fileDirectory = new()
                {
                    Name = name,
                    Path = path,
                    Type = type
                };
                
                if (type == "Category")
                {
                    var existingFileDirectory = await db.QueryAsync<FileDirectory>("select * from FileDirectory where Type = 'Category'");
                    if (existingFileDirectory.Count > 0)
                    {
                        int modifiedRows = await db.ExecuteAsync($"DELETE FROM FileDirectory WHERE Type = 'Category'");
                        int rowsAddedNumber = await db.InsertAsync(fileDirectory);
                        if (modifiedRows != 0 && rowsAddedNumber != 0) { return 1; } else { return 0; }
                    }
                    else
                    {
                        int rowsAddedNumber = await db.InsertAsync(fileDirectory);
                        return rowsAddedNumber;
                    }
                }
                int objectsAddedNumber = await db.InsertAsync(fileDirectory);
                if (objectsAddedNumber > 0)
                {
                    int pk = 0;
                    List<FileDirectory> fileDirectories = await db.QueryAsync<FileDirectory>($"select * from FileDirectory where Type = '{type}' and Name = '{name}'");
                    foreach (FileDirectory fd in fileDirectories)
                    {
                        pk = fd.Pk;
                    }
                    return pk;
                } else { return 0; }
            }
            catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                return 0;
            }
        }

        public static async Task<bool> RemoveFileDirectory(int pk)
        {
            try
            {
                await Init();
                int objectsDeletedNumber = await db.DeleteAsync<FileDirectory>(pk);
                if (objectsDeletedNumber > 0) { return true; } else { return false; }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                return false;
            }
        }

        public static async Task<List<SourceFolder>> GetSourceFolderFileDirectories()
        {
            List<SourceFolder> sourceFolders = new();
            try
            {
                await Init();
                List<FileDirectory> fileDirectories = await db.QueryAsync<FileDirectory>("select * from FileDirectory where Type = 'SourceFolder'");
                foreach (FileDirectory fileDirectory in fileDirectories)
                {
                    SourceFolder sourceFolder = new()
                    {
                        Name = fileDirectory.Name,
                        FilePath = fileDirectory.Path,
                        Pk = fileDirectory.Pk
                    };
                    
                    sourceFolders.Add(sourceFolder);
                }
                return sourceFolders;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                return sourceFolders;
            }
        }

        public static async Task<FileDirectory> GetCategoryFileDirectory()
        {
            FileDirectory fileDirectory = new();
            try
            {
                await Init();
                List<FileDirectory> fileDirectories = await db.QueryAsync<FileDirectory>("select * from FileDirectory where Type = 'Category'");
                foreach (FileDirectory fd in fileDirectories)
                {
                    fileDirectory.Name = fd.Name;
                    fileDirectory.Path = fd.Path;
                    fileDirectory.Pk = fd.Pk;
                    fileDirectory.Type = fd.Type;
                }
                return fileDirectory;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                return fileDirectory;
            }
        }
    }
}
