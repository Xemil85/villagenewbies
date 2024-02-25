using Microsoft.Maui.Controls;
using System;
using System.Diagnostics;

namespace VillageNewbies.Views
{
    public partial class CabinsPage : ContentPage
    {
        public CabinsPage()
        {
            InitializeComponent();
            printmokit();
        }

        public async void printmokit()
        {
            var mokkiAccess = new MokkiAccess();

            List<Mokki> mokit = await mokkiAccess.FetchAllMokitAsync();

            foreach (var mokki in mokit)
            {
                Debug.WriteLine($"Id: {mokki.mokki_id}, Nimi: {mokki.mokkinimi}, Kuvaus: {mokki.kuvaus}, Varustelu:{mokki.varustelu}");
            }
        }
    }
}
