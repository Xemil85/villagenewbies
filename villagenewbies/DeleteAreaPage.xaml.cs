using System.Collections.ObjectModel;

namespace VillageNewbies;

public partial class DeleteAreaPage : ContentPage
{
    public ObservableCollection<Alue> Alueet { get; private set; }

    public DeleteAreaPage()
	{
		InitializeComponent();
        Alueet = new ObservableCollection<Alue>();
        AreasCollectionView.ItemsSource = Alueet;
        LoadAlueetAsync();
    }
    private async Task LoadAlueetAsync()
    {
        var alueetAccess = new MokkiAccess();
        var alueetList = await alueetAccess.FetchAllAlueAsync();
        MainThread.InvokeOnMainThreadAsync(() =>
        {
            foreach (var alue in alueetList)
            {
                Alueet.Add(alue);
            }
            //AreasCollectionView.ItemsSource = Alueet;
        });
    }
}