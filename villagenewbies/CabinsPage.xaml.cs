using Google.Protobuf.WellKnownTypes;
using Microsoft.Maui.Controls;
using CommunityToolkit.Maui.Views;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;


namespace VillageNewbies.Views
{

    public partial class CabinsPage : ContentPage
    {

        public ObservableCollection<Mokki> Mokit { get; private set; }


        public CabinsPage()
        {
            InitializeComponent();
            Mokit = new ObservableCollection<Mokki>();
            CabinsCollectionView.ItemsSource = Mokit;
           // AreaPicker.ItemsSource = _alueNimet.Values.ToList();
            //HintaPicker.ItemsSource = _hintaLuokat.Values.ToList();
            LoadMokitAsync();
            Debug.WriteLine(Aloituspaiva.Date.ToString("yyyy-MM-dd HH:mm:ss"));
           // LisaaMokki.Clicked += LisaaMokki_Clicked;
           // LisaaAlue.Clicked += LisaaAlue_Clicked;
           // PoistaAlue.Clicked += PoistaAlue_Clicked;
            //PoistaMokki.Clicked += PoistaMokki_Clicked;
            
        }
        private async void LisaaMokki_Clicked(object? sender, EventArgs e)
        {
            await Navigation.PushAsync(new AddCabinPage());
        }

        private async void MuokkaaMokkia_Clicked(object? sender, EventArgs e)
        {
                if (!(sender is Button button)) return;

                var mokki = button.CommandParameter as Mokki;
                if (mokki == null)
                {
                    await DisplayAlert("Virhe", "Mökintietojen lataaminen epäonnistui.", "OK");
                    return;
                }

                // Siirrytään muokkaussivulle ja välitetään asiakas-olio konstruktorin kautta
                await Navigation.PushAsync(new AddCabinPage(mokki));
        }

        private async void LisaaAlue_Clicked(object? sender, EventArgs e)
        {
            await Navigation.PushAsync(new AddAreaPage());
        }

        private async void PoistaAlue_Clicked(object? sender, EventArgs e)
        {
            await Navigation.PushAsync(new DeleteAreaPage());
        }


        private async Task LoadMokitAsync()
        {
            var mokkiAccess = new MokkiAccess();
            var mokitList = await mokkiAccess.FetchAllMokitAsync();
            MainThread.InvokeOnMainThreadAsync(() =>
            {
                Mokit.Clear();
                foreach (var mokki in mokitList)
                {
                    Mokit.Add(mokki);
                }
                CabinsCollectionView.ItemsSource = Mokit;
            });
        }
        protected override void OnAppearing()
        {
            base.OnAppearing();
            LoadMokitAsync(); // Päivitä mökkilista täällä
        }

        private async void Varaamokki(object sender, EventArgs e)
        {
            if (!(sender is Button button)) return;

            var mokki = button.CommandParameter as Mokki;
            if (mokki == null)
            {
                await DisplayAlert("Virhe", "Mökintietojen lataaminen epäonnistui.", "OK");
                return;
            }

            var aloitusPaiva = Aloituspaiva.Date.ToString("yyyy-MM-dd");
            var lopetusPaiva = Lopetuspaiva.Date.ToString("yyyy-MM-dd");

            // Siirrytään muokkaussivulle ja välitetään asiakas-olio konstruktorin kautta
            await Navigation.PushAsync(new SelectServices(mokki, aloitusPaiva, lopetusPaiva));
        }
    }
}

        