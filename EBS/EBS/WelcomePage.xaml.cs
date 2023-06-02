using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace EBS
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class WelcomePage : TabbedPage
    {
        public WelcomePage ()
        {
            InitializeComponent();
            BindingContext = this;
        }

        protected override void OnAppearing()
        {
            if (Preferences.ContainsKey("logged_in"))
            {
                if (Preferences.Get("user_id", 0) == 99)
                    new AdminPage();
                else
                    new NavigationPage(new MainPage());
            }
        }

        public void Login(object sender, EventArgs e)
        {

            Email = emailEntry.Text;
            Password = passwordEntry.Text;

            User user = Users.Find(x => x.Email.ToLower() == Email.ToLower());

            if(user == null)
            {
                DisplayAlert("Klaida", "Vartotojas nerastas", "Uždaryti");
                return;
            }

            if(user.Password.ToLower() != Password.ToLower())
            {
                DisplayAlert("Klaida", "Neteisingas slaptažodis", "Uždaryti");
                return;
            }

            Preferences.Set("logged_in", true);
            Preferences.Set("user_id", user.Id.ToString());
            Preferences.Set("full_name", user.FullName);

            if (user.Id == 99)
                Application.Current.MainPage = new AdminPage();
            else
                Application.Current.MainPage = new NavigationPage(new MainPage());
        }

        public void Register(object sender, EventArgs e)
        {
            string fullName = fullNameEntry.Text;
            string emailValue = registerEmail.Text;
            string passw = registerPassword.Text;

            User user = Users.Find(x => x.Email.ToLower() == emailValue.ToLower() && x.Email != "test@test");

            if(user != null)
            {
                DisplayAlert("Klaida", "El. paštas jau naudojamas", "Uždaryti");
                return;
            }

            user = Users.Find(x => x.Id == 14);
            Preferences.Set("logged_in", true);
            Preferences.Set("user_id", user.Id.ToString());
            Preferences.Set("full_name", user.FullName);

            Application.Current.MainPage = new NavigationPage(new MainPage());

        }

    }

    public partial class WelcomePage
    {
        private string email;
        public string Email
        {
            get { return email; }
            set
            {
                email = value;
            }
        }

        private string password;
        public string Password
        {
            get { return password; }
            set { password = value; }
        }

        private class User
        {
            public string Email { get; set; }
            public string Password { get; set; }
            public int Id { get; set; }
            public string FullName { get; set; }
        }

        private readonly List<User> Users = new List<User>()
        {
            new User
            {
                Id = 13,
                Email = "mantas@gmail.com",
                Password = "mantas",
                FullName = "Mantas Lengvinas"
            },
            new User
            {
                Id = 2,
                Email = "mantas2@gmail.com",
                Password = "mantas",
                FullName = "Mantas Lengvinas"
            },
            new User
            {
                Id = 1,
                Email = "gustas@gmail.com",
                Password = "gustas",
                FullName = "Gustas Stasevskis"
            },
            new User
            {
                Id = 99,
                Email = "admin@ebs.lt",
                Password = "admin",
                FullName = "Administratorius"
            },
            new User
            {
                Id = 14,
                Email = "test@test",
                Password = "test",
                FullName = "Testas Testauskas"
            }
        };
    }
}
