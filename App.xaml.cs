namespace Sample_Librarian;

public partial class App : Application
{
	public App()
	{
		InitializeComponent();

		MainPage = new AppShell();
	}

    protected override Window CreateWindow(IActivationState activationState)
    {
        var window = base.CreateWindow(activationState);

        const int newWidth = 1500;
        window.Width = newWidth;

        const int newHeight = 900;
        window.Height = newHeight;

        return window;
    }
}
