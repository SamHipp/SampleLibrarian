using System;
using System.ComponentModel;
using System.IO;
using System.Collections;
using System.Windows.Input;
using Sample_Librarian.Model;
using Sample_Librarian.ViewModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using Sample_Librarian.Services;
using System.Diagnostics;
using System.Media;
using Plugin.Maui.Audio;
using Microsoft.Maui.Controls;
using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Storage;
using CommunityToolkit.Maui.Core.Primitives;
using System.Threading;
using System.Data;

namespace Sample_Librarian.ViewModel;
public partial class MainViewModel : BaseViewModel
{
    FileDataRowService fileDataRowService;
    CategoryService categoryService;
    SourceFolderService sourceFolderService;
    DBService dBService;
    public IFolderPicker folderPicker;

    public ObservableCollection<FileDataRow> FileDataRows { get; set; } = new();

    public ObservableCollection<SourceFolder> SourceFolders { get; set; } = new();

    public ObservableCollection<CategoryGroup> CategoryGroups { get; set; } = new();

    public string ActiveCategoryFilePath = null;

    [ObservableProperty]
    double volumeLevel = 100;

    [ObservableProperty]
    bool allSelected = false;
    [ObservableProperty]
    public bool isFileDataRowsLoaded = true;
    [ObservableProperty]
    public bool isFileDataRowsNotLoaded = false;

    [ObservableProperty]
    public bool isSourceFolderPresent = false;

    [ObservableProperty]
    public string currentSourceFolderPath = "";

    [ObservableProperty]
    public string categoriesBaseFilePath = @"X:\Downloads\test\TestCategories";

    public MainViewModel(FileDataRowService fileDataRowService, CategoryService categoryService, SourceFolderService sourceFolderService, IFolderPicker folderPicker, DBService dBService)
    {
        this.fileDataRowService = fileDataRowService;
        this.categoryService = categoryService;
        this.sourceFolderService = sourceFolderService;
        this.folderPicker = folderPicker;
        this.dBService = dBService;
    }

    public async Task OnAppStartup()
    {
        IsFileDataRowsLoaded = false;
        IsFileDataRowsNotLoaded = true;
        OnPropertyChanged(nameof(IsFileDataRowsLoaded));
        OnPropertyChanged(nameof(IsFileDataRowsNotLoaded));
        List<SourceFolder> sourceFolders = await DBService.GetSourceFolderFileDirectories();
        FileDirectory categoriesBaseFPFD = await DBService.GetCategoryFileDirectory();
        if (categoriesBaseFPFD != null) { CategoriesBaseFilePath = categoriesBaseFPFD.Path; }
        if (sourceFolders == null || sourceFolders.Count > 0)
        {
            CurrentSourceFolderPath = sourceFolders[0].FilePath;
            sourceFolders[0].IsSelected = true;
            IsSourceFolderPresent = true;
            foreach (SourceFolder sourceFolder in sourceFolders)
            {
                SourceFolders.Add(sourceFolder);
            }
            await GetFiles(sourceFolders[0].FilePath);
            if (CategoriesBaseFilePath.Length > 0) { GetCategoryGroup(CategoriesBaseFilePath); }
            IsFileDataRowsLoaded = true;
            IsFileDataRowsNotLoaded = false;
            OnPropertyChanged(nameof(IsFileDataRowsLoaded));
            OnPropertyChanged(nameof(IsFileDataRowsNotLoaded));
            OnPropertyChanged(nameof(SourceFolders));
            OnPropertyChanged(nameof(CurrentSourceFolderPath));
            OnPropertyChanged(nameof(IsSourceFolderPresent));
        }
        else 
        {
            IsFileDataRowsLoaded = true;
            IsFileDataRowsNotLoaded = false;
            OnPropertyChanged(nameof(IsFileDataRowsLoaded));
            OnPropertyChanged(nameof(IsFileDataRowsNotLoaded));
            return; 
        }
    }

