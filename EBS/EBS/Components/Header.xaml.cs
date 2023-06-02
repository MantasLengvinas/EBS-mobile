using System;
using System.Collections.Generic;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace EBS.Components
{
    public partial class Header : ContentView
    {
        public Header()
        {
            InitializeComponent();
        }

        public async void NavigateToHistory(Object sender, EventArgs e)
        {
            await this.Navigation.PushAsync(new UserPage());
        }

        public void Logout(object sender, EventArgs e)
        {
            Preferences.Clear();
            Application.Current.MainPage = new WelcomePage();
        }

        void Button_Clicked(System.Object sender, System.EventArgs e)
        {

        }

        void Button_Clicked_1(System.Object sender, System.EventArgs e)
        {
        }
    }
}

