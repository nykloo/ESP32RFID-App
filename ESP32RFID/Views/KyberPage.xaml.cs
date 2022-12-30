namespace ESP32RFID;
using ViewModels;
public partial class KyberPage : ContentPage
{
	public KyberPage()
	{
        BindingContext = new KyberModeViewModel();
        InitializeComponent();
	}
}