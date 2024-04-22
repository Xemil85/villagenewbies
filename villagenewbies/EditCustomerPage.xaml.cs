using MySql.Data.MySqlClient;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Microsoft.Maui.Controls;
using System.Threading.Tasks;
using System.Data;

namespace VillageNewbies
{
    public partial class EditCustomerPage : ContentPage
    {
        private Asiakas _asiakas;

        public EditCustomerPage(Asiakas asiakas)
        {
            InitializeComponent();
            _asiakas = asiakas;

            if (_asiakas != null)
            {
                etunimiEntry.Text = _asiakas.etunimi;
                sukunimiEntry.Text = _asiakas.sukunimi;
                lahiosoiteEntry.Text = _asiakas.lahiosoite;
                postinumeroEntry.Text = _asiakas.postinro;
                toimipaikkaEntry.Text = _asiakas.toimipaikka;
                puhelinnumeroEntry.Text = _asiakas.puhelinnro;
                sahkopostiEntry.Text = _asiakas.email;
            }
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            if (_asiakas != null)
            {
                var databaseAccess = new DatabaseAccess();
                var toimipaikka = await databaseAccess.HaeToimipaikkaPostinronPerusteella(_asiakas.postinro);
                toimipaikkaEntry.Text = toimipaikka ?? "Ei saatavilla";
            }
        }


        private async void TallennaAsiakkaanTietoja_Clicked(object sender, EventArgs e)
        {
            // Päivitä asiakkaan tiedot uusilla tiedoilla
            _asiakas.etunimi = etunimiEntry.Text;
            _asiakas.sukunimi = sukunimiEntry.Text;
            _asiakas.lahiosoite = lahiosoiteEntry.Text;
            _asiakas.postinro = postinumeroEntry.Text;
            _asiakas.toimipaikka = toimipaikkaEntry.Text;
            _asiakas.puhelinnro = puhelinnumeroEntry.Text;
            _asiakas.email = sahkopostiEntry.Text;

            var databaseAccess = new DatabaseAccess();
            await databaseAccess.TallennaAsiakasTietokantaan(_asiakas);
            await DisplayAlert("Tiedot päivitetty", "Asiakkaan tiedot on päivitetty onnistuneesti.", "OK");

            // Poistu muokkaussivulta
            await Navigation.PopAsync();
        }

        private async void PoistaAsiakasTietokannasta_Clicked(object sender, EventArgs e)
        {
            // Toteuta asiakkaan poistaminen tietokannasta
            var vastaus = await DisplayAlert("Vahvista poisto", "Haluatko varmasti poistaa tämän asiakkaan?", "Kyllä", "Ei");
            if (vastaus)
            {
                var databaseAccess = new DatabaseAccess();
                await databaseAccess.PoistaAsiakasTietokannasta(_asiakas.asiakas_id);
                await DisplayAlert("Poistettu", "Asiakas on poistettu onnistuneesti.", "OK");
                await Navigation.PopAsync();
            }
        }
        public class DatabaseAccess
        {
            private string connectionString;

            public DatabaseAccess()
            {
                InitializeConnectionString();
            }

            private void InitializeConnectionString()
            {
                // Lataa ympäristömuuttujat tai määrittele tietokantayhteystiedot suoraan.
                string projectDirectory = System.AppDomain.CurrentDomain.BaseDirectory;
                var projectRoot = Path.GetFullPath(Path.Combine(projectDirectory, @"..\..\..\..\..\"));

                DotNetEnv.Env.Load(projectRoot);
                var env = Environment.GetEnvironmentVariables();

                connectionString = $"server={env["SERVER"]};port={env["SERVER_PORT"]};database={env["SERVER_DATABASE"]};user={env["SERVER_USER"]};password={env["SERVER_PASSWORD"]}";
            }

            public async Task TallennaAsiakasTietokantaan(Asiakas asiakas)
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

                        string tietokannanToimipaikka = await HaeToimipaikkaPostinronPerusteella(asiakas.postinro);
                        if (tietokannanToimipaikka == null)
                        {
                            // Jos postinumeroa ei löydy tietokannasta, lisätään se
                            await LisaaPostinumero(asiakas.postinro, asiakas.toimipaikka);
                        }
                        else if (!asiakas.toimipaikka.Equals(tietokannanToimipaikka, StringComparison.OrdinalIgnoreCase))
                        {
                            // Jos toimipaikka ei vastaa tietokannan toimipaikkaa, annetaan virheilmoitus
                            Debug.WriteLine("Annettu toimipaikka ei vastaa postinumeroa tietokannassa.");
                            //return false; // Voit myös heittää poikkeuksen tai palauttaa virhekoodin
                        }




                        
                        if (!await OnkoPostinumeroOlemassa(asiakas.postinro))
                        {
                            await LisaaPostinumero(asiakas.postinro, asiakas.toimipaikka);
                        }
                        
                        var query = @"UPDATE asiakas 
                                  SET etunimi = @etunimi, 
                                      sukunimi = @sukunimi, 
                                      lahiosoite = @lahiosoite, 
                                      postinro = @postinro, 
                                      email = @email, 
                                      puhelinnro = @puhelinnro 
                                  WHERE asiakas_id = @asiakas_id;";

                        using (var command = new MySqlCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@etunimi", asiakas.etunimi);
                            command.Parameters.AddWithValue("@sukunimi", asiakas.sukunimi);
                            command.Parameters.AddWithValue("@lahiosoite", asiakas.lahiosoite);
                            command.Parameters.AddWithValue("@postinro", asiakas.postinro);
                            command.Parameters.AddWithValue("@email", asiakas.email);
                            command.Parameters.AddWithValue("@puhelinnro", asiakas.puhelinnro);
                            command.Parameters.AddWithValue("@asiakas_id", asiakas.asiakas_id);

                            await command.ExecuteNonQueryAsync();
                        }
                        
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Virhe tietokantaan tallennettaessa: {ex.Message}");
                        
                    }
                }
            }

