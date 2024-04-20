using Microsoft.Maui.Controls;
using MySql.Data.MySqlClient;
using System;
using System.IO;
using System.Threading.Tasks;

namespace VillageNewbies
{
    public partial class EditCabinPage : ContentPage
    {
        private readonly Mokki _mokki;

        public EditCabinPage(Mokki mokki)
        {
            InitializeComponent();
            _mokki = mokki;
            PopulateFields();
        }

        private void PopulateFields()
        {
            if (_mokki != null)
            {
                mokkinimi.Text = _mokki.mokkinimi;
                katuosoite.Text = _mokki.katuosoite;
                postinro.Text = _mokki.postinro.ToString();
                hinta.Text = _mokki.hinta.ToString();
                kuvaus.Text = _mokki.kuvaus;
                henkilomaara.Text = _mokki.henkilomaara.ToString();
                varustelu.Text = _mokki.varustelu;
            }
        }

        private async void TallennaMokki_Clicked(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(mokkinimi.Text) ||
                string.IsNullOrWhiteSpace(katuosoite.Text) ||
                string.IsNullOrWhiteSpace(postinro.Text) ||
                !int.TryParse(postinro.Text, out int parsedPostinro) ||
                string.IsNullOrWhiteSpace(hinta.Text) ||
                !double.TryParse(hinta.Text, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double parsedHinta) ||
                string.IsNullOrWhiteSpace(henkilomaara.Text) ||
                !int.TryParse(henkilomaara.Text, out int parsedHenkilomaara) ||
                string.IsNullOrWhiteSpace(kuvaus.Text) ||
                string.IsNullOrWhiteSpace(varustelu.Text))
            {
                await DisplayAlert("Täyttämättömät tiedot", "Täytä kaikki tiedot ennen lähettämistä.", "Ok");
                return;
            }

            var muokattuMokki = new Mokki
            {
                mokki_id = _mokki.mokki_id,
                alue_id = _mokki.alue_id,
                mokkinimi = mokkinimi.Text,
                katuosoite = katuosoite.Text,
                postinro = int.Parse(postinro.Text),
                hinta = double.Parse(hinta.Text),
                kuvaus = kuvaus.Text,
                henkilomaara = int.Parse(henkilomaara.Text),
                varustelu = varustelu.Text
            };

            var success = await PaivitaMokinTiedot(muokattuMokki);
            if (success)
            {
                await DisplayAlert("Onnistui", "Mökin tiedot tallennettu onnistuneesti.", "OK");
                // Voit päivittää käyttöliittymän tai navigoida takaisin pääsivulle
            }
            else
            {
                await DisplayAlert("Virhe", "Mökin tietojen tallentaminen epäonnistui.", "OK");
            }
            await Navigation.PopAsync();
        }

        private async void PoistaMokki_Clicked(object sender, EventArgs e)
        {
            var vastaus = await DisplayAlert("Vahvista poisto", "Haluatko varmasti poistaa tämän Mökin?", "Kyllä", "Ei");
            if (vastaus)
            {
                bool success = await PoistaMokinTiedot(_mokki.mokki_id);
                if (success)
                {
                    await DisplayAlert("Poistettu", "Mökki on poistettu onnistuneesti.", "OK");
                    await Navigation.PopAsync();
                }
                else
                {
                    await DisplayAlert("Virhe", "Mökin poistaminen epäonnistui.", "OK");
                }
            }
        }

        private async Task<bool> PoistaMokinTiedot(int mokki_id)
        {
            string projectDirectory = System.AppDomain.CurrentDomain.BaseDirectory;
            var projectRoot = Path.GetFullPath(Path.Combine(projectDirectory, @"..\..\..\..\..\"));

            DotNetEnv.Env.Load(projectRoot);
            var env = Environment.GetEnvironmentVariables();

            string connectionString = $"server={env["SERVER"]};port={env["SERVER_PORT"]};database={env["SERVER_DATABASE"]};user={env["SERVER_USER"]};password={env["SERVER_PASSWORD"]}";
            using (var connection = new MySqlConnection(connectionString))
            {
                await connection.OpenAsync();
                var query = "DELETE FROM mokki WHERE mokki_id = @MokkiId";
                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@MokkiId", mokki_id);
                    var result = await command.ExecuteNonQueryAsync();
                    return result > 0; // palauttaa true jos yksi tai usempi rivi poistettiin
                }
            }
        }


        private async Task<bool> PaivitaMokinTiedot(Mokki muokattuMokki)
        {
            // Tässä suoritetaan tietokantapäivitys
            string projectDirectory = System.AppDomain.CurrentDomain.BaseDirectory;
            var projectRoot = Path.GetFullPath(Path.Combine(projectDirectory, @"..\..\..\..\..\"));

            DotNetEnv.Env.Load(projectRoot);
            var env = Environment.GetEnvironmentVariables();

            string connectionString = $"server={env["SERVER"]};port={env["SERVER_PORT"]};database={env["SERVER_DATABASE"]};user={env["SERVER_USER"]};password={env["SERVER_PASSWORD"]}";
            using (var connection = new MySqlConnection(connectionString))
            {
                await connection.OpenAsync();

                var query =
                    @"UPDATE mokki 
                SET alue_id = @AlueId,
                    mokkinimi = @Mokkinimi,
                    katuosoite = @Katuosoite,
                    postinro = @Postinro,
                    hinta = @Hinta,
                    kuvaus = @Kuvaus,
                    henkilomaara = @Henkilomaara,
                    varustelu = @Varustelu
                WHERE mokki_id = @MokkiId";

                using (var command = new MySqlCommand(query, connection))
                {
                    // Parametrien asettaminen
                    command.Parameters.AddWithValue("@MokkiId", muokattuMokki.mokki_id);
                    command.Parameters.AddWithValue("@AlueId", muokattuMokki.alue_id);
                    command.Parameters.AddWithValue("@Mokkinimi", muokattuMokki.mokkinimi);
                    command.Parameters.AddWithValue("@Katuosoite", muokattuMokki.katuosoite);
                    command.Parameters.AddWithValue("@Postinro", muokattuMokki.postinro);
                    command.Parameters.AddWithValue("@Hinta", muokattuMokki.hinta);
                    command.Parameters.AddWithValue("@Kuvaus", muokattuMokki.kuvaus);
                    command.Parameters.AddWithValue("@Henkilomaara", muokattuMokki.henkilomaara);
                    command.Parameters.AddWithValue("@Varustelu", muokattuMokki.varustelu);

                    var result = await command.ExecuteNonQueryAsync();
                    return result > 0; // Onnistuiko päivitys
                }
            }
        }
    }
}
