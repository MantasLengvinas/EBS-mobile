using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Xamarin.Forms;
using EBS.Models.Dtos;
using EBS.Models.General;
using Xamarin.Essentials;
using EBS.Services;
using System.Threading.Tasks;
using System.Linq;

namespace EBS
{
    public partial class UserPage : ContentPage
    {
        public UserPage()
        {
            InitializeComponent();
            apiClient = new ApiClient();
            BindingContext = this;
            PageData.ItemsSource = addresses;

        }

        protected override async void OnAppearing()
        {
            addresses.Clear();

            ApiResponse<List<GetAddressResponseDto>> response = await apiClient.GetAsync<List<GetAddressResponseDto>>($"http://localhost:50001/api/address/user/{Preferences.Get("user_id", "0")}");
            if (response.IsSuccess)
                foreach (GetAddressResponseDto addr in response.Data)
                    addresses.Add(addr);

            await GetProvidersAsync();
        }

        private async Task GetProvidersAsync()
        {
            ApiResponse<List<GetProviderResponseDto>> response = await apiClient.GetAsync<List<GetProviderResponseDto>>("http://localhost:50001/api/provider");

            if (response.IsSuccess)
                providers.AddRange(response.Data);
        }

        public async void AddAddress(object sender, EventArgs e)
        {
            string addressName = await DisplayPromptAsync("Adresas", "Įrašykite adresą", accept: "Išsaugoti", cancel: "Atšaukti", placeholder: "Gatvė namas-butas, miestas");

            if (string.IsNullOrEmpty(addressName) && addressName.Length == 1)
            {
                await DisplayAlert("Klaida", "Neteisingas adresas", "Uždaryti");
                return;
            }

            string provider = await DisplayActionSheet("Tiekėjas", "Atšaukti", null, providers.Select(x => x.ProviderName).ToArray());

            if(provider == "Atšaukti")
            {
                await DisplayAlert("Klaida", "Neišsaugota", "Uždaryti");
                return;
            }

            int providerId = providers.Find(x => x.ProviderName == provider).ProviderId;

            ApiResponse<GetAddressResponseDto> res = await apiClient.PostAsync<GetAddressResponseDto>("http://localhost:50001/api/address", new GetAddressResponseDto
            {
                FullAddress = addressName,
                UserId = int.Parse(Preferences.Get("user_id", "0")),
                ProviderId = providerId,
                UserFullName = Preferences.Get("full_name", "mantas")
            });

            if(!res.IsSuccess)
            {
                await DisplayAlert("Klaida", "Serverio klaida", "Uždaryti");
                return;
            }


            ApiResponse<List<GetAddressResponseDto>> response = await apiClient.GetAsync<List<GetAddressResponseDto>>($"http://localhost:50001/api/address/user/{Preferences.Get("user_id", "0")}");

            if (response.IsSuccess)
            {
                addresses.Clear();
                foreach (var addr in response.Data)
                    addresses.Add(addr);
            }

            await DisplayAlert("Išsaugota", "Adresas sėkmingai pridėtas", "Uždaryti");

            addresses.Clear();

            ApiResponse<List<GetAddressResponseDto>> res1 = await apiClient.GetAsync<List<GetAddressResponseDto>>($"http://localhost:50001/api/address/user/{Preferences.Get("user_id", "0")}");
            if (res1.IsSuccess)
                foreach (GetAddressResponseDto addr in res1.Data)
                    addresses.Add(addr);
        }

        public async void DeleteAddress(object sender, EventArgs e)
        {
            string delete = await DisplayActionSheet("Ar tikrai norite ištrinti šį adresą?", cancel: "Ne", destruction: "Taip");

            if (delete == "Ne")
                return;

            Button button = (Button)sender;
            Grid grid = (Grid)button.Parent;
            Label label = (Label)grid.Children[0];

            GetAddressResponseDto addr = addresses.First(x => x.FullAddress == label.Text);

            if(addr == null)
            {
                await DisplayAlert("Klaida", "Nepavyko ištrinti", "Uždaryti");
                return;
            }

            await apiClient.DeleteAsync($"http://localhost:50001/api/address/{addr.AddressId}");

            addresses.Clear();

            ApiResponse<List<GetAddressResponseDto>> response = await apiClient.GetAsync<List<GetAddressResponseDto>>($"http://localhost:50001/api/address/user/{Preferences.Get("user_id", "0")}");
            if (response.IsSuccess)
                foreach (GetAddressResponseDto address in response.Data)
                    addresses.Add(address);
        }

    }

    public partial class UserPage
    {
        private readonly ApiClient apiClient;

        private List<GetProviderResponseDto> providers = new List<GetProviderResponseDto>();

        private ObservableCollection<GetAddressResponseDto> addresses = new ObservableCollection<Models.Dtos.GetAddressResponseDto>();
        public ObservableCollection<GetAddressResponseDto> Addresses { get { return addresses; } }
    }
}

