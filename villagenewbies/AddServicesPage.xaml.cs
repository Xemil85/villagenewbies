using MySql.Data.MySqlClient;
using System.Diagnostics;

namespace VillageNewbies;

public partial class AddServicesPage : ContentPage
{
    private Dictionary<int, string> _alueNimet = new Dictionary<int, string>();
    private int? selectedAreaId;
    public AddServicesPage()
	{
		InitializeComponent();
        //AreaPicker.ItemsSource = ladatutAlueet.Select(a => a.nimi).ToList();
        Task.Run(LataaAlueet);
        Lisaapalvelu.Clicked += Lisaapalvelu_Clicked;
        LataaAlueet();
    }

    private async void LataaAlueet()
    {
        var alueetAccess = new MokkiAccess(); // Oletetaan, että tämä luokka hakee tietokannasta
        var alueet = await alueetAccess.FetchAllAlueAsync();

        // Muunna haetut alueet sanakirjaksi
        _alueNimet = alueet.ToDictionary(a => a.alue_id, a => a.nimi);
        AreaPicker.ItemsSource = _alueNimet.Values.ToList();
        
    }

    private async void Lisaapalvelu_Clicked(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(palvelunimi.Text) ||
        string.IsNullOrWhiteSpace(palvelukuvaus.Text) ||
        string.IsNullOrWhiteSpace(palveluhinta.Text))
        {
            // Näytä varoitusikkuna
            await DisplayAlert("Täyttämättömät tiedot", "Täytä kaikki palvelutiedot ennen lähettämistä.", "OK");
            return; // Lopeta metodin suoritus tähän
        }


        var uusiPalvelu = new Palvelu
        {
            alue_id = selectedAreaId.Value,
            nimi = palvelunimi.Text,
            tyyppi = int.Parse(Palvelutyyppi.Text),
            kuvaus = palvelukuvaus.Text,
            hinta = double.Parse(palveluhinta.Text),
            alv = 24.00
        };

        var databaseAccess = new DatabaseAccess();
        await databaseAccess.LisaaPalveluTietokantaan(uusiPalvelu);

        AreaPicker.SelectedIndex = -1;
        palvelunimi.Text = "";
        Palvelutyyppi.Text = "";
        palvelukuvaus.Text = "";
        palveluhinta.Text = "";
    }

    private void OnAreaSelected(object sender, EventArgs e)
    {
        if (AreaPicker.SelectedIndex == -1)
        {
            selectedAreaId = null;
            return;
        }

        var selectedAreaName = AreaPicker.SelectedItem.ToString();
        selectedAreaId = _alueNimet.FirstOrDefault(x => x.Value == selectedAreaName).Key;
    }

    public class DatabaseAccess
    {
        public async Task LisaaPalveluTietokantaan(Palvelu uusiPalvelu)
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

                    var query = "INSERT INTO palvelu (alue_id, nimi, tyyppi, kuvaus, hinta, alv) VALUES (@Alue_id, @Nimi, @Tyyppi, @Kuvaus, @Hinta, @Alv)";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Alue_id", uusiPalvelu.alue_id);
                        command.Parameters.AddWithValue("@Nimi", uusiPalvelu.nimi);
                        command.Parameters.AddWithValue("@Tyyppi", uusiPalvelu.tyyppi);
                        command.Parameters.AddWithValue("@Kuvaus", uusiPalvelu.kuvaus);
                        command.Parameters.AddWithValue("@Hinta", uusiPalvelu.hinta);
                        command.Parameters.AddWithValue("@Alv", uusiPalvelu.alv);

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