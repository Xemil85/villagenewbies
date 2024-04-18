using Microsoft.Maui.Controls;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Input;

namespace VillageNewbies
{
    public partial class BillsPage : ContentPage
    {
        public ObservableCollection<Lasku> Laskut { get; private set; }
        public ICommand PayBillCommand { get; private set; }

        public BillsPage()
        {
            InitializeComponent();
            Laskut = new ObservableCollection<Lasku>();
            PayBillCommand = new Command<int>(async (laskuId) => await MarkBillAsPaid(laskuId));
            this.BindingContext = this;
            LoadBills();
            BillsCollectionView.ItemsSource = Laskut; 
        }

        private async Task LoadBills()
        {
            var laskuAccess = new LaskuAccess();
            var laskuList = await laskuAccess.FetchAllLaskutAsync();
            Laskut.Clear();
            foreach (var lasku in laskuList)
            {
                Laskut.Add(lasku);
            }
        }

        private async Task MarkBillAsPaid(int laskuId)
        {
            var laskuAccess = new LaskuAccess();
            await laskuAccess.MarkAsPaid(laskuId);
            await LoadBills();
        }
    }

    public class BooleanToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return boolValue ? "Maksettu" : "Ei Maksettu";
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
