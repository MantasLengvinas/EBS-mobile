using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EBS.Models.Dtos;
using EBS.Services;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace EBS
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AdminPage : TabbedPage
    {
        private readonly ApiClient apiClient;

        private ObservableCollection<GetUserResponseDto> users = new ObservableCollection<GetUserResponseDto>();
        public ObservableCollection<GetUserResponseDto> Users { get { return users; } }

        private ObservableCollection<GetAddressResponseDto> addresses = new ObservableCollection<GetAddressResponseDto>();
        public ObservableCollection<GetAddressResponseDto> Addresses { get { return addresses; } }

        public AdminPage ()
        {
            InitializeComponent();
            apiClient = new ApiClient();
            BindingContext = this;
            UsagesList.ItemsSource = users;
            AddressList.ItemsSource = addresses;
        }

        async protected override void OnAppearing()
        {
            await GetUsersAsync();
            await GetAddressesAsync();
        }

        private async Task GetUsersAsync()
        {
            users.Clear();

            Models.General.ApiResponse<List<GetUserResponseDto>> response = await apiClient.GetAsync<List<GetUserResponseDto>>($"http://localhost:50001/api/user");

            if (!response.IsSuccess)
                return;

            foreach(var user in response.Data)
            {
                users.Add(new GetUserResponseDto()
                {
                    ClientId = user.ClientId,
                    FullName = user.FullName,
                    Business = user.Business,
                    UserId = user.UserId,
                    Balance = user.Balance
                });
            }
        }

        private async Task GetAddressesAsync()
        {
            addresses.Clear();

            Models.General.ApiResponse<List<GetAddressResponseDto>> response = await apiClient.GetAsync<List<GetAddressResponseDto>>($"http://localhost:50001/api/address");

            if (response.IsSuccess)
            {
                addresses.Clear();
                foreach (var addr in response.Data)
                    addresses.Add(addr);
            }
        }

        public async void DeleteUser(object sender, EventArgs e)
        {
            string delete = await DisplayActionSheet("Ar tikrai norite ištrinti šį klientą?", cancel: "Ne", destruction: "Taip");

            if (delete == "Ne")
                return;

            Button button = (Button)sender;
            Grid grid = (Grid)button.Parent;
            Label label = (Label)grid.Children[0];

            GetUserResponseDto addr = users.First(x => x.UserId.ToString() == label.Text);

            if (addr == null)
            {
                await DisplayAlert("Klaida", "Nepavyko ištrinti", "Uždaryti");
                return;
            }

            await apiClient.DeleteAsync($"http://localhost:50001/api/user/{addr.UserId}");

            await GetUsersAsync();
        }

        public async void DeleteAddress(object sender, EventArgs e)
        {
            string delete = await DisplayActionSheet("Ar tikrai norite ištrinti šį adresą?", cancel: "Ne", destruction: "Taip");

            if (delete == "Ne")
                return;

            Button button = (Button)sender;
            Grid grid = (Grid)button.Parent;
            Label label = (Label)grid.Children[0];

            GetAddressResponseDto addr = addresses.First(x => x.FullAddress.ToString() == label.Text);

            if (addr == null)
            {
                await DisplayAlert("Klaida", "Nepavyko ištrinti", "Uždaryti");
                return;
            }

            await apiClient.DeleteAsync($"http://localhost:50001/api/address/{addr.AddressId}");

            await GetAddressesAsync();
        }
    }
}
