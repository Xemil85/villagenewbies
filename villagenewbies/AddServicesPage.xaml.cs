using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace VillageNewbies 
{
    public partial class AddServicesPage : ContentPage
    {
        private Dictionary<int, string> _alueNimet = new Dictionary<int, string>();
        private Dictionary<int, string> _tyyppiNimet = new Dictionary<int, string>();
        private int? selectedAreaId;
        private int? selectedTypeId;

        public AddServicesPage()
        {
            InitializeComponent();
            //AreaPicker.ItemsSource = ladatutAlueet.Select(a => a.nimi).ToList();
            Lisaapalvelu.Clicked += Lisaapalvelu_Clicked;
            LataaAlueet();
            lataaTyypit();
        }

        private async void LataaAlueet()
        {
            var alueetAccess = new MokkiAccess(); 
            var alueet = await alueetAccess.FetchAllAlueAsync();

            // Muunna haetut alueet sanakirjaksi
            _alueNimet = alueet.ToDictionary(a => a.alue_id, a => a.nimi);
            AreaPicker.ItemsSource = _alueNimet.Values.ToList();
        }

        private async void lataaTyypit()
        {
            var tyyppiAccess = new MokkiAccess(); 
            var tyypit = await tyyppiAccess.FetchAllPalveluTyypitAsync();

            _tyyppiNimet = tyypit.ToDictionary(a => a.tyyppi, a => a.nimi);
            TypePicker.ItemsSource = _tyyppiNimet.Values.ToList();
        }

    private async void Lisaapalvelu_Clicked(object? sender, EventArgs e)
    {
        if (AreaPicker.SelectedIndex == -1 ||
            string.IsNullOrWhiteSpace(palvelunimi.Text) ||
            TypePicker.SelectedIndex == -1 ||
            string.IsNullOrWhiteSpace(palvelukuvaus.Text) ||
            string.IsNullOrWhiteSpace(palveluhinta.Text))
        {
            // Näytä varoitusikkuna
            await DisplayAlert("Täyttämättömät tiedot", "Täytä kaikki palvelutiedot ennen lähettämistä.", "OK");
            return; // Lopeta metodin suoritus tähän
        }

        if (!double.TryParse(palveluhinta.Text, out double hinta))
        {
            await DisplayAlert("Virhe", "Hinta on virheellinen. Hinnassa ei saa olla kirjaimia", "OK");
            return;
        }

        var uusiPalvelu = new Palvelu
        {
            alue_id = selectedAreaId.Value,
            nimi = palvelunimi.Text,
            tyyppi = selectedTypeId.Value,
            kuvaus = palvelukuvaus.Text,
            hinta = hinta,
            alv = 24.00
        };

            var databaseAccess = new DatabaseAccess();
            bool success = await databaseAccess.LisaaPalveluTietokantaan(uusiPalvelu);

            if (success)
            {
                await DisplayAlert("Palvelu lisätty", "Uusi palvelu on onnistuneesti lisätty.", "OK");
            }
            else
            {
                await DisplayAlert("Virhe", "Palvelun lisääminen epäonnistui.", "OK");
            }

            // Tyhjennetään kentät ja siirrytään takaisin
            AreaPicker.SelectedIndex = -1;
            palvelunimi.Text = "";
            TypePicker.SelectedIndex = -1;
            palvelukuvaus.Text = "";
            palveluhinta.Text = "";

            await Navigation.PopAsync(); // Palataan edelliselle sivulle
        }

        private void OnAreaSelected(object sender, EventArgs e) // Lisää cabinsivulle
        {
            if (AreaPicker.SelectedIndex == -1)
            {
                selectedAreaId = null;
                return;
            }

            var selectedAreaName = AreaPicker.SelectedItem.ToString();
            selectedAreaId = _alueNimet.FirstOrDefault(x => x.Value == selectedAreaName).Key;
        }

        private void OnTypeSelected(object sender, EventArgs e)
        {
            if (TypePicker.SelectedIndex == -1)
            {
                selectedTypeId = null;
                return;
            }

            var selectedTypeName = TypePicker.SelectedItem.ToString();
            selectedTypeId = _tyyppiNimet.FirstOrDefault(x => x.Value == selectedTypeName).Key;
        }

        public class DatabaseAccess
        {
            public async Task<bool> LisaaPalveluTietokantaan(Palvelu uusiPalvelu)
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
                            return true;
                        }
                    }
                    catch (Exception ex)
                    {
                        // Käsittely mahdollisille poikkeuksille
                        Debug.WriteLine(ex.Message);
                        return false;
                    }
                }
            }
        }
    }
}
