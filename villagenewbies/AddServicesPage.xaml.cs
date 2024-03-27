using MySql.Data.MySqlClient;
using System.Diagnostics;

namespace VillageNewbies;

public partial class AddServicesPage : ContentPage
{
    private readonly Dictionary<int, string> _alueNimet = new Dictionary<int, string>
        {
            { 1, "Ylläs" },
            { 2, "Ruka" },
            { 3, "Pyhä" },
            { 4, "Levi" },
            { 5, "Syöte" },
            { 6, "Vuokatti" },
            { 7, "Tahko" },
            { 8, "Himos" },
        };
    public AddServicesPage()
	{
		InitializeComponent();
        AreaPicker.ItemsSource = _alueNimet.Values.ToList();
        Lisaapalvelu.Clicked += Lisaapalvelu_Clicked;
    }

    private async void Lisaapalvelu_Clicked(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(palvelunimi.Text) ||
        string.IsNullOrWhiteSpace(palvelukuvaus.Text) ||
        string.IsNullOrWhiteSpace(palveluhinta.Text))
        {
            // Näytä varoitusikkuna
            await DisplayAlert("Täyttämättömät tiedot", "Täytä kaikki asiakastiedot ennen lähettämistä.", "OK");
            return; // Lopeta metodin suoritus tähän
        }


        var uusiPalvelu = new Palvelu
        {
            alue_id = AreaPicker.SelectedIndex,
            nimi = palvelunimi.Text,
            kuvaus = palvelukuvaus.Text,
            hinta = double.Parse(palveluhinta.Text),
            alv = 24.00
        };

        var databaseAccess = new DatabaseAccess();
        await databaseAccess.LisaaPalveluTietokantaan(uusiPalvelu);
    }

    private void OnAreaSelected(object sender, EventArgs e)
    {
        if (AreaPicker.SelectedIndex == -1)
            return;

        var selectedAreaName = AreaPicker.SelectedItem.ToString();
        int selectedAreaId = _alueNimet.FirstOrDefault(x => x.Value == selectedAreaName).Key;
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

                    var query = "INSERT INTO palvelu (alue_id, nimi, kuvaus, hinta, alv) VALUES (@Alue_id, @Nimi, @Kuvaus, @Hinta, @Alv)";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Alue_id", uusiPalvelu.alue_id);
                        command.Parameters.AddWithValue("@Nimi", uusiPalvelu.nimi);
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