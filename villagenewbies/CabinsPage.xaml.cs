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
        private Dictionary<int, string> _alueNimet = new Dictionary<int, string>();
            //{
            //  { 0, "Valitse"},
            //  { 1, "Ylläs" },
            //  { 2, "Ruka" },
            //  { 3, "Pyhä" },
            //  { 4, "Levi" },
            //  { 5, "Syöte" },
            //  { 6, "Vuokatti" },
            //  { 7, "Tahko" },
            //  { 8, "Himos" },
            //};

        private readonly Dictionary<int, string> _hintaLuokat = new Dictionary<int, string>
            {
              {0, "Valitse"},  
              {1, "Edullinen" },
              {2, "Keskiluokka" },
              {3, "Premium" }
            };

        public ObservableCollection<Mokki> Mokit { get; private set; }
        private int? valittuAlueId;
        private int? valittuHintaLuokka;
        public CabinsPage()
        {
            InitializeComponent();
            Mokit = new ObservableCollection<Mokki>();
            CabinsCollectionView.ItemsSource = Mokit;
            AreaPicker.ItemsSource = _alueNimet.Values.ToList();
            HintaPicker.ItemsSource = _hintaLuokat.Values.ToList();
            LoadMokitAsync();
            LataaAlueet();
            SetDefaultDates();
            Debug.WriteLine(Aloituspaiva.Date.ToString("yyyy-MM-dd HH:mm:ss"));
          
        }

        // Aseta oletuspäivämäärät aloitus- ja lopetuspäiville
        private void SetDefaultDates()
        {
            Aloituspaiva.Date = DateTime.Now; // Aseta aloituspäivä nykyhetkeen
            Lopetuspaiva.Date = DateTime.Now.AddDays(1); // Aseta lopetuspäivä yksi päivä eteenpäin
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
            await Navigation.PushAsync(new EditCabinPage(mokki));
        }

        private async void LisaaAlue_Clicked(object? sender, EventArgs e)
        {
            await Navigation.PushAsync(new AddAreaPage());
        }

        private async void PoistaAlue_Clicked(object? sender, EventArgs e)
        {
            await Navigation.PushAsync(new DeleteAreaPage());
        }

        private void ResetFiltersButton_Clicked(object sender, EventArgs e)
        {
            // Nollaa valitut suodatusasetukset
            valittuAlueId = null;
            valittuHintaLuokka = null;

            // Varmista, että Pickerit näyttävät "valitse" tai tyhjä asetus
            AreaPicker.SelectedIndex = -1;
            HintaPicker.SelectedIndex = -1;

            // Lataa kaikki mökit uudelleen
            LoadMokitAsync();
        }



        private async Task LoadMokitAsync()
        {
            var mokkiAccess = new MokkiAccess();
            var mokitList = await mokkiAccess.FetchAllMokitAsync();

            //var suodatetutMokit = SuodataMokit(mokitList);
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
             LoadMokitAsync();// Päivitä lista  
            LataaAlueet();
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

            var aloitusPaiva = Aloituspaiva.Date;
            var lopetusPaiva = Lopetuspaiva.Date;

            // Tarkista, ettei aloitus- ja lopetuspäivät ole menneisyydessä tai että lopetuspäivä on ennen aloituspäivää
            if (lopetusPaiva <= aloitusPaiva || aloitusPaiva <= DateTime.Today)
            {
                await DisplayAlert("Virheellinen päivämäärä", "Aloituspäivämäärän on oltava tulevaisuudessa ja lopetuspäivämäärän on oltava jälkeen aloituspäivän.", "OK");
                return;
            }

            var mokkiAccess = new MokkiAccess();
            bool isAvailable = await mokkiAccess.IsCabinAvailable(mokki.mokki_id, aloitusPaiva, lopetusPaiva);

            if (!isAvailable)
            {
                await DisplayAlert("Varaus ei onnistu", "Valitsemasi mökki on jo varattu valituille päivämäärille. Tarkista saatavuus ja yritä uudelleen.", "OK");
                return;
            }

            await Navigation.PushAsync(new SelectServices(mokki, aloitusPaiva.ToString("yyyy-MM-dd"), lopetusPaiva.ToString("yyyy-MM-dd")));
        }

        private async void LataaAlueet()
        {
            var alueetAccess = new MokkiAccess(); 
            var alueet = await alueetAccess.FetchAllAlueAsync();

            // Muunna haetut alueet sanakirjaksi
            _alueNimet = alueet.ToDictionary(a => a.alue_id, a => a.nimi);
            AreaPicker.ItemsSource = _alueNimet.Values.ToList();
           
        }



        // Kutsutaan, kun alue valitaan Pickeristä
        private void OnAreaSelected(object sender, EventArgs e)
        {
            /*if (AreaPicker.SelectedIndex != -1)
            {
                var selectedAreaName = AreaPicker.SelectedItem.ToString();
                valittuAlueId = _alueNimet.FirstOrDefault(x => x.Value == selectedAreaName).Key;
            }
            else
            {
                valittuAlueId = null;
            }*/
            if (AreaPicker.SelectedIndex == -1)
            {
                valittuAlueId = null;
                return;
            }

            var selectedAreaName = AreaPicker.SelectedItem.ToString();
            valittuAlueId = _alueNimet.FirstOrDefault(x => x.Value == selectedAreaName).Key;


            SuodataMokit();
        }


        private void onPriceSelected(object sender, EventArgs e)
        {
            if (HintaPicker.SelectedIndex != -1)
            {
                var selectedPriceName = HintaPicker.SelectedItem.ToString();
                valittuHintaLuokka = _hintaLuokat.FirstOrDefault(x => x.Value == selectedPriceName).Key;
            }
            else
            {
                valittuHintaLuokka = null;
            }

            SuodataMokit();
        }

        // Yhdistetty suodatusmetodi
        private async Task SuodataMokit()
        {
            var mokitList = await new MokkiAccess().FetchAllMokitAsync();
            IEnumerable<Mokki> suodatetutMokit = mokitList;
            //var suodatetutMokit = Mokit.AsEnumerable();

            if (valittuAlueId.HasValue)
            {
                suodatetutMokit = suodatetutMokit.Where(m => m.alue_id == valittuAlueId.Value);
            }

            if (valittuHintaLuokka.HasValue)
            {
                switch (valittuHintaLuokka.Value)
                {
                    case 1:
                        suodatetutMokit = suodatetutMokit.Where(m => m.hinta >= 10 && m.hinta <= 100);
                        break;
                    case 2:
                        suodatetutMokit = suodatetutMokit.Where(m => m.hinta >= 105 && m.hinta <= 120);
                        break;
                    case 3:
                        suodatetutMokit = suodatetutMokit.Where(m => m.hinta >= 130);
                        break;
                }

            }
              // Päivitä CabinsCollectionView suodatetulla listalla
              MainThread.InvokeOnMainThreadAsync(() =>
              {
                CabinsCollectionView.ItemsSource = suodatetutMokit.ToList();
              });

        }  
    }      
} 

        