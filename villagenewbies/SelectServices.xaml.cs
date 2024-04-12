using System.Collections.ObjectModel;

namespace VillageNewbies;

public partial class SelectServices : ContentPage
{
    public ObservableCollection<Palvelu> Palvelut { get; private set; }
    private List<Palvelu> selectedServices = new List<Palvelu>();
    private Mokki _mokki;
    private string _aloitusPaiva;
    private string _lopetusPaiva;
    public SelectServices(Mokki mokki, string aloitusPaiva, string lopetusPaiva)
	{
		InitializeComponent();
		_mokki = mokki;
        _aloitusPaiva = aloitusPaiva;
        _lopetusPaiva = lopetusPaiva;
        Palvelut = new ObservableCollection<Palvelu>();
        ServicesCollectionView.ItemsSource = Palvelut;
        LoadPalvelut();
	}

    private async Task LoadPalvelut()
    {
        //TODO Toteuta oikea tietokantahaku
        var mokkiAccess = new MokkiAccess();
        var palveluList = await mokkiAccess.FetchAllPalveluWithAlueAsync(_mokki.alue_id);
        MainThread.InvokeOnMainThreadAsync(() =>
        {
            foreach (var palvelu in palveluList)
            {
                Palvelut.Add(palvelu);
            }
            ServicesCollectionView.ItemsSource = Palvelut;
        });
    }

    private void CheckBox_CheckedChanged(object sender, CheckedChangedEventArgs e)
    {
        if (!(sender is CheckBox checkBox)) return;

        var palvelu = checkBox.BindingContext as Palvelu;
        if (palvelu == null) return;

        if (e.Value)
        {
            if (!selectedServices.Contains(palvelu))
            {
                selectedServices.Add(palvelu);
            }
        }
        else
        {
            if (selectedServices.Contains(palvelu))
            {
                selectedServices.Remove(palvelu);
            }
        }
    }


    private async void Valitsepalvelu(object sender, EventArgs e)
    {
       
        await Navigation.PushAsync(new BookingForm(_mokki, selectedServices, _aloitusPaiva, _lopetusPaiva));
    }
}