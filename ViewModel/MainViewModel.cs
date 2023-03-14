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

namespace Sample_Librarian.ViewModel;
public partial class MainViewModel : BaseViewModel
{
    FileDataRowService fileDataRowService;

    public ObservableCollection<FileDataRow> FileDataRows { get; set; } = new();



    public MainViewModel(FileDataRowService fileDataRowService)
    {
        this.fileDataRowService = fileDataRowService;
    }



    [ObservableProperty]
    string testString;


    [RelayCommand]
    void ChangeText()
    {
        try
        {
            List<FileDataRow> dataRows = fileDataRowService.GetFileDataRows();
            FileDataRows.Clear();
            foreach (var dataRow in dataRows) { FileDataRows.Add(dataRow); };
            OnPropertyChanged("FileDataRows");
            OnPropertyChanged();
        }
        catch (Exception ex)
        { 
            Debug.WriteLine( ex );
            Shell.Current.DisplayAlert("Error!", $"{ex.Message}", "OK");
        }
        finally 
        {
            Debug.WriteLine("Worked");
        }
    }
}
