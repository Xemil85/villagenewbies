using MySql.Data.MySqlClient;

namespace VillageNewbies;

public partial class AddCustomerPage : ContentPage
{
	public AddCustomerPage()
	{
		InitializeComponent();
	}

    // asiakkaan vienti tietokantaan
    private async void LisaaAsiakas_Clicked(object sender, EventArgs e)
    {
        // jos kentät tyhjät ja yritetään tallentaa
        if (string.IsNullOrWhiteSpace(etunimi.Text) ||
        string.IsNullOrWhiteSpace(sukunimi.Text) ||
        string.IsNullOrWhiteSpace(lähiosoite.Text) ||
        string.IsNullOrWhiteSpace(postinro.Text) ||
        string.IsNullOrWhiteSpace(sähköposti.Text) ||
        string.IsNullOrWhiteSpace(puhelinnro.Text))
        {
            // Näytä varoitusikkuna
            await DisplayAlert("Täyttämättömät tiedot", "Täytä kaikki asiakastiedot ennen lähettämistä.", "OK");
            return; // Lopeta metodin suoritus tähän
        }

       



        var uusiAsiakas = new Asiakas
        {
            etunimi = etunimi.Text,
            sukunimi = sukunimi.Text,
            lahiosoite = lähiosoite.Text,
            postinro = int.Parse(postinro.Text),
            email = sähköposti.Text,
            puhelinnro = puhelinnro.Text
        };


        var databaseAccess = new DatabaseAccess();
        await databaseAccess.LisaaAsiakasTietokantaan(uusiAsiakas);

        // lisää tähän: palaa edelliselle sivulle tai anna käyttäjälle palaute onnistuneesta lisäyksestä
        
        etunimi.Text = "";
        sukunimi.Text = "";
        lähiosoite.Text = "";
        postinro.Text = "";
        toimipaikka.Text = "";
        sähköposti.Text = "";
        puhelinnro.Text = "";
    }
      
       

    public class DatabaseAccess
    {
        public async Task LisaaAsiakasTietokantaan(Asiakas uusiAsiakas)
        {
            string connectionString = "server=localhost;database=vn;user=;password=;";
            using (var connection = new MySqlConnection(connectionString))
            {
                try
                {
                    await connection.OpenAsync();

                    var query = "INSERT INTO asiakas (postinro, etunimi, sukunimi, lahiosoite, email, puhelinnro) VALUES (@Postinro, @Etunimi, @Sukunimi, @Lahiosoite, @Email, @Puhelinnro)";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        
                        command.Parameters.AddWithValue("@Postinro", uusiAsiakas.postinro);
                        command.Parameters.AddWithValue("@Etunimi", uusiAsiakas.etunimi);
                        command.Parameters.AddWithValue("@Sukunimi", uusiAsiakas.sukunimi);
                        command.Parameters.AddWithValue("@Lahiosoite", uusiAsiakas.lahiosoite);
                        command.Parameters.AddWithValue("@Email", uusiAsiakas.email);
                        command.Parameters.AddWithValue("@Puhelinnro", uusiAsiakas.puhelinnro);

                        await command.ExecuteNonQueryAsync();
                    }
                    

                    


                }
                catch (Exception ex)
                {
                    // Käsittely mahdollisille poikkeuksille
                    Console.WriteLine(ex.Message);
                }
            }
        }
    }
}

    
