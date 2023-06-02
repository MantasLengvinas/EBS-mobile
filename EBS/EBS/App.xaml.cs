using System;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace EBS
{
    public partial class App : Application
    {
        public INavigation navigation { get; set; }

        public App ()
        {
            InitializeComponent();

            bool sessionActive = Preferences.Get("logged_in", false);

            if(sessionActive)
                MainPage = new NavigationPage(new MainPage());
            else
                MainPage = new WelcomePage();
        }

        protected override void OnStart ()
        {
        }

        protected override void OnSleep ()
        {
        }

        protected override void OnResume ()
        {
        }
    }
}

