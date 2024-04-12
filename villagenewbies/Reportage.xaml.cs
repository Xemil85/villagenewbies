namespace VillageNewbies;

public partial class Reportage : ContentPage
{
	public Reportage()
	{
		InitializeComponent();
        InitializePicker();

    }

    private void InitializePicker()
    {
        
        AreaPicker.ItemsSource = new List<string> { "Alue 1", "Alue 2", "Alue 3" };
        AreaPicker.SelectedIndex = 0;  // Valitsee ensimm‰isen alueen oletuksena

        // Aseta datepickerit n‰ytt‰m‰‰n t‰m‰n p‰iv‰n p‰iv‰m‰‰r‰
        StartDatePicker.Date = DateTime.Today;
        EndDatePicker.Date = DateTime.Today.AddDays(1); // Oletus loppup‰iv‰m‰‰r‰ on huomenna
    }

    private void OnFetchBookingsClicked(object sender, EventArgs e)
    {
        // T‰m‰ metodi ei tee mit‰‰n t‰ll‰ hetkell‰
        DisplayAlert("Toiminnallisuus", "Varausten haku ei ole viel‰ toteutettu.", "OK");
    }
}
