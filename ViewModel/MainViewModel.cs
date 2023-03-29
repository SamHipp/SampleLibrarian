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

    public string ActiveCatgoryFilePath = null;

    [ObservableProperty]
    double volumeLevel = 100;

    [ObservableProperty]
    bool allSelected = false;

    [ObservableProperty]
    public bool isSourceFolderPresent = false;

    [ObservableProperty]
    public bool isSourceFoldersNotFull = true;

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
        List<SourceFolder> sourceFolders = await DBService.GetSourceFolderFileDirectories();
        FileDirectory categoriesBaseFPFD = await DBService.GetCategoryFileDirectory();
        if (categoriesBaseFPFD != null) { CategoriesBaseFilePath = categoriesBaseFPFD.Path; }
        if (sourceFolders == null || sourceFolders.Count > 0) {
            CurrentSourceFolderPath = sourceFolders[0].FilePath;
            sourceFolders[0].IsSelected = true;
            IsSourceFolderPresent = true;
            foreach (SourceFolder sourceFolder in sourceFolders)
            {
                SourceFolders.Add(sourceFolder);
            }
            GetFiles(sourceFolders[0].FilePath);
            await Task.Run(() => { OnPropertyChanged(nameof(SourceFolders)); });
            await Task.Run(() => { OnPropertyChanged(nameof(CurrentSourceFolderPath)); });
            await Task.Run(() => { OnPropertyChanged(nameof(IsSourceFolderPresent)); });
        } else { return; }
    }

    [RelayCommand]
    public async Task AddSourceFolder(CancellationToken cancellationToken)
    {
        try
        {
            SourceFolder sourceFolder = await sourceFolderService.GetSourceFolder(cancellationToken);
            if (SourceFolders.Count > 0)
            {
                for (int i = 0; i < SourceFolders.Count; i++)
                {
                    SourceFolders[i].IsSelected = false;
                }
            }
            if (SourceFolders.Count > 3)
            {
                IsSourceFoldersNotFull = false;
            };
            sourceFolder.Pk = await DBService.AddFileDirectory(sourceFolder.Name, sourceFolder.FilePath, "SourceFolder");
            SourceFolders.Add(sourceFolder);
            CurrentSourceFolderPath = sourceFolder.FilePath;
            IsSourceFolderPresent = true;
            await Task.Run(() => { OnPropertyChanged(nameof(SourceFolders)); });
            await Task.Run(() => { OnPropertyChanged(nameof(CurrentSourceFolderPath)); });
            await Task.Run(() => { OnPropertyChanged(nameof(IsSourceFolderPresent)); });
                
            GetFiles(sourceFolder.FilePath);
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
    public void GetFiles(string filePath)
    {
        try
        {
            for (int i = 0; i < SourceFolders.Count; i++)
            {
                SourceFolders[i].IsSelected = false;
                if (SourceFolders[i].FilePath== filePath)
                {
                    SourceFolders[i].IsSelected = true;
                }
            }
            CurrentSourceFolderPath = filePath;
            FileDataRows.Clear();
            OnPropertyChanged("FileDataRows");
            List<FileDataRow> dataRows = fileDataRowService.GetFileDataRows(filePath);
            foreach (var dataRow in dataRows) {
                if (dataRow.HasPlayer == true)
                {
                    string seconds = "";
                    string minutes = "";
                    if (dataRow.Player.Duration >= 60)
                    {
                        seconds = (Convert.ToInt32(dataRow.Player.Duration) % 60).ToString();
                        minutes = Math.Floor((Convert.ToDecimal(dataRow.Player.Duration) / 60)).ToString();
                    }
                    else { seconds = Math.Floor(Convert.ToDecimal(dataRow.Player.Duration)).ToString(); }
                    if (seconds.Length == 1)
                    {
                        seconds = $"0{seconds}";
                    }
                    dataRow.Length = $"{minutes}:{seconds}";
                    if (dataRow.Player != null && dataRow.Player.Duration > 0) { dataRow.BitRate = Math.Floor(((Convert.ToDecimal(dataRow.Size) / 1000 * 8) / (Convert.ToDecimal(dataRow.Player.Duration)))).ToString(); }
                    decimal DRSize = Math.Floor(Convert.ToDecimal(dataRow.Size) / 1000);
                    if (DRSize > 1000)
                    {
                        dataRow.Size = $"{Math.Round(DRSize / 1000, 2)} MB";
                    }
                    else { dataRow.Size = $"{DRSize} kB"; }
                }
                
                FileDataRows.Add(dataRow); 
            };
            if (CategoryGroups.Count > 0) { CategoryGroups.Clear(); }
            GetCategoryGroup(CategoriesBaseFilePath);
            OnPropertyChanged("FileDataRows");
            OnPropertyChanged("CategoryGroups");
            OnPropertyChanged();
        }
        catch (Exception ex)
        { 
            Debug.WriteLine( ex );
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
            categoryGroup.AddCategoryText = "+";
            for (int i = CategoryGroups.Count - 1; i >= 0; i--)
            {
                if (CategoryGroups[i].Id >= categoryGroup.Id)
                {
                    CategoryGroups.RemoveAt(i);
                }
            }
            CategoryGroups.Add(categoryGroup);
            ActiveCatgoryFilePath = parentFilePath;
            if (ActiveCatgoryFilePath != null && ActiveCatgoryFilePath.Length > 0) 
            {
                for (int i = 0; i < CategoryGroups.Count; i++)
                {
                    for (int j = 0; j < CategoryGroups[i].Categories.Count; j++)
                    {
                        if (CategoryGroups[i].Categories[j].IsSelected == true)
                        {
                            CategoryGroups[i].Categories[j].IsSelected = false;
                        }
                        if (CategoryGroups[i].Categories[j].FilePath == parentFilePath)
                        {
                            CategoryGroup newCategoryGroup = CategoryGroups[i];
                            CategoryGroups.Remove(CategoryGroups[i]);
                            OnPropertyChanged(nameof(CategoryGroups));
                            CategoryGroups.Insert(i, newCategoryGroup);
                            CategoryGroups[i].Categories[j].IsSelected = true;
                            OnPropertyChanged(nameof(CategoryGroups));
                        }
                    }
                }
            }
            OnPropertyChanged("CategoryGroups");
            OnPropertyChanged();
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
            fileDirectory.Pk = await DBService.AddFileDirectory(fileDirectory.Name, fileDirectory.Path, "Category");
            if (fileDirectory.Pk != 0) {
                CategoriesBaseFilePath = fileDirectory.Path;
                OnPropertyChanged(nameof(CategoriesBaseFilePath));

                GetFiles(SourceFolders[0].FilePath);
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
            categoryGroup.AddCategoryText = "";
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
                            OnPropertyChanged("CategoryGroups");
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
    public void OnAllSelectedChanged()
    {
        try
        {
            if (AllSelected)
            {
                Collection<FileDataRow> fileDataRows = new Collection<FileDataRow>();
                for (int i = 0; i < FileDataRows.Count; i++)
                {
                    FileDataRows[i].IsSelected = true;
                    fileDataRows.Add(FileDataRows[i]);
                }
                FileDataRows.Clear();
                OnPropertyChanged(nameof(FileDataRows));
                for (int i = 0; i < fileDataRows.Count; i++)
                {
                    FileDataRows.Add(fileDataRows[i]);
                }
                OnPropertyChanged(nameof(FileDataRows));
            }
            else
            {
                Collection<FileDataRow> fileDataRows = new Collection<FileDataRow>();
                for (int i = 0; i < FileDataRows.Count; i++)
                {
                    FileDataRows[i].IsSelected = false;
                    fileDataRows.Add(FileDataRows[i]);
                }
                FileDataRows.Clear();
                OnPropertyChanged(nameof(FileDataRows));
                for (int i = 0; i < fileDataRows.Count; i++)
                {
                    FileDataRows.Add(fileDataRows[i]);
                }
                OnPropertyChanged(nameof(FileDataRows));
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
            Shell.Current.DisplayAlert("Error!", $"{ex.Message}", "OK");
        }
    }

    [RelayCommand]
    async Task MoveFiles()
    {
        try
        {
            List<FileDataRow> files = new List<FileDataRow>();
            for (int i = 0; i < FileDataRows.Count; i++)
            {
                if (FileDataRows[i].IsSelected)
                {
                    files.Add(FileDataRows[i]);
                }
            }
            await Task.Run(() =>
            {
                files.ForEach((file) => File.Move(file.FilePath, $"{ActiveCatgoryFilePath}\\{file.FileName}{file.Format}"));
                GetFiles(CurrentSourceFolderPath);
            });
            
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
                        files.Add(FileDataRows[i]);
                    }
                }
                files.ForEach((file) => File.Delete(file.FilePath));
                GetFiles(CurrentSourceFolderPath);
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
            if (fileDataRow.Player.IsPlaying)
            {
                fileDataRow.Player.Stop();
            }
            else
            {
                fileDataRow.Player.Volume = (VolumeLevel / 100);
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