            public async Task<bool> OnkoPostinumeroOlemassa(string postinro)
            {
                string projectDirectory = System.AppDomain.CurrentDomain.BaseDirectory;
                var projectRoot = Path.GetFullPath(Path.Combine(projectDirectory, @"..\..\..\..\..\"));

                DotNetEnv.Env.Load(projectRoot);
                var env = Environment.GetEnvironmentVariables();

                string connectionString = $"server={env["SERVER"]};port={env["SERVER_PORT"]};database={env["SERVER_DATABASE"]};user={env["SERVER_USER"]};password={env["SERVER_PASSWORD"]}";
                using (var connection = new MySqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    var query = "SELECT COUNT(*) FROM posti WHERE postinro = @Postinro";
                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Postinro", postinro);

                        var tulos = Convert.ToInt32(await command.ExecuteScalarAsync());
                        return tulos > 0;
                    }
                }
            }

            public async Task LisaaPostinumero(string postinro, string toimipaikka)
            {
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

            
            public async Task PoistaAsiakasTietokannasta(int asiakasId)
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    try
                    {
                        await connection.OpenAsync();

                        // Ensin poistetaan kaikki varaukset, jotka viittaavat tähän asiakkaaseen
                        var deleteReservationsQuery = "DELETE FROM varaus WHERE asiakas_id = @AsiakasId";
                        using (var command = new MySqlCommand(deleteReservationsQuery, connection))
                        {
                            command.Parameters.AddWithValue("@AsiakasId", asiakasId);
                            await command.ExecuteNonQueryAsync();
                        }

                        // Sitten, kun kaikki viittaukset on poistettu, voit poistaa itse asiakkaan.
                        var deleteCustomerQuery = "DELETE FROM asiakas WHERE asiakas_id = @AsiakasId";
                        using (var command = new MySqlCommand(deleteCustomerQuery, connection))
                        {
                            command.Parameters.AddWithValue("@AsiakasId", asiakasId);
                            await command.ExecuteNonQueryAsync();
                        }
                    }
                    catch (MySqlException ex) when (ex.Number == 1451)
                    {
                        Debug.WriteLine("Ei voi poistaa asiakasta, koska siihen viitataan muissa tauluissa.");
                        // Tässä voit esimerkiksi näyttää käyttäjälle viestin, että poisto ei ole mahdollista.
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Virhe asiakkaan poistossa tietokannasta: {ex.Message}");
                        throw; // Heitä poikkeus ylemmälle tasolle, jos haluat käsitellä sitä siellä.
                    }
                }
            }


            public async Task<string> HaeToimipaikkaPostinronPerusteella(string postinro)
            {
                string toimipaikka = "";
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
                        var query = "SELECT toimipaikka FROM posti WHERE postinro = @postinro;";
                        using (var command = new MySqlCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@postinro", postinro);
                            using (var reader = await command.ExecuteReaderAsync())
                            {
                                if (await reader.ReadAsync())
                                {
                                    toimipaikka = reader.GetString("toimipaikka");
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        // Käsittely mahdollisille poikkeuksille
                        Console.WriteLine(ex.Message);
                    }
                }
                return toimipaikka;
            }
        }
    }
    }

