using MySql.Data.MySqlClient;
using System.Diagnostics;
using System.Globalization;
using VillageNewbies.Views;

namespace VillageNewbies;

public partial class BookingForm : ContentPage
{
	private Mokki _mokki;
    private Palvelu _palvelu;
    private string _aloitusPaiva;
    private string _lopetusPaiva;
    private List<Asiakas> _asiakkaat;
    private Dictionary<int, string> _asiakasNimet = new Dictionary<int, string>();
    private int? selectedAsiakasId;

    public BookingForm(Mokki mokki, Palvelu palvelu, string aloitusPaiva, string lopetusPaiva)
	{
		InitializeComponent();
        _mokki = mokki;
        _palvelu = palvelu;
        _aloitusPaiva = aloitusPaiva;
        _lopetusPaiva = lopetusPaiva;
        LataaAsiakkaat();
        string formattedNow = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
        TeeVaraus.Clicked += TeeVaraus_Clicked;
    }

    private async void TeeVaraus_Clicked(object? sender, EventArgs e)
    {
        DateTime aloitusPaiva = DateTime.Parse(_aloitusPaiva);
        DateTime lopetusPaiva = DateTime.Parse(_lopetusPaiva);

        // Otetaan aika TimePicker-komponentista
        TimeSpan aloitusAika = Aloitusaika.Time;
        TimeSpan lopetusAika = new TimeSpan(14, 0, 0); // Esimerkissä lopetusaika on kovakoodattu klo 14:00

        // Yhdistetään aloituspaivamaara ja -aika
        var varattuAlkupvm = new DateTime(aloitusPaiva.Year, aloitusPaiva.Month, aloitusPaiva.Day, aloitusAika.Hours, aloitusAika.Minutes, aloitusAika.Seconds);
        var varattuLoppupvm = new DateTime(lopetusPaiva.Year, lopetusPaiva.Month, lopetusPaiva.Day, lopetusAika.Hours, lopetusAika.Minutes, lopetusAika.Seconds);

        var uusiVaraus = new Varaus
        {
            asiakas_id = selectedAsiakasId.Value,
            mokki_id = _mokki.mokki_id,
            varattu_pvm = DateTime.Now.ToString("yyyy'-'MM'-'dd' 'HH':'mm':'ss"),
            vahvistus_pvm = DateTime.Now.ToString("yyyy'-'MM'-'dd' 'HH':'mm':'ss"),
            varattu_alkupvm = varattuAlkupvm.ToString("yyyy'-'MM'-'dd' 'HH':'mm':'ss"),
            varattu_loppupvm = varattuLoppupvm.ToString("yyyy'-'MM'-'dd' 'HH':'mm':'ss")
        };

        var databaseAccess = new DatabaseAccess();
        await databaseAccess.LisaaVarausTietokantaan(uusiVaraus);
        await DisplayAlert("Uusi varaus lisätty", "Uusi varaus on onnistuneesti lisätty.", "OK");

        Asiakaspicker.SelectedIndex = -1;
        Sahkoposti.Text = "";
        Etunimi.Text = "";
        Sukunimi.Text = "";
        Lahiosoite.Text = "";
        Puhelin.Text = "";

        await Navigation.PushAsync(new MainPage());
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

    public class DatabaseAccess
    {
        public async Task LisaaVarausTietokantaan(Varaus uusiVaraus)
        {
            string projectDirectory = System.AppDomain.CurrentDomain.BaseDirectory;
            var projectRoot = Path.GetFullPath(Path.Combine(projectDirectory, @"..\..\..\..\..\"));

            DotNetEnv.Env.Load(projectRoot);
            var env = Environment.GetEnvironmentVariables();

            string connectionString = $"server={env["SERVER"]};port={env["SERVER_PORT"]};database={env["SERVER_DATABASE"]};user={env["SERVER_USER"]};password={env["SERVER_PASSWORD"]}";
            using (var connection = new MySqlConnection(connectionString))
            {
                try
                {
                    await connection.OpenAsync();

                    var query = "INSERT INTO varaus (asiakas_id, mokki_mokki_id, varattu_pvm, vahvistus_pvm, varattu_alkupvm, varattu_loppupvm) VALUES (@Asiakas_id, @Mokki_id, @Varattu_pvm, @Vahvistus_pvm, @Varattu_alkupvm, @Varattu_loppupvm)";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Asiakas_id", uusiVaraus.asiakas_id);
                        command.Parameters.AddWithValue("@Mokki_id", uusiVaraus.mokki_id);
                        command.Parameters.AddWithValue("@Varattu_pvm", uusiVaraus.varattu_pvm);
                        command.Parameters.AddWithValue("@Vahvistus_pvm", uusiVaraus.vahvistus_pvm);
                        command.Parameters.AddWithValue("@Varattu_alkupvm", uusiVaraus.varattu_alkupvm);
                        command.Parameters.AddWithValue("@Varattu_loppupvm", uusiVaraus.varattu_loppupvm);

                        await command.ExecuteNonQueryAsync();
                    }
                }
                catch (Exception ex)
                {
                    // Käsittely mahdollisille poikkeuksille
                    Debug.WriteLine(ex.Message);
                }
            }
        }
    }
}