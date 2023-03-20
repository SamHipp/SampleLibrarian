using Sample_Librarian.ViewModel;
using System;
using System.IO;
using System.Collections;

namespace Sample_Librarian;

public partial class MainPage : ContentPage
{
	private MainViewModel viewModel;

	public MainPage(MainViewModel mainViewModel)
    {
        InitializeComponent();
        viewModel = mainViewModel;
		BindingContext = mainViewModel;

    }

    public void OnSelectAllChanged(object sender, CheckedChangedEventArgs e)
	{
		viewModel.OnAllSelectedChanged();

    }
}

