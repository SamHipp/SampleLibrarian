using CommunityToolkit.Mvvm.ComponentModel;

namespace Sample_Librarian.ViewModel;

public partial class BaseViewModel : ObservableObject
{
    [ObservableProperty]
    bool isBusy;
}

