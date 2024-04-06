using Microsoft.Maui.Controls;
using MySql.Data.MySqlClient;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace VillageNewbies;

public partial class AddCabinPage : ContentPage
{
    private DatabaseAccess databaseAccess = new DatabaseAccess();
    public ObservableCollection<Mokki> Mokit { get; private set; }
    private Mokki _mokki;

    public AddCabinPage()
    {
        InitializeComponent();
       
        Mokit = new ObservableCollection<Mokki>();       
    }

    public AddCabinPage(Mokki mokki) : this()
    {
        _mokki = mokki;
        // Täytä kentät mökin tiedoilla

        if (_mokki != null)
        {
            mokki_id.Text = _mokki.ToString();
            alue_id.Text = _mokki.ToString();
            mokkinimi.Text = _mokki.mokkinimi;
            katuosoite.Text = _mokki.katuosoite;
            postinro.Text = _mokki.postinro.ToString(); // Olettaen että postinro on tietotyyppiä, joka vaatii muunnoksen stringiksi
            hinta.Text = _mokki.hinta.ToString();
            henkilomaara.Text = _mokki.henkilomaara.ToString();
            kuvaus.Text = _mokki.kuvaus;
            varustelu.Text = _mokki.varustelu.ToString();
        }
    }

    private async void LisaaMokki_Clicked(object sender, EventArgs e)
    {
        // jos kentät tyhjät ja yritetään tallentaa
        if (string.IsNullOrWhiteSpace(mokki_id.Text) ||
            !int.TryParse(mokki_id.Text, out int parsedMokki_id) ||
            string.IsNullOrWhiteSpace(alue_id.Text) ||
            !int.TryParse(alue_id.Text, out int parsedAlue_id) ||
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
            mokki_id = int.Parse(mokki_id.Text),
            alue_id = int.Parse(alue_id.Text),
            mokkinimi = mokkinimi.Text,
            katuosoite = katuosoite.Text,
            postinro = int.Parse(postinro.Text),
            hinta = double.Parse(hinta.Text),
            henkilomaara = int.Parse(henkilomaara.Text),
            kuvaus = kuvaus.Text,
            varustelu = varustelu.Text
        };

        var databaseAccess = new DatabaseAccess();
        await databaseAccess.LisaaMokkiTietokantaan(uusiMokki);


        mokki_id.Text = "";
        alue_id.Text = "";
        mokkinimi.Text = "";
        katuosoite.Text = "";
        postinro.Text = "";
        hinta.Text = "";
        henkilomaara.Text = "";
        kuvaus.Text = "";
        varustelu.Text = "";

        await Navigation.PopAsync();
        

    }

    private async void TallennaMokki_Clicked(object sender, EventArgs e)
    {
        // Oletetaan, että olet luonut muokattavaMokki-olion johonkin sopivaan kohtaan luokassa ja se on alustettu olemassa olevilla tiedoilla.
        // Nyt, oletetaan myös, että käyttöliittymäkentät on sidottu tähän muokattavaMokki-olioon kaksisuuntaisella sidoksella.

        // Jos jokin kentistä on tyhjä tai ei mene läpi tietotyypin tarkistuksesta, näytetään virheviesti
        // Päivitä muokattavaMokki-olion ominaisuuksia tarpeen mukaan.
        // Oletetaan, että käyttöliittymän kontrollit on sidottu muokattavaMokki-olioon.
        // Varmista ensin, että mokki_id.Text on kokonaisluku
        var muokattavaMokki = new Mokki();
        /*{
            mokki_id = int.Parse(mokki_id.Text),
            alue_id = int.Parse(alue_id.Text),
            mokkinimi = mokkinimi.Text,
            katuosoite = katuosoite.Text,
            postinro = int.Parse(postinro.Text),
            hinta = double.Parse(hinta.Text),
            henkilomaara = int.Parse(henkilomaara.Text),
            kuvaus = kuvaus.Text,
            varustelu = varustelu.Text
        };*/

        if (!string.IsNullOrWhiteSpace(mokki_id.Text) && int.TryParse(mokki_id.Text, out int parsedMokkiId))
        {
            muokattavaMokki.mokki_id = parsedMokkiId;
        }

        // Tarkista alue_id ja päivitä, jos se on annettu ja se on kokonaisluku
        if (!string.IsNullOrWhiteSpace(alue_id.Text) && int.TryParse(alue_id.Text, out int parsedAlueId))
        {
            muokattavaMokki.alue_id = parsedAlueId;
        }

        // Päivitä mokkinimi, jos se on annettu
        if (!string.IsNullOrWhiteSpace(mokkinimi.Text))
        {
            muokattavaMokki.mokkinimi = mokkinimi.Text;
        }

        // Päivitä katuosoite, jos se on annettu
        if (!string.IsNullOrWhiteSpace(katuosoite.Text))
        {
            muokattavaMokki.katuosoite = katuosoite.Text;
        }

        // Varmista, että postinro on kokonaisluku ja päivitä
        if (!string.IsNullOrWhiteSpace(postinro.Text) && int.TryParse(postinro.Text, out int parsedPostinro))
        {
            muokattavaMokki.postinro = parsedPostinro;
        }

        // Varmista, että hinta on desimaaliluku ja päivitä
        if (!string.IsNullOrWhiteSpace(hinta.Text) && double.TryParse(hinta.Text, out double parsedHinta))
        {
            muokattavaMokki.hinta = parsedHinta;
        }

        // Varmista, että henkilomaara on kokonaisluku ja päivitä
        if (!string.IsNullOrWhiteSpace(henkilomaara.Text) && int.TryParse(henkilomaara.Text, out int parsedHenkilomaara))
        {
            muokattavaMokki.henkilomaara = parsedHenkilomaara;
        }

        // Päivitä kuvaus, jos se on annettu
        if (!string.IsNullOrWhiteSpace(kuvaus.Text))
        {
            muokattavaMokki.kuvaus = kuvaus.Text;
        }

        // Päivitä varustelu, jos se on annettu
        if (!string.IsNullOrWhiteSpace(varustelu.Text))
        {
            muokattavaMokki.varustelu = varustelu.Text;
        }


        // Tarkista, onko jotain muutettu, ennen kuin yritetään päivittää tietokantaan.
        /*if (muokattavaMokki.IsDirty) // Oletetaan, että on IsDirty-ominaisuus, joka kertoo, onko oliota muokattu.
            
                var success = await databaseAccess.PaivitaMokinTiedot(muokattavaMokki);
                if (success)
                {
                    await DisplayAlert("Onnistui", "Mökin tiedot tallennettu onnistuneesti.", "OK");
                    // Tässä kohtaa voit päivittää käyttöliittymän tai navigoida takaisin pääsivulle.
                }
                else 
                {
                    await DisplayAlert("Virhe", "Mökin tietojen tallentaminen epäonnistui.", "OK");
                }
            
            else
            {
                await DisplayAlert("Ei muutoksia", "Ei tehty muutoksia tallennettavaksi.", "OK");
            }*/

        // Päivitetään muokattavaMokki-olion tiedot käyttöliittymästä saaduilla tiedoilla
       /* muokattavaMokki.mokki_id = int.Parse(mokki_id.Text);
        muokattavaMokki.alue_id = int.Parse(alue_id.Text);
        muokattavaMokki.mokkinimi = mokkinimi.Text;
        muokattavaMokki.katuosoite = katuosoite.Text;
        muokattavaMokki.postinro = int.Parse(postinro.Text);
        muokattavaMokki.hinta = double.Parse(hinta.Text);
        muokattavaMokki.henkilomaara = int.Parse(henkilomaara.Text);
        muokattavaMokki.kuvaus = kuvaus.Text;
        muokattavaMokki.varustelu = varustelu.Text;*/

        // Kutsutaan DatabaseAccess-luokan päivitysmetodia
        var success = await databaseAccess.PaivitaMokinTiedot(muokattavaMokki);
        if (success)
        {
            await DisplayAlert("Onnistui", "Mökin tiedot tallennettu onnistuneesti.", "OK");
            // Tässä kohtaa voit päivittää käyttöliittymän tai navigoida takaisin pääsivulle
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
            var databaseAccess = new DatabaseAccess();
            await databaseAccess.PoistaMokkiTietokannasta(_mokki.mokki_id);
            await DisplayAlert("Poistettu", "Mökki on poistettu onnistuneesti.", "OK");
            // Palaa tarvittaessa edelliselle sivulle
            await Navigation.PopAsync();
        }
    }

