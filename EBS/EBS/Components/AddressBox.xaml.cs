using System;
using System.Collections.Generic;
using EBS.Models.Dtos;
using EBS.Models.General;
using System.Collections.ObjectModel;
using Xamarin.Forms;
using System.ComponentModel;
using System.Threading.Tasks;
using EBS.Services;
using System.Runtime.CompilerServices;

namespace EBS.Components
{	
	public partial class AddressBox : ContentView
	{
        private ApiClient apiClient;

        private ObservableCollection<GetUsageResponseDto> usages = new ObservableCollection<GetUsageResponseDto>();
        public ObservableCollection<GetUsageResponseDto> Usages { get { return usages; } }

        public AddressBox()
        {
            InitializeComponent();
            UsagesList.ItemsSource = usages;
            apiClient = new ApiClient();
        }

        async protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            if (propertyName == nameof(AddressId))
                await LoadUsages();
        }

        private async Task LoadUsages() 
        {
            usages.Clear();

            ApiResponse<GetUnpaidUsagesResponseDto> response = await apiClient.GetAsync<GetUnpaidUsagesResponseDto>($"http://localhost:50001/api/usage/unpaid?id={AddressId}");
            if (response.IsSuccess)
            {
                if(response.Data.Usages.Count == 0)
                {
                    usages.Add(new GetUsageResponseDto()
                    {
                        ShowNoDataMessage = true
                    });
                    return;
                }

                foreach (GetUsageResponseDto addr in response.Data.Usages)
                    usages.Add(new GetUsageResponseDto
                    {
                        UsageId = addr.UsageId,
                        OnDate = addr.OnDate,
                        AddressId = addr.AddressId,
                        ElectricityDue = decimal.Round(addr.ElectricityDue.Value, 2),
                        AmountDue = decimal.Round(addr.AmountDue.Value, 2),
                        IsPaid = addr.IsPaid,
                        PaidTariff = decimal.Round(addr.PaidTariff.Value, 2)
                    });
            }
            else
                usages.Add(new GetUsageResponseDto()
                {
                    ShowNoDataMessage = true
                });
        }

        public async void OpenAddress(Object sender, EventArgs e)
        {

            await Application.Current.MainPage.Navigation.PushAsync(new AddressPage(AddressId, FullAddress, ProviderId));

        }
    }

    public partial class AddressBox // Properties
    {
        public string FullAddress
        {
            get { return (string)GetValue(FullAddressProperty); }
            set { SetValue(FullAddressProperty, value); }
        }
        public static readonly BindableProperty FullAddressProperty = BindableProperty.Create(nameof(FullAddress), typeof(string), typeof(Label));

        public int AddressId
        {
            get { return (int)GetValue(AddressIdProperty); }
            set { SetValue(AddressIdProperty, value); }
        }
        public static readonly BindableProperty AddressIdProperty = BindableProperty.Create(nameof(AddressId), typeof(int), typeof(Label));

        public int ProviderId
        {
            get { return (int)GetValue(ProviderIdProperty); }
            set { SetValue(ProviderIdProperty, value); }
        }
        public static readonly BindableProperty ProviderIdProperty = BindableProperty.Create(nameof(ProviderId), typeof(int), typeof(Label));

    }
}

