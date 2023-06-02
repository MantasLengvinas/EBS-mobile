using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EBS.Services;
using EBS.Models.Dtos;
using EBS.Models.General;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using Microcharts;
using SkiaSharp;
using System.Net.NetworkInformation;
using System.Drawing;

namespace EBS
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AddressPage : TabbedPage
    {
        private readonly ApiClient apiClient;

        private int AddressId { get; set; }
        public int ProviderId { get; set; }

        public int TariffId { get; set; }

        public AddressPage (int addressId, string fullAddress, int providerId)
        {
            InitializeComponent();
            AddressId = addressId;
            apiClient = new ApiClient();
            FullAddress = fullAddress;
            ProviderId = providerId;

            UsagesList.ItemsSource = usages;
            HistoryList.ItemsSource = history;

            BindingContext = this;
        }

        async protected override void OnAppearing()
        {
            await LoadUnpaidUsages();
            await LoadTariff();
        }

        async protected override void OnCurrentPageChanged()
        {
            int tabIndex = Children.IndexOf(CurrentPage);

            if(tabIndex == 0)
            {
                usages.Clear();
                // Load unpaid usages
                await LoadUnpaidUsages();

            }

            if(tabIndex == 1)
            {
                history.Clear();
                // Load history
                await LoadHistory();
            }

            if(tabIndex == 2)
            {
                history.Clear();
                usageEntries.Clear();
                usageDeltaEntries.Clear();
                paidEntries.Clear();
                await LoadHistory();

                if (history is null || history.Count == 0)
                    return;

                decimal? usageAvg = history.Sum(x => x.ElectricityDue) / history.Count;

                for (int i = 0; i < history.Count; i++)
                {
                    string color = "#0fd419"; // default (green)

                    if(history.Count > 1)
                    {
                        if (i != 0 && Math.Abs((history[i - 1].ElectricityDue - history[i].ElectricityDue).Value) > (history[i - 1].ElectricityDue.Value * 0.5M) )
                            color = "#eb3434";
                    }

                    usageEntries.Add(new ChartEntry(float.Parse(history[i].ElectricityDue.ToString()))
                    {
                        Label = history[i].ElectricityDue.ToString(),
                        ValueLabel = history[i].OnDate.Value.ToString("yyyy-MM-dd"),
                        ValueLabelColor = SKColor.Parse("#646464"),
                        Color = SKColor.Parse(color)
                    });
                }

                for(int i = 0; i < history.Count; i++)
                {
                    string color = "#0fd419";

                    if (history.Count > 1)
                    {
                        if (i != 0 && Math.Abs((history[i - 1].AmountDue - history[i].AmountDue).Value) > (history[i - 1].AmountDue.Value * 0.5M))
                            color = "#eb3434";
                    }

                    paidEntries.Add(new ChartEntry(float.Parse(history[i].AmountDue.ToString()))
                    {
                        Label = history[i].AmountDue.ToString(),
                        ValueLabel = history[i].OnDate.Value.ToString("yyyy-MM-dd"),
                        ValueLabelColor = SKColor.Parse("#646464"),
                        Color = SKColor.Parse(color)
                    });
                }

                decimal delta = 0;
                decimal prevDelta = 0;

                for(int i = 0; i < history.Count; i++)
                {
                    string color = "#0fd419";

                    if (i == 0)
                        continue;
                    else
                    {
                        delta = Math.Abs((history[i - 1].ElectricityDue - history[i].ElectricityDue).Value);
                        if (Math.Abs(prevDelta - delta) > 100)
                            color = "#eb3434";
                    }

                    prevDelta = delta;

                    usageDeltaEntries.Add(new ChartEntry(float.Parse(delta.ToString()))
                    {
                        Color = SKColor.Parse(color),
                        Label = delta.ToString(),
                        ValueLabelColor = SKColor.Parse("#646464"),
                        ValueLabel = history[i].OnDate.Value.ToString("yyyy-MM-dd")
                    });
                }

                UsagesChart.Chart = new LineChart { Entries = usageEntries, LabelTextSize = 38, LabelOrientation = Orientation.Horizontal };
                UsageDeltaChart.Chart = new LineChart { Entries = usageDeltaEntries, LabelTextSize = 38, LabelOrientation = Orientation.Horizontal };
                PaidChart.Chart = new PointChart { Entries = paidEntries, LabelTextSize = 38, PointMode = PointMode.Square };
            }
        }

        private async Task LoadHistory()
        {
            try
            {
                ApiResponse<GetUnpaidUsagesResponseDto> response = await apiClient.GetAsync<GetUnpaidUsagesResponseDto>($"http://localhost:50001/api/usage/history?id={AddressId}");

                if (!response.IsSuccess)
                    history.Add(new GetUsageResponseDto()
                    {
                        ShowNoDataMessage = true
                    });

                if (response.Data.Usages.Count > 0)
                {
                    response.Data.Usages = response.Data.Usages.OrderBy(x => x.OnDate).ToList();
                    foreach (GetUsageResponseDto addr in response.Data.Usages)
                        history.Add(new GetUsageResponseDto
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
                {
                    history.Add(new GetUsageResponseDto()
                    {
                        ShowNoDataMessage = true
                    });
                }

            }
            catch (Exception)
            {

            }

        }

        private async Task LoadTariff()
        {
            ApiResponse<List<GetTariffResponseDto>> response = new ApiResponse<List<GetTariffResponseDto>>();

            if (ProviderId == 1)
                response = await apiClient.GetAsync<List<GetTariffResponseDto>>($"http://localhost:50001/api/tariff?year=2022&month=9&providerId={ProviderId}");
            else
                response = await apiClient.GetAsync<List<GetTariffResponseDto>>($"http://localhost:50001/api/tariff?year=2022&month=10&providerId={ProviderId}");

            if (response.IsSuccess)
            {
                TariffId = response.Data.Where(x => !x.IsBusiness).Select(x => x.TariffId).FirstOrDefault();
            }
        }

        private async Task LoadUnpaidUsages()
        {
            usages.Clear();
            try
            {
                ApiResponse<GetUnpaidUsagesResponseDto> response = await apiClient.GetAsync<GetUnpaidUsagesResponseDto>($"http://localhost:50001/api/usage/unpaid?id={AddressId}");

                if (!response.IsSuccess)
                    usages.Add(new GetUsageResponseDto()
                    {
                        ShowNoDataMessage = true
                    });

                if (response.Data.Usages.Count > 0)
                {
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
                {
                    usages.Add(new GetUsageResponseDto()
                    {
                        ShowNoDataMessage = true
                    });
                }

            }
            catch (Exception)
            {
                
            }
        }

        public void RowSelected(object sender, EventArgs e)
        {
            TotalToPay = 0;

            foreach(var addr in Usages)
            {
                if (addr.Checked)
                    TotalToPay += addr.AmountDue.Value;
            }

            if (TotalToPay > 0)
                CanPay = true;
            else
                CanPay = false;

        }

        public async void PaySelectedUsages(object sender, EventArgs e)
        {
            List<GetUsageResponseDto> unpaid = Usages.Where(x => x.Checked).ToList();

            foreach(var u in unpaid)
            {
                await apiClient.PutAsync($"http://localhost:50001/api/usage/paid/{u.UsageId}");
            }

            await DisplayAlert("Sumokėta", "Sąskaitos sėkmingai sumokėtos", "Uždaryti");

            await LoadUnpaidUsages();
        }

        public async void AddUsage(object sender, EventArgs e)
        {
            string newUsage = await DisplayPromptAsync("Pridėti", "Įrašykite elektros sąnaudas", accept: "Išsaugoti", cancel: "Atšaukti", placeholder: "kWh");

            if(!double.TryParse(newUsage, out double usage))
            {
                await DisplayAlert("Neteisinga reikšmė", "Sąnaudos gali būti tik skaičiai", "Uždaryti");
                return;
            }

            if(usage <= 0)
            {
                await DisplayAlert("Neteisinga reikšmė", "Sąnaudos negali būti mažiau už 0", "Uždaryti");
                return;
            }

            PostUsageRequestDto usageRequest = new PostUsageRequestDto()
            {
                AddressId = AddressId,
                OnDate = DateTime.Now.Date,
                IsPaid = false,
                ElectricityDue = usage,
                PaidTariff = TariffId
            };

            ApiResponse<PostUsageRequestDto> res = await apiClient.PostAsync<PostUsageRequestDto>("http://localhost:50001/api/usage", usageRequest);

            if (res.IsSuccess)
            {
                await DisplayAlert("Išsaugota", "Sėkmingai išsaugoga", "Uždaryti");
                await LoadUnpaidUsages();
            }
            else
                await DisplayAlert("Klaida", "Nepavyko išsaugoti", "Uždaryti");
        }
    }

    public partial class AddressPage
    {
        private ObservableCollection<GetUsageResponseDto> history = new ObservableCollection<GetUsageResponseDto>();
        public ObservableCollection<GetUsageResponseDto> History { get { return history; } }


        private ObservableCollection<GetUsageResponseDto> usages = new ObservableCollection<GetUsageResponseDto>();
        public ObservableCollection<GetUsageResponseDto> Usages { get { return usages; } }

        public string FullAddress
        {
            get { return (string)GetValue(FullAddressProperty); }
            set { SetValue(FullAddressProperty, value); }
        }

        public static readonly BindableProperty FullAddressProperty = BindableProperty.Create(nameof(FullAddress), typeof(string), typeof(Label));

        private decimal totalToPay = 0;
        public decimal TotalToPay
        {
            get
            {
                return totalToPay;
            }
            set
            {
                totalToPay = value;

                OnPropertyChanged(nameof(TotalToPay));
            }
        }

        private bool canPay;
        public bool CanPay
        {
            get
            {
                return canPay;
            }
            set
            {
                canPay = value;
                OnPropertyChanged(nameof(CanPay));
            }
        }

        private List<ChartEntry> usageEntries = new List<ChartEntry>();
        private List<ChartEntry> usageDeltaEntries = new List<ChartEntry>();
        private List<ChartEntry> paidEntries = new List<ChartEntry>();
    }
}