    public partial class DatabaseAccess
    {
        public async Task LisaaMokkiTietokantaan(Mokki uusiMokki)
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

                    var query = "INSERT INTO mokki (mokki_id, alue_id, mokkinimi, katuosoite, postinro, henkilomaara, hinta, kuvaus, varustelu)  VALUES (@Mokki_id, @Alue_id, @Mokkinimi, @Katuosoite, @Postinro, @Hinta, @Henkilomaara, @Kuvaus, @Varustelu)";

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
                    }
                }
                catch (Exception ex)
                {
                    // Käsittely mahdollisille poikkeuksille
                    Debug.WriteLine(ex.Message);
                }
            }
        }

        public async Task<bool> PoistaMokkiTietokannasta(int mokki_id)
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

                    var query = "DELETE FROM mokki WHERE mokki_id = @Mokki_id";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Mokki_id", mokki_id);
                        var result = await command.ExecuteNonQueryAsync();
                        return result > 0; // palauttaa true jos yksi tai usempi rivi poistettiin
                    }
                }
                catch (Exception ex)
                {
                    // Käsittely mahdollisille poikkeuksille
                    Console.WriteLine(ex.Message);
                    return false;
                }
            }
        }

        public async Task<bool> PaivitaMokinTiedot(Mokki muokattuMokki)
        {
            string projectDirectory = System.AppDomain.CurrentDomain.BaseDirectory;
            var projectRoot = Path.GetFullPath(Path.Combine(projectDirectory, @"..\..\..\..\..\"));

            DotNetEnv.Env.Load(projectRoot);
            var env = Environment.GetEnvironmentVariables();

            string connectionString = $"server={env["SERVER"]};port={env["SERVER_PORT"]};database={env["SERVER_DATABASE"]};user={env["SERVER_USER"]};password={env["SERVER_PASSWORD"]}";
            using (var connection = new MySqlConnection(connectionString))
         
            {
                await connection.OpenAsync();

                var query = @"
                UPDATE mokki SET
                    alue_id = @AlueId;
                    mokkinimi = @Mokkinimi,
                    katuosoite = @Katuosoite,
                    postinro = @Postinro,
                    hinta = @Hinta,
                    henkilomaara = @Henkilomaara,
                    kuvaus = @Kuvaus,
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
                    command.Parameters.AddWithValue("@Henkilomaara", muokattuMokki.henkilomaara);
                    command.Parameters.AddWithValue("@Kuvaus", muokattuMokki.kuvaus);
                    command.Parameters.AddWithValue("@Varustelu", muokattuMokki.varustelu);
                    

                    var result = await command.ExecuteNonQueryAsync();
                    return result > 0; // Onnistuiko päivitys
                }
            }
        }
    }
}





    
