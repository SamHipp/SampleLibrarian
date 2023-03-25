using Sample_Librarian.ViewModel;
using Sample_Librarian.Services;
using System;
using System.IO;
using System.Collections;
using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Storage;
using System.Threading;
using Sample_Librarian.Model;

namespace Sample_Librarian;

public partial class MainPage : ContentPage
{
	private MainViewModel viewModel;

	public MainPage(MainViewModel mainViewModel)
    {
        InitializeComponent();
        viewModel = mainViewModel;
		BindingContext = mainViewModel;
        Task.Run(async () =>
        {
            await viewModel.OnAppStartup();
        });
    }

    public void OnSelectAllChanged(object sender, CheckedChangedEventArgs e)
	{
		viewModel.OnAllSelectedChanged();

    }
}

