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

    readonly Animation rotation;

	public MainPage(MainViewModel mainViewModel)
    {
        InitializeComponent();
        viewModel = mainViewModel;
		BindingContext = mainViewModel;
        Task.Run(async () =>
        {
            await viewModel.OnAppStartup();
        });
        rotation = new Animation(v => FDRLoadingEllipse.Rotation = v, 0, 360, Easing.Linear);
        viewModel.PropertyChanged += ViewModel_PropertyChanged;
    }

    private void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(viewModel.IsFileDataRowsNotLoaded))
        {
            if (viewModel.IsFileDataRowsNotLoaded == true)
            {
                rotation.Commit(this, "rotate", 16, 1000, Easing.Linear, (v, c) => FDRLoadingEllipse.Rotation = 0, () => true);
            } else
            {
                this.AbortAnimation("rotate");
            }
        }
    }

    public void OnSelectAllChanged(object sender, CheckedChangedEventArgs e)
	{
		viewModel.OnAllSelectedChanged();

    }
}

