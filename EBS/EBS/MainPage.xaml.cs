using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using EBS.Models.Dtos;
using System.Net.Http;
using EBS.Services;
using EBS.Models.General;
using Xamarin.Essentials;

namespace EBS
{
    public partial class MainPage : ContentPage
    {
        private readonly ApiClient apiClient;

        private ObservableCollection<GetAddressResponseDto> addresses = new ObservableCollection<GetAddressResponseDto>();
        public ObservableCollection<GetAddressResponseDto> Addresses { get { return addresses; } }

        public MainPage()
        {
            InitializeComponent();
            apiClient = new ApiClient();
            BindingContext = this;
            PageData.ItemsSource = addresses;
        }

        async override protected void OnAppearing()
        {
            addresses.Clear();

            ApiResponse<List<GetAddressResponseDto>> response = await apiClient.GetAsync<List<GetAddressResponseDto>>($"http://localhost:50001/api/address/user/{Preferences.Get("user_id", "0")}");
            if (response.IsSuccess)
                foreach (GetAddressResponseDto addr in response.Data)
                    addresses.Add(addr);
        }
    }
}

