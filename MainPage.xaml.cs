using Microsoft.Maui.Controls;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace CPSTest
{
    public partial class MainPage : ContentPage
    {
        int maara = 0;
        TimeSpan aika = TimeSpan.Zero;
        Timer ajastin;
        bool ajastinKaynnissa = false;
        string tulostenTiedostopolku = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "tulokset.json");
        public MainPage()
        {
            InitializeComponent();
            ajastin = new Timer(OnTimerTick, null, Timeout.Infinite, 100);
        }
        protected override void OnAppearing()
        {
            //Tätä funktiota kutsutaan, kun sivu aukeaa. Tämä funktio resetoi ajastimen ja klikkauslaskurin.
            base.OnAppearing();
            maara = 0;
            ClickBtn.Text = "Klikkaa tästä";
            LBBtn.IsEnabled = false;
        }
        private void OnClickClicked(object sender, EventArgs e)
        {
            //Tämä funktio päivittää ylimmän napin tekstiä ja käynnistää ajastimen, jos se ei ole käynnissä.
            maara++;

            if (maara == 0)
            {
                ClickBtn.Text = "Klikkaa tästä";
            }
            else
            {
                ClickBtn.Text = $"{maara} klikkausta";
            }
        
            SemanticScreenReader.Announce(ClickBtn.Text);

            if (!ajastinKaynnissa)
            {
                ajastin.Change(0, 100);
                ajastinKaynnissa = true;
                LBBtn.IsEnabled = false;
            }
        }
        private async void OnLBClicked(object sender, EventArgs e)
        { 
            /*
             Tämä funktio pysäyttää ajastimen, kun LBBtn-nappia painetaan. LBBtn-nappia voidaan painaa vasta,
            kun se on ollut käynnissä 5 sekuntia. Nappia painettaessa cps-tulos tallennetaan ja käyttöliittymässä
            siirrytään tulostaulukkosivulle.
             */
            if (aika.TotalSeconds >= 5) 
            {
                var kulunutAika = aika;
                ajastin.Change(Timeout.Infinite, Timeout.Infinite);
                ajastinKaynnissa = false;
                aika = TimeSpan.Zero;
                AjastinLabel.Text = "00:00:00";

                var tulos = new Tulos { clicks = maara, elapsedTime = kulunutAika };
                await TallennaTulos(tulos);

                var lbPage = new LBPage(await LataaTulokset());
                await Navigation.PushAsync(lbPage);
            }
        }
        private void OnTimerTick(object? state)
        {
            //Tämä funktio päivittää ajastimen labelia
            aika = aika.Add(TimeSpan.FromMilliseconds(100));
            string aikaYksikot = string.Format("{0:D2}:{1:D2}:{2:D2}", 
                aika.Minutes, aika.Seconds, aika.Milliseconds / 10);

            _ = MainThread.InvokeOnMainThreadAsync(() =>
            {
                AjastinLabel.Text = aikaYksikot;

                if (aika.TotalSeconds >= 5) //LBtn-nappia voidaan klikata vasta,
                // kun ajastin on ollut käynnissä 5 sekuntia.
                {
                    LBBtn.IsEnabled = true;
                }
            });
        }
        private async Task TallennaTulos(Tulos tulos)
        {
            //Tämä funktio tallentaa tuloksen json-tiedostoon.
            List<Tulos> tulokset = await LataaTulokset();
            tulokset.Add(tulos);

            var json = JsonSerializer.Serialize(tulokset);
            await File.WriteAllTextAsync(tulostenTiedostopolku, json);
        }
        private async Task<List<Tulos>> LataaTulokset()
        {
            //Tämä funktio lataa tulokset json-tiedostosta.
            List<Tulos>? tulokset;

            if (File.Exists(tulostenTiedostopolku))
            {
                var json = await File.ReadAllTextAsync(tulostenTiedostopolku);
                tulokset = JsonSerializer.Deserialize<List<Tulos>>(json);
            }
            else //Luodaan tyhjä lista Tulos-objekteista, jos tulokset.json -tiedostoa ei ole olemassa.
            {
                tulokset = new List<Tulos>();
            }

            return tulokset ?? new List<Tulos>();
        }
    }
}
