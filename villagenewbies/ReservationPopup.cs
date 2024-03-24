using Microsoft.Maui.Controls;
using CommunityToolkit.Maui.Views;
using System;


namespace VillageNewbies.Views
{
    public class ReservationPopup : Popup
    {
        public ReservationPopup()
        {
            // Määrittele popup-ikkunan sisältö täällä
            Content = new VerticalStackLayout
            {
                Children =
                {
                    new Label { Text = "Varauksen tiedot", HorizontalOptions = LayoutOptions.Center }
                    // Tähän sit kaikkea siistiä
                }
            };

            // Muokkaa popupin kokoa
            Content.WidthRequest = 400;
            Content.HeightRequest = 300;
        }
    }
}
