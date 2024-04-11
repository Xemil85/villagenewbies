namespace VillageNewbies;

public partial class BookingForm : ContentPage
{
	private Mokki _mokki;
    private Palvelu _palvelu;
    private List<Asiakas> _asiakkaat;
    private Dictionary<int, string> _asiakasNimet = new Dictionary<int, string>();
    private int? selectedAsiakasId;

    public BookingForm(Mokki mokki, Palvelu palvelu)
	{
		InitializeComponent();
        _mokki = mokki;
        _palvelu = palvelu;
        LataaAsiakkaat();
        TeeVaraus.Clicked += TeeVaraus_Clicked;
    }

    private void TeeVaraus_Clicked(object? sender, EventArgs e)
    {
        throw new NotImplementedException();
    }

    private async void LataaAsiakkaat()
    {
        var asiakkaatAccess = new MokkiAccess(); // Oletetaan, että tämä luokka hakee tietokannasta
        _asiakkaat = await asiakkaatAccess.FetchAllAsiakasAsync();

        // Muunna haetut alueet sanakirjaksi
        _asiakasNimet = _asiakkaat.ToDictionary(a => a.asiakas_id, a => a.etunimi + " " + a.sukunimi);
        Asiakaspicker.ItemsSource = _asiakasNimet.Values.ToList();

    }

    private void OnAsiakasSelected(object sender, EventArgs e)
    {
        if (Asiakaspicker.SelectedIndex == -1)
        {
            selectedAsiakasId = null;
            Sahkoposti.Text = "";
            Etunimi.Text = "";
            Sukunimi.Text = "";
            Lahiosoite.Text = "";
            Puhelin.Text = "";
            return;
        }

        var selectedAsiakasName = Asiakaspicker.SelectedItem.ToString();
        selectedAsiakasId = _asiakasNimet.FirstOrDefault(x => x.Value == selectedAsiakasName).Key;
        var selectedAsiakas = _asiakkaat[Asiakaspicker.SelectedIndex];

        Sahkoposti.Text = selectedAsiakas.email; // Oletetaan, että asiakas-objektilla on nämä kentät
        Etunimi.Text = selectedAsiakas.etunimi;
        Sukunimi.Text = selectedAsiakas.sukunimi;
        Lahiosoite.Text = selectedAsiakas.lahiosoite;
        Puhelin.Text = selectedAsiakas.puhelinnro;
    }
}