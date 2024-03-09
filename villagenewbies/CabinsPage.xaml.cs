using Google.Protobuf.WellKnownTypes;
using Microsoft.Maui.Controls;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;


namespace VillageNewbies.Views
{
    public partial class CabinsPage : ContentPage
    {
        // Alueiden nimien mappaus
        private readonly Dictionary<int, string> _alueNimet = new Dictionary<int, string>
        {
            { 1, "Ylläs" },
            { 2, "Ruka" },
            { 3, "Pyhä" },
            { 4, "Levi" },
            { 5, "Syöte" },
            { 6, "Vuokatti" },
            { 7, "Tahko" },
            { 8, "Himos" },
        };

        public ObservableCollection<Mokki> Mokit { get; private set; }

        public CabinsPage()
        {
            InitializeComponent();
            Mokit = new ObservableCollection<Mokki>();
            AreaPicker.ItemsSource = _alueNimet.Values.ToList(); // Alueiden nimet Pickeriin
            LoadMokitAsync();
        }


        private async Task LoadMokitAsync()
        {
            var mokkiAccess = new MokkiAccess();
            var mokitList = await mokkiAccess.FetchAllMokitAsync();
            MainThread.InvokeOnMainThreadAsync(() =>
            {
                foreach (var mokki in mokitList)
                {
                    Mokit.Add(mokki);
                }
                CabinsCollectionView.ItemsSource = Mokit;
            });
        }

        // Kutsutaan, kun alue valitaan Pickeristä
        //TODO Alueen valinnan jälkeen mökkien uusi suodatus toimimaan 
        private void OnAreaSelected(object sender, EventArgs e)
        {
            if (AreaPicker.SelectedIndex == -1)
                return;

            var selectedAreaName = AreaPicker.SelectedItem.ToString();
            int selectedAreaId = _alueNimet.FirstOrDefault(x => x.Value == selectedAreaName).Key;

            var filteredMokit = Mokit.Where(m => m.alue_id == selectedAreaId).ToList();
            CabinsCollectionView.ItemsSource = filteredMokit;
        }
    }
}
    


