namespace VillageNewbies.Views
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
            // Tähän lisätään navigointilogiikka
            AvaaMokit.Clicked += AvaaMokit_Clicked;
            AvaaPalvelut.Clicked += AvaaPalvelut_Clicked;
            AvaaVaraukset.Clicked += AvaaVaraukset_Clicked;
            AvaaAsiakkaat.Clicked += AvaaAsiakkaat_Clicked;
            AvaaRaportit.Clicked += AvaaRaportit_Clicked;
        }

        private async void AvaaAsiakkaat_Clicked(object? sender, EventArgs e)
        {
            await Navigation.PushAsync(new CustomerPage());
        }

        private async void AvaaVaraukset_Clicked(object? sender, EventArgs e)
        {
            await Navigation.PushAsync(new BookingPage());
        }

        private async void AvaaPalvelut_Clicked(object? sender, EventArgs e)
        {
            await Navigation.PushAsync(new ServicesPage());
        } 

        
        private async void AvaaMokit_Clicked(object? sender, EventArgs e)
        {
            await Navigation.PushAsync(new CabinsPage());
        }

        private async void AvaaLaskut_Clicked(object? sender, EventArgs e)
        {
            await Navigation.PushAsync(new BillsPage());
        }

        private async void AvaaRaportit_Clicked(object? sender, EventArgs e)
        {
            await Navigation.PushAsync(new Reportage());
        }

    }
}