    [RelayCommand]
    public async Task AddSourceFolder()
    {
        try
        {
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = cancellationTokenSource.Token;
            IsFileDataRowsLoaded = false;
            IsFileDataRowsNotLoaded = true;
            OnPropertyChanged(nameof(IsFileDataRowsLoaded));
            OnPropertyChanged(nameof(IsFileDataRowsNotLoaded));
            SourceFolder sourceFolder = await sourceFolderService.GetSourceFolder(cancellationToken);
            if (sourceFolder.Id == 0) { return; }
            sourceFolder.Pk = await DBService.AddFileDirectory(sourceFolder.Name, sourceFolder.FilePath, "SourceFolder");
            cancellationTokenSource.Dispose();
            if (sourceFolder.Pk > 0)
            {
                SourceFolders.Clear();
                List<SourceFolder> sourceFolders = await DBService.GetSourceFolderFileDirectories();
                for (int i = 0; i < sourceFolders.Count; i++)
                {
                    if (sourceFolders[i].FilePath == sourceFolder.FilePath)
                    {
                        sourceFolders[i].IsSelected = true;
                    }
                    SourceFolders.Add(sourceFolders[i]);
                }
                CurrentSourceFolderPath = sourceFolder.FilePath;
                IsSourceFolderPresent = true;
                OnPropertyChanged(nameof(SourceFolders));
                OnPropertyChanged(nameof(CurrentSourceFolderPath));
                OnPropertyChanged(nameof(IsSourceFolderPresent));

                await GetFiles(sourceFolder.FilePath);
            }
            else { 
                await Shell.Current.DisplayAlert("Error!", "There was a problem with adding the Source Folder to the Database.", "OK");
                IsFileDataRowsLoaded = true;
                IsFileDataRowsNotLoaded = false;
                OnPropertyChanged(nameof(IsFileDataRowsLoaded));
                OnPropertyChanged(nameof(IsFileDataRowsNotLoaded));
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
            await Shell.Current.DisplayAlert("Error!", $"{ex.Message}", "OK");
        }
    }

    [RelayCommand]
    public async Task RemoveSourceFolder()
    {
        try
        {
            SourceFolder selectedSourceFolder = new();
            foreach (var folder in SourceFolders)
            {
                if (folder.IsSelected == true)
                {
                    selectedSourceFolder = folder;
                }
            }
            SourceFolders.Remove(selectedSourceFolder);
            OnPropertyChanged(nameof(SourceFolders));
            bool result = await DBService.RemoveFileDirectory(selectedSourceFolder.Pk);
            if (result == true)
            {
                await Shell.Current.DisplayAlert("Source Folder Deleted!", $"The \"{selectedSourceFolder.Name}\" folder has been removed from the list.", "OK");
            }
            if (SourceFolders.Count == 0) { IsSourceFolderPresent = false; OnPropertyChanged(nameof(IsSourceFolderPresent)); }
        }
        catch(Exception ex)
        {
            Debug.WriteLine(ex);
            await Shell.Current.DisplayAlert("Error!", $"{ex.Message}", "OK");
        }
    }


    [RelayCommand]
    public async Task GetFiles(string filePath)
    {
        try
        {
            IsFileDataRowsLoaded = false;
            IsFileDataRowsNotLoaded = true;
            OnPropertyChanged(nameof(IsFileDataRowsLoaded));
            OnPropertyChanged(nameof(IsFileDataRowsNotLoaded));
            await Task.Run(() =>
            {
                for (int i = 0; i < SourceFolders.Count; i++)
                {
                    SourceFolders[i].IsSelected = false;
                }
                CurrentSourceFolderPath = filePath;
                if (FileDataRows.Count > 0)
                {
                    for (int i = 0; i < FileDataRows.Count; i++)
                    {
                        if (FileDataRows[i].HasPlayer == true && FileDataRows[i].Player != null)
                        {
                            FileDataRows[i].Player.Dispose();
                        }
                    }
                }
            });
            await Task.Run(() =>
            {
                FileDataRows.Clear();
                OnPropertyChanged("FileDataRows");
                List<FileDataRow> dataRows = fileDataRowService.GetFileDataRows(filePath);
                foreach (var dataRow in dataRows)
                {
                    FileDataRows.Add(dataRow);
                };
            });
            await Task.Run(() =>
            {
                for (int j = 0; j < SourceFolders.Count; j++)
                {
                    if (SourceFolders[j].FilePath == CurrentSourceFolderPath)
                    {
                        SourceFolders[j].IsSelected = true;
                    }
                }
                AllSelected = false;
                IsFileDataRowsLoaded = true;
                IsFileDataRowsNotLoaded = false;
                OnPropertyChanged(nameof(IsFileDataRowsLoaded));
                OnPropertyChanged(nameof(IsFileDataRowsNotLoaded));
                OnPropertyChanged(nameof(FileDataRows));
                OnPropertyChanged(nameof(SourceFolders));
                OnPropertyChanged();
            });
        }
        catch (Exception ex)
        { 
            Debug.WriteLine( ex );
            await Shell.Current.DisplayAlert("Error!", $"{ex.Message}", "OK");
        }
    }

    [RelayCommand]
    public void StartChangeFDRName(FileDataRow fileDataRow)
    {
        try
        {
            fileDataRow.IsChangingName = true;
            fileDataRow.IsNotChangingName = false;
            for (int i = 0; i < FileDataRows.Count; i++)
            {
                if (FileDataRows[i].IsChangingName == true)
                {
                    FileDataRows[i].IsChangingName = false;
                    FileDataRows[i].IsNotChangingName = true;
                }
                if (FileDataRows[i].Id == fileDataRow.Id)
                {
                    FileDataRows[i].IsChangingName = true;
                    FileDataRows[i].IsNotChangingName = false;
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
            Shell.Current.DisplayAlert("Error!", $"{ex.Message}", "OK");
        }
    }

    [RelayCommand]
    public async Task FDRNameChanged(string inputText)
    {
        try
        {
            for (int i = 0; i < FileDataRows.Count; i++)
            {
                if (FileDataRows[i].IsChangingName == true)
                {
                    bool alreadyExists = false;
                    try
                    {
                        for (int j = 0; j < FileDataRows.Count; j++)
                        {
                            if (FileDataRows[j].FileName == inputText)
                            {
                                alreadyExists = true;
                            }
                        }
                        if (!alreadyExists)
                        {
                            if (FileDataRows[i].HasPlayer == true)
                            {
                                FileDataRows[i].Player.Dispose();
                            }
                            string newFilePath = $"{CurrentSourceFolderPath}\\{inputText}{FileDataRows[i].Format}";
                            await Task.Run(() => Directory.Move(FileDataRows[i].FilePath, newFilePath));
                            FileDataRow newDataRow = new();
                            FileDataRows[i].Player.Dispose();
                            newDataRow.FileName = inputText;
                            newDataRow.FilePath = newFilePath;
                            newDataRow.Id = i;
                            newDataRow.Format = FileDataRows[i].Format;
                            newDataRow.Size = FileDataRows[i].Size;
                            newDataRow.IsChangingName = false;
                            newDataRow.IsNotChangingName = true;
                            if (FileDataRows[i].HasPlayer == true)
                            {
                                Stream stream = new FileStream(newDataRow.FilePath, FileMode.Open, FileAccess.Read);
                                newDataRow.Player = AudioManager.Current.CreatePlayer(stream);
                                newDataRow.HasPlayer = true;
                                newDataRow.PlayerIcon = "play_icon.png";
                            }
                            FileDataRows.Remove(FileDataRows[i]);
                            OnPropertyChanged(nameof(FileDataRows));
                            FileDataRows.Insert(i, newDataRow);
                            break;
                        }
                        else
                        {
                            await Shell.Current.DisplayAlert("Error!", $"The {inputText} file already exists!", "OK");
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex);
                        await Shell.Current.DisplayAlert("Error!", $"{ex.Message}", "OK");
                    }

                }
            }
            OnPropertyChanged("CategoryGroups");
            OnPropertyChanged();

        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
            await Shell.Current.DisplayAlert("Error!", $"{ex.Message}", "OK");
        }
    }

    [RelayCommand]
    public void CancelChangeFDRName()
    {
        try
        {
            for (int i = 0; i < FileDataRows.Count; i++)
            {
                if (FileDataRows[i].IsChangingName == true)
                {
                    FileDataRows[i].IsChangingName = false;
                    FileDataRows[i].IsNotChangingName = true;
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
            Shell.Current.DisplayAlert("Error!", $"{ex.Message}", "OK");
        }
    }

    [RelayCommand]
    public void GetCategoryGroup(string parentFilePath)
    {
        try
        {
            if (parentFilePath == null || parentFilePath.Length == 0) { parentFilePath = CategoriesBaseFilePath; }
            CategoryGroup categoryGroup = CategoryService.GetCategoryGroup(parentFilePath);
            for (int i = CategoryGroups.Count - 1; i >= 0; i--)
            {
                if (CategoryGroups[i].Id >= categoryGroup.Id)
                {
                    CategoryGroups.RemoveAt(i);
                }
            }
            CategoryGroups.Add(categoryGroup);
            ActiveCategoryFilePath = parentFilePath;
            if (ActiveCategoryFilePath != null && ActiveCategoryFilePath.Length > 0) 
            {
                for (int i = 0; i < CategoryGroups.Count; i++)
                {
                    if (CategoryGroups[i].Id + 1 == categoryGroup.Id)
                    {
                        for (int j = 0; j < CategoryGroups[i].Categories.Count; j++)
                        {
                            if (CategoryGroups[i].Categories[j].IsSelected == true)
                            {
                                CategoryGroups[i].Categories[j].IsSelected = false;
                            }
                            if (CategoryGroups[i].Categories[j].FilePath == parentFilePath)
                            {
                                CategoryGroups[i].Categories[j].IsSelected = true;
                            }
                        }
                    }
                }
            }
            OnPropertyChanged("CategoryGroups");
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
            Shell.Current.DisplayAlert("Error!", $"{ex.Message}", "OK");
        }
    }

    [RelayCommand]
    public async Task SetCategoriesBaseFilePath(CancellationToken cancellationToken)
    {
        try
        {
            FileDirectory fileDirectory = await categoryService.SetBaseFilePath(cancellationToken);
            if (fileDirectory.Name == "") { return; }
            fileDirectory.Pk = await DBService.AddFileDirectory(fileDirectory.Name, fileDirectory.Path, "Category");
            if (fileDirectory.Pk != 0) {
                CategoriesBaseFilePath = fileDirectory.Path;
                OnPropertyChanged(nameof(CategoriesBaseFilePath));

                GetCategoryGroup(fileDirectory.Path);
            }
            else
            {
                await Shell.Current.DisplayAlert("Error!", "Couldn't set the Categories base directory", "OK");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
            await Shell.Current.DisplayAlert("Error!", $"{ex.Message}", "OK");
        }
    }

    [RelayCommand]
    public void ClearCategories()
    {
        if (CategoryGroups.Count > 1)
        {
            CategoryGroup categoryGroup = CategoryGroups.FirstOrDefault();
            CategoryGroups.Clear();
            CategoryGroups.Add(categoryGroup);
            OnPropertyChanged(nameof(CategoryGroups));
        }
    }

    [RelayCommand]
    public void StartAddCategory(CategoryGroup categoryGroup)
    {
        try
        {
            for (int i = 0; i < CategoryGroups.Count; i++)
            {
                if (CategoryGroups[i].IsAdding == true)
                {
                    CategoryGroups[i].IsAdding = false;
                }
            }
            categoryGroup.IsAdding= true;
            for (int i = 0; i < CategoryGroups.Count; i++)
            {
                if (CategoryGroups[i].Id == categoryGroup.Id)
                {
                    CategoryGroups.Remove(CategoryGroups[i]);
                    CategoryGroups.Insert(i, categoryGroup);
                    return;
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
            Shell.Current.DisplayAlert("Error!", $"{ex.Message}", "OK");
        }
    }

    [RelayCommand]
    async Task OnCategoryTextChanged(string inputText)
    {
        try
        {
            for (int i = 0; i < CategoryGroups.Count; i++)
            {
                if (CategoryGroups[i].IsAdding == true)
                {
                    bool alreadyExists = false;
                    try
                    {
                        CategoryGroups[i].Categories.ForEach(category =>
                        {
                            if (category.Name == inputText)
                            {
                                alreadyExists = true;
                            }
                        });
                        if (!alreadyExists)
                        {
                            Category category = new Category();
                            CategoryGroup categoryGroup = CategoryService.GetCategoryGroup(CategoryGroups[i].FilePath);
                            category.Name = inputText;
                            category.FilePath = CategoryGroups[i].FilePath;
                            category.ColumnNumber = CategoryGroups[i].Categories.Count % 4;
                            category.RowNumber = Convert.ToInt32(Math.Floor(Convert.ToDecimal(CategoryGroups[i].Categories.Count) / 4));
                            Directory.CreateDirectory($"{category.FilePath}/{inputText}");
                            categoryGroup.Categories.Add(category);
                            categoryGroup.IsAdding = false;
                            CategoryGroups.Remove(CategoryGroups[i]);
                            OnPropertyChanged("CategoryGroups");
                            if (categoryGroup != null) { CategoryGroups.Insert(i, categoryGroup); }
                            GetCategoryGroup(CategoryGroups[i].FilePath);
                        }
                        else
                        {
                            await Shell.Current.DisplayAlert("Error!", $"The {inputText} category already exists!", "OK");
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex);
                        await Shell.Current.DisplayAlert("Error!", $"{ex.Message}", "OK");
                    }
                        
                }
            }
                
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
            await Shell.Current.DisplayAlert("Error!", $"{ex.Message}", "OK");
        }
    }

    [RelayCommand]
    async Task CancelAddCategory(CategoryGroup categoryGroup)
    {
        try
        {
            for (int i = 0; i < CategoryGroups.Count; i++)
            {
                if (CategoryGroups[i].IsAdding == true)
                {
                    CategoryGroups[i].IsAdding = false;
                }
                CategoryGroups[i].AddCategoryText = "+";
                if (CategoryGroups[i].Id== categoryGroup.Id)
                {
                    CategoryGroups.Remove(CategoryGroups[i]);
                    OnPropertyChanged("CategoryGroups");
                    CategoryGroup newCategoryGroup = CategoryService.GetCategoryGroup(categoryGroup.FilePath);
                    if (newCategoryGroup != null) { CategoryGroups.Insert(i, newCategoryGroup); }
                    OnPropertyChanged("CategoryGroups");
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
            await Shell.Current.DisplayAlert("Error!", $"{ex.Message}", "OK");
        }
    }

    [RelayCommand]
    public async Task DeleteCategory()
    {
        string name = "";
        try
        {
            name = Path.GetFileNameWithoutExtension(ActiveCategoryFilePath);
            bool confirmed = await Shell.Current.DisplayAlert($"Delete the \"{name}\" folder and all its contents?", null, "Yes", "No");
            if (confirmed)
            {
                for (int i = 0; i < CategoryGroups.Count; i++)
                {
                    if (CategoryGroups[i].IsAdding == true)
                    {
                        CategoryGroups[i].IsAdding = false;
                    }
                    CategoryGroups[i].AddCategoryText = "+";
                    if (CategoryGroups[i].FilePath == ActiveCategoryFilePath)
                    {
                        Directory.Delete(ActiveCategoryFilePath, true);
                        CategoryGroups.Remove(CategoryGroups[i]);
                        OnPropertyChanged("CategoryGroups");
                    }
                }
                CategoryGroups.Clear();
                GetCategoryGroup(CategoriesBaseFilePath);

            }
            else { return; }
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
            await Shell.Current.DisplayAlert("Error!", $"{ex.Message}", "OK");
        }
    }
    public async Task OnAllSelectedChanged()
    {
        try
        {
            if (AllSelected)
            {
                for (int i = 0; i < FileDataRows.Count; i++)
                {
                    FileDataRows[i].IsSelected = true;
                }
            }
            else
            {
                for (int i = 0; i < FileDataRows.Count; i++)
                {
                    FileDataRows[i].IsSelected = false;
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
            await Shell.Current.DisplayAlert("Error!", $"{ex.Message}", "OK");
        }
    }

    [RelayCommand]
    async Task MoveFiles()
    {
        try
        {
            List<FileDataRow> files = new List<FileDataRow>();
            await Task.Run(() =>
            {
                for (int i = 0; i < FileDataRows.Count; i++)
                {
                    if (FileDataRows[i].IsSelected)
                    {
                        if (FileDataRows[i].HasPlayer == true)
                        {
                            FileDataRows[i].Player.Dispose();
                        }
                        files.Add(FileDataRows[i]);
                    }
                }
            });
            await Task.Run(() =>
            {
                files.ForEach((file) => {
                    if (file.HasPlayer == true)
                    {
                        file.Player.Dispose();
                    }
                    File.Move(file.FilePath, $"{ActiveCategoryFilePath}\\{file.FileName}{file.Format}");
                });
            });
            await GetFiles(CurrentSourceFolderPath);
        }
        catch(Exception ex)
        {
            Debug.WriteLine(ex);
            await Shell.Current.DisplayAlert("Error!", $"{ex.Message}", "OK");
        }
    }

    [RelayCommand]
    async Task DeleteFiles()
    {
        try
        {
            bool confirmed = await Shell.Current.DisplayAlert("Delete these files?", null, "Yes", "No");
            if (confirmed)
            {
                List<FileDataRow> files = new();
                for (int i = 0; i < FileDataRows.Count; i++)
                {
                    if (FileDataRows[i].IsSelected)
                    {
                        if (FileDataRows[i].HasPlayer == true)
                        {
                            FileDataRows[i].Player.Dispose();
                        }
                        files.Add(FileDataRows[i]);
                    }
                }
                files.ForEach((file) =>
                {
                    if (file.HasPlayer == true)
                    {
                        file.Player.Dispose();
                    }
                    File.Delete(file.FilePath);
                });
                await GetFiles(CurrentSourceFolderPath);
            }
            else { return; }
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
            await Shell.Current.DisplayAlert("Error!", $"{ex.Message}", "OK");
        }
    }

    [RelayCommand]
    async void PlaySound(FileDataRow fileDataRow)
    {
        
        if (fileDataRow == null) { return; }
        try
        {
            if (fileDataRow.Player != null && fileDataRow.Player.IsPlaying)
            {
                fileDataRow.Player.Stop();
                fileDataRow.PlayerIcon = "play_icon.png";
            }
            else
            {
                
                if (fileDataRow.Player == null)
                {
                    Stream stream = new FileStream(fileDataRow.FilePath, FileMode.Open, FileAccess.Read);
                    fileDataRow.Player = AudioManager.Current.CreatePlayer(stream);
                }
                fileDataRow.Player.Volume = (VolumeLevel / 100);
                fileDataRow.PlayerIcon = "pause_icon.png";
                fileDataRow.Player.Play();
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine( ex );
            await Shell.Current.DisplayAlert("Error!", $"{ex.Message}", "OK");
        }
    }

    [RelayCommand]
    async public void NavigateToSettings()
    {
        try
        {
            await Shell.Current.GoToAsync(nameof(SettingsPage));
        }
        catch(Exception ex)
        {
            Debug.WriteLine(ex);
            await Shell.Current.DisplayAlert("Error!", $"{ex.Message}", "OK");
        }
    }
}
