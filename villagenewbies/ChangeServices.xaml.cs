using CommunityToolkit.Maui.Views;
using static System.Net.Mime.MediaTypeNames;

namespace VillageNewbies;

public partial class ChangeServices : Popup
{
    private Palvelu _palvelu;

    public ChangeServices()
    {
        InitializeComponent();
        TallennaPalvelu.Clicked += TallennaPalvelu_Clicked;
    }

    private void TallennaPalvelu_Clicked(object? sender, EventArgs e)
    {
        // Päivitetyn tiedot tulee tähän.
    }

    public ChangeServices(Palvelu palvelu) : this()
	{
        _palvelu = palvelu;

        if (_palvelu != null)
        {
            palvelunimi.Text = _palvelu.nimi;
            Palvelutyyppi.Text = _palvelu.tyyppi.ToString();
            palvelukuvaus.Text = _palvelu.kuvaus;
            palveluhinta.Text = _palvelu.hinta.ToString();
        }
    }
}