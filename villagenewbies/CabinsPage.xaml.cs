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
        private readonly Dictionary<int, string> _alueNimet = new Dictionary<int, string>
            {
              { 0, "Valitse"},
              { 1, "Yll�s" },
              { 2, "Ruka" },
              { 3, "Pyh�" },
              { 4, "Levi" },
              { 5, "Sy�te" },
              { 6, "Vuokatti" },
              { 7, "Tahko" },
              { 8, "Himos" },
            };

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
            Debug.WriteLine(Aloituspaiva.Date.ToString("yyyy-MM-dd HH:mm:ss"));
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
                await DisplayAlert("Virhe", "M�kintietojen lataaminen ep�onnistui.", "OK");
                return;
            }

            // Siirryt��n muokkaussivulle ja v�litet��n asiakas-olio konstruktorin kautta
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

        private void ResetFiltersButton_Clicked(object sender, EventArgs e)
        {
            // Nollaa valitut suodatusasetukset
            valittuAlueId = null;
            valittuHintaLuokka = null;

            // Varmista, ett� Pickerit n�ytt�v�t "valitse" tai tyhj� asetus
            AreaPicker.SelectedIndex = -1;
            HintaPicker.SelectedIndex = -1;

            // Lataa kaikki m�kit uudelleen
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
                //SuodataMokit();
            });
        }
        protected override void OnAppearing()
        {
            base.OnAppearing();
             LoadMokitAsync();// P�ivit� lista  
        }
       

        private async void Varaamokki(object sender, EventArgs e)
        {
            if (!(sender is Button button)) return;

            var mokki = button.CommandParameter as Mokki;
            if (mokki == null)
            {
                await DisplayAlert("Virhe", "M�kintietojen lataaminen ep�onnistui.", "OK");
                return;
            }

            var aloitusPaiva = Aloituspaiva.Date.ToString("yyyy-MM-dd");
            var lopetusPaiva = Lopetuspaiva.Date.ToString("yyyy-MM-dd");

            // Siirryt��n muokkaussivulle ja v�litet��n asiakas-olio konstruktorin kautta
            await Navigation.PushAsync(new SelectServices(mokki, aloitusPaiva, lopetusPaiva));
        }


        // Kutsutaan, kun alue valitaan Pickerist�
        private void OnAreaSelected(object sender, EventArgs e)
        {
            if (AreaPicker.SelectedIndex != -1)
            {
                var selectedAreaName = AreaPicker.SelectedItem.ToString();
                valittuAlueId = _alueNimet.FirstOrDefault(x => x.Value == selectedAreaName).Key;
            }
            else
            {
                valittuAlueId = null;
            }

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
              // P�ivit� CabinsCollectionView suodatetulla listalla
              MainThread.InvokeOnMainThreadAsync(() =>
              {
                CabinsCollectionView.ItemsSource = suodatetutMokit.ToList();
              });

        }  
    }      
} 

        