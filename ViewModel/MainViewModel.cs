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

namespace Sample_Librarian.ViewModel;
public partial class MainViewModel : BaseViewModel
{
    FileDataRowService fileDataRowService;
    CategoryService categoryService;

    public ObservableCollection<FileDataRow> FileDataRows { get; set; } = new();

    public ObservableCollection<CategoryGroup> CategoryGroups { get; set; } = new();

    public string ActiveFilePath = null;

    [ObservableProperty]
    double volumeLevel = 0;

    public MainViewModel(FileDataRowService fileDataRowService, CategoryService categoryService)
    {
        this.fileDataRowService = fileDataRowService;
        this.categoryService = categoryService;
    }



    [ObservableProperty]
    string testString;


    [RelayCommand]
    async Task GetFiles()
    {
        try
        {
            List<FileDataRow> dataRows = await fileDataRowService.GetFileDataRows();
            FileDataRows.Clear();
            foreach (var dataRow in dataRows) {
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
                } else { dataRow.Size = $"{DRSize} kB"; }
                FileDataRows.Add(dataRow); 
            };
            if (CategoryGroups.Count > 0) { CategoryGroups.Clear(); }
            GetCategoryGroup("");
            OnPropertyChanged("FileDataRows");
            OnPropertyChanged("CategoryGroups");
            OnPropertyChanged();
        }
        catch (Exception ex)
        { 
            Debug.WriteLine( ex );
            await Shell.Current.DisplayAlert("Error!", $"{ex.Message}", "OK");
        }
    }

    [RelayCommand]
    public void GetCategoryGroup(string parentFilePath)
    {
        try
        {

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
            ActiveFilePath = parentFilePath;
            if (ActiveFilePath != null && ActiveFilePath.Length > 0) 
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
            files.ForEach((file) => File.Move(file.FilePath, $"{ActiveFilePath}\\{file.FileName}{file.Format}"));
            await GetFiles();
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
                List<FileDataRow> files = new List<FileDataRow>();
                for (int i = 0; i < FileDataRows.Count; i++)
                {
                    if (FileDataRows[i].IsSelected)
                    {
                        files.Add(FileDataRows[i]);
                    }
                }
                files.ForEach((file) => File.Delete(file.FilePath));
                await GetFiles();
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
