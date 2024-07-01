using System.Linq;
using System.Collections.Generic;

namespace CPSTest
{
    public partial class LBPage : ContentPage
    {
        private List<Tulos> tulokset;
        public LBPage(List<Tulos> tulokset)
        {
            //Muodostin; ottaa listan tuloksista ja alustaa tulostaulukko-sivun.
            InitializeComponent();
            this.tulokset = tulokset;
            PaivitaTulostaulukko();
        }
        public void PaivitaTulostaulukko()
        {
            //Lasketaan käyttäjän klikkausnopeus, lajitellaan laskevassa järjetyksessä ja näytetään 5 parasta
            var lajitellutTulokset = tulokset
                .OrderByDescending(x => (double)x.clicks / x.elapsedTime.TotalSeconds)
                .Take(5);

            var items = lajitellutTulokset
                .Select((x, index) => $"{index + 1}: {(double)x.clicks / x.elapsedTime.TotalSeconds:F2} CPS")
                .ToList();

            LBListView.ItemsSource = items;
        }
        private async void OnBackClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }
    }
}