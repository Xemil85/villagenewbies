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
        private int? selectedAreaId;

        public AddCabinPage()
        {
            InitializeComponent();
            Mokit = new ObservableCollection<Mokki>();
            LataaAlueet();
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

            // Tarkistetaan, onko alue valittu
            if (AreaPicker.SelectedIndex == -1)
            {
                await DisplayAlert("Virhe", "Alue on valittava ennen mˆkin lis‰‰mist‰.", "Ok");
                return;
            }

            // Tarkista, ett‰ postinumero ei ole tyhj‰ ja on viisinumeroinen
            if (string.IsNullOrWhiteSpace(postinro.Text) || postinro.Text.Length != 5 || !int.TryParse(postinro.Text, out _))
            {
                await DisplayAlert("Virhe", "Postinumeron on oltava viisinumeroinen ja koostuttava vain numeroista.", "Ok");
                return;
            }

            // Tarkista, ett‰ hinta on oikein muotoiltu
            if (string.IsNullOrWhiteSpace(hinta.Text) ||
                !double.TryParse(hinta.Text, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out double parsedHinta))
            {
                await DisplayAlert("Virhe", "Hinnan on oltava luku ja koostuttava vain numeroista ja pisteest‰.", "Ok");
                return;
            }


            // Tarkista, ett‰ postinumero ei ole tyhj‰ ja on viisinumeroinen
            if (string.IsNullOrWhiteSpace(postinro.Text) || postinro.Text.Length != 5 || !int.TryParse(postinro.Text, out _))
            {
                await DisplayAlert("Virhe", "Postinumeron on oltava viisinumeroinen ja koostuttava vain numeroista.", "Ok");
                return;
            }


            // Tarkista, ett‰ postinumero ei ole tyhj‰ ja on viisinumeroinen
            if (!int.TryParse(henkilomaara.Text, out _))
            {
                await DisplayAlert("Virhe", "Henkilˆm‰‰r‰n on oltava kokonaisluku.", "Ok");
                return;
            }

            // Tarkistetaan, ett‰ kaikki muut kent‰t ovat t‰ytetty
            if (string.IsNullOrWhiteSpace(mokkinimi.Text) ||
                string.IsNullOrWhiteSpace(katuosoite.Text) ||
                string.IsNullOrWhiteSpace(kuvaus.Text) ||
                string.IsNullOrWhiteSpace(varustelu.Text))
            {
                await DisplayAlert("T‰ytt‰m‰ttˆm‰t tiedot", "T‰yt‰ kaikki tiedot ennen l‰hett‰mist‰.", "Ok");
                return;
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
            var selectedAreaName = _alueNimet[selectedAreaId.Value];
            // Tarkistetaan, onko postinumero jo olemassa
            bool postinumeroOlemassa = await databaseAccess.OnkoPostinumeroOlemassa(postinro.Text, selectedAreaName);
            if (!postinumeroOlemassa)
            {
                await databaseAccess.LisaaPostinumero(postinro.Text, selectedAreaName);
            }

            // Yritet‰‰n lis‰t‰ mˆkki tietokantaan
            bool success = await databaseAccess.LisaaMokkiTietokantaan(uusiMokki, selectedAreaName);

            if (success)
            {
                await DisplayAlert("Onnistui", "Mˆkin lis‰‰minen onnistui!", "Ok");
            }
            else
            {
                await DisplayAlert("Virhe", "Mˆkin lis‰‰minen ep‰onnistui. Yrit‰ uudelleen.", "Ok");
            }

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


        private async void OnAreaSelected(object sender, EventArgs e)
        {
            if (AreaPicker.SelectedIndex == -1)
            {
                selectedAreaId = null;
                return;
            }

            var selectedAreaName = AreaPicker.SelectedItem.ToString();
            selectedAreaId = _alueNimet.FirstOrDefault(x => x.Value == selectedAreaName).Key;

        }
    }


    public partial class DatabaseAccess
    {
        public async Task<bool> LisaaMokkiTietokantaan(Mokki uusiMokki, string toimipaikka)
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

                    var postinroString = uusiMokki.postinro.ToString();

                    // Lis‰‰ postinumero tietokantaan
                    await LisaaPostinumero(postinroString, toimipaikka);

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
                    // K‰sittely mahdollisille poikkeuksille
                    Debug.WriteLine(ex.Message);
                    return false;
                }
            }
        }


        public async Task<bool> OnkoPostinumeroOlemassa(string postinro, string toimipaikka)
        {
            string projectDirectory = System.AppDomain.CurrentDomain.BaseDirectory;
            var projectRoot = Path.GetFullPath(Path.Combine(projectDirectory, @"..\..\..\..\..\"));

            DotNetEnv.Env.Load(projectRoot);
            var env = Environment.GetEnvironmentVariables();

            string connectionString = $"server={env["SERVER"]};port={env["SERVER_PORT"]};database={env["SERVER_DATABASE"]};user={env["SERVER_USER"]};password={env["SERVER_PASSWORD"]}";
            using (var connection = new MySqlConnection(connectionString))
            {
                await connection.OpenAsync();
                var query = "SELECT COUNT(*) FROM posti WHERE postinro = @Postinro AND toimipaikka = @Toimipaikka";
                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Postinro", postinro);
                    command.Parameters.AddWithValue("@Toimipaikka", toimipaikka);
                    //command.Parameters.AddWithValue("@Toimipaikka", toimipaikka);

                    var tulos = Convert.ToInt32(await command.ExecuteScalarAsync());
                    return tulos > 0;
                }
            }
        }

        public async Task LisaaPostinumero(string postinro, string toimipaikka)
        {
            if (await OnkoPostinumeroOlemassa(postinro, toimipaikka))
            {
                return; // Jos postinumero on jo tietokannassa, ei tehd‰ mit‰‰n
            }

            string projectDirectory = System.AppDomain.CurrentDomain.BaseDirectory;
            var projectRoot = Path.GetFullPath(Path.Combine(projectDirectory, @"..\..\..\..\..\"));

            DotNetEnv.Env.Load(projectRoot);
            var env = Environment.GetEnvironmentVariables();

            string connectionString = $"server={env["SERVER"]};port={env["SERVER_PORT"]};database={env["SERVER_DATABASE"]};user={env["SERVER_USER"]};password={env["SERVER_PASSWORD"]}";

            using (var connection = new MySqlConnection(connectionString))
            {
                await connection.OpenAsync();
                var query = "INSERT INTO posti (postinro, toimipaikka) VALUES (@Postinro, @Toimipaikka)";
                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Postinro", postinro);
                    command.Parameters.AddWithValue("@Toimipaikka", toimipaikka);
                    await command.ExecuteNonQueryAsync();
                }
            }
        }
    }
}


