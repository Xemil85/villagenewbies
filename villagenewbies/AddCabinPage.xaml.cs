using Microsoft.Maui.Controls;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace VillageNewbies
{
    public partial class AddCabinPage : ContentPage
    {
        private DatabaseAccess databaseAccess = new DatabaseAccess();
        public ObservableCollection<Mokki> Mokit { get; private set; }
        private Mokki _mokki;
        private Dictionary<int, string> _alueNimet = new Dictionary<int, string>();
        private Dictionary<string, string> _aluePostinumerot = new Dictionary<string, string>()
        {
            {"Ylläs", "95980"},
            {"Ruka", "93830"},
            {"Pyhä", "98530"},
            {"Levi", "99130"},
            {"Syöte", "93280"},
            {"Vuokatti", "88610"},
            {"Tahko", "73310"},
            {"Himos", "42100"}
        };
        private int? selectedAreaId;
        private bool postinroUpdatedAutomatically = false;

        public AddCabinPage()
        {
            InitializeComponent();
            Mokit = new ObservableCollection<Mokki>();
            Task.Run(LataaAlueet);
            LataaAlueet();

            // Lisätään käsittelijä postinumero-kentälle
            postinro.TextChanged += Postinro_TextChanged;
        }

        public AddCabinPage(Mokki mokki) : this()
        {
            _mokki = mokki;


            if (_mokki != null)
            {
                AreaPicker.SelectedIndex = _mokki.alue_id;
                mokkinimi.Text = _mokki.mokkinimi;
                katuosoite.Text = _mokki.katuosoite;
                postinro.Text = _mokki.postinro.ToString();
                hinta.Text = _mokki.hinta.ToString();
                kuvaus.Text = _mokki.kuvaus;
                henkilomaara.Text = _mokki.henkilomaara.ToString();
                varustelu.Text = _mokki.varustelu.ToString();
            }
        }

        private async void LisaaMokki_Clicked(object sender, EventArgs e)
        {
            // jos kentät tyhjät ja yritetään tallentaa
            if
                (
                AreaPicker.SelectedIndex == -1 ||
                string.IsNullOrWhiteSpace(mokkinimi.Text) ||
                string.IsNullOrWhiteSpace(katuosoite.Text) ||
                string.IsNullOrWhiteSpace(postinro.Text) ||
                !int.TryParse(postinro.Text, out int parsedPostinro) ||
                string.IsNullOrWhiteSpace(hinta.Text) ||
                !double.TryParse(hinta.Text, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double parsedHinta) ||
                string.IsNullOrWhiteSpace(henkilomaara.Text) ||
                !int.TryParse(henkilomaara.Text, out int parsedHenkilomaara) ||
                string.IsNullOrWhiteSpace(kuvaus.Text) ||
                string.IsNullOrWhiteSpace(varustelu.Text))

            {// Näytä virheviesti tai käsittele virhetilanne tässä
                await DisplayAlert("Täyttämättömät tiedot", "Täytä kaikki tiedot ennen lähettämistä.", "Ok");
                return; // Lopeta metodin suoritus tähän
            }

            var uusiMokki = new Mokki
            {

                alue_id = selectedAreaId.Value,
                mokkinimi = mokkinimi.Text,
                katuosoite = katuosoite.Text,
                postinro = int.Parse(postinro.Text),
                hinta = double.Parse(hinta.Text),
                kuvaus = kuvaus.Text,
                henkilomaara = int.Parse(henkilomaara.Text),
                varustelu = varustelu.Text
            };

            var databaseAccess = new DatabaseAccess();
            bool success = await databaseAccess.LisaaMokkiTietokantaan(uusiMokki);

            if (success)
            {
                await DisplayAlert("Onnistui", "Mökin lisääminen onnistui!", "Ok");
            }
            else
            {
                await DisplayAlert("Virhe", "Mökin lisääminen epäonnistui. Yritä uudelleen.", "Ok");
            }

            AreaPicker.SelectedIndex = -1;
            mokkinimi.Text = "";
            katuosoite.Text = "";
            postinro.Text = "";
            hinta.Text = "";
            kuvaus.Text = "";
            henkilomaara.Text = "";
            varustelu.Text = "";

            await Navigation.PopAsync();
        }

        private async void LataaAlueet()
        {
            var alueetAccess = new MokkiAccess();
            var alueet = await alueetAccess.FetchAllAlueAsync();

            // Muunna haetut alueet sanakirjaksi
            _alueNimet = alueet.ToDictionary(a => a.alue_id, a => a.nimi);
            AreaPicker.ItemsSource = _alueNimet.Values.ToList();

        }

        private void Postinro_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Jos postinumero on päivitetty automaattisesti alueen valinnan perusteella,
            // estetään käyttäjää muokkaamasta sitä manuaalisesti.
            if (postinroUpdatedAutomatically)
            {
                postinroUpdatedAutomatically = false;
                return;
            }

            // Jos postinumeroa ei päivitetty automaattisesti, estetään sen muokkaaminen.
            // Asetetaan postinumero takaisin valitun alueen postinumeroksi.
            if (AreaPicker.SelectedIndex != -1)
            {
                var selectedAreaName = AreaPicker.SelectedItem.ToString();
                if (_aluePostinumerot.ContainsKey(selectedAreaName))
                {
                    postinroUpdatedAutomatically = true;
                    postinro.Text = _aluePostinumerot[selectedAreaName];
                }
            }
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

            // Aseta automaattisesti valitun alueen postinumero postinumero-kenttään
            if (_aluePostinumerot.ContainsKey(selectedAreaName))
            {
                postinroUpdatedAutomatically = true;
                postinro.Text = _aluePostinumerot[selectedAreaName];
            }
        }

        public partial class DatabaseAccess
        {
            public async Task<bool> LisaaMokkiTietokantaan(Mokki uusiMokki)
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

                        var query = "INSERT INTO mokki (mokki_id, alue_id, mokkinimi, katuosoite, postinro, hinta, kuvaus,henkilomaara, varustelu)  VALUES (@Mokki_id, @Alue_id, @Mokkinimi, @Katuosoite, @Postinro, @Hinta, @Kuvaus, @Henkilomaara, @Varustelu)";

                        using (var command = new MySqlCommand(query, connection))
                        {
                            Debug.WriteLine(uusiMokki.postinro);
                            command.Parameters.AddWithValue("@Mokki_id", uusiMokki.mokki_id);
                            command.Parameters.AddWithValue("@Alue_id", uusiMokki.alue_id);
                            command.Parameters.AddWithValue("@Mokkinimi", uusiMokki.mokkinimi);
                            command.Parameters.AddWithValue("@Katuosoite", uusiMokki.katuosoite);
                            command.Parameters.AddWithValue("@Postinro", uusiMokki.postinro);
                            command.Parameters.AddWithValue("@Hinta", uusiMokki.hinta);
                            command.Parameters.AddWithValue("@Henkilomaara", uusiMokki.henkilomaara);
                            command.Parameters.AddWithValue("@Kuvaus", uusiMokki.kuvaus);
                            command.Parameters.AddWithValue("@Varustelu", uusiMokki.varustelu);
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
