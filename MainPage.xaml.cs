using Sample_Librarian.ViewModel;
using System;
using System.IO;
using System.Collections;

namespace Sample_Librarian;

public partial class MainPage : ContentPage
{

	public MainPage(MainViewModel mainViewModel)
	{
		InitializeComponent();
		BindingContext = mainViewModel;

    }

	

    
}

