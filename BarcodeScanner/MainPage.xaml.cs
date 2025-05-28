using System.Globalization;
using ZXing.Net.Maui;

namespace BarcodeScanner
{
    public partial class MainPage : ContentPage
    {
        private bool isDetecting = true;
        private List<string> scanHistory = new List<string>();

        public bool IsDetecting
        {
            get => isDetecting;
            set
            {
                isDetecting = value;
                OnPropertyChanged();
            }
        }

        public MainPage()
        {
            InitializeComponent();
            BindingContext = this;

            // Configure barcode reader options
            ConfigureBarcodeReader();

            // Request camera permission on startup
            RequestCameraPermission();
        }

        private void ConfigureBarcodeReader()
        {
            cameraBarcodeReaderView.Options = new BarcodeReaderOptions
            {
                Formats = BarcodeFormat.Code128 | BarcodeFormat.Code39 | BarcodeFormat.Code93 |
                         BarcodeFormat.Ean13 | BarcodeFormat.Ean8 | BarcodeFormat.QrCode |
                         BarcodeFormat.DataMatrix | BarcodeFormat.Pdf417,
                AutoRotate = true,
                Multiple = false,
                TryHarder = true
            };
        }

        private async void RequestCameraPermission()
        {
            var status = await Permissions.RequestAsync<Permissions.Camera>();
            if (status != PermissionStatus.Granted)
            {
                await DisplayAlert("Permission Required",
                    "Camera permission is required to scan barcodes.",
                    "OK");
            }
        }

        private void BarcodesDetected(object sender, BarcodeDetectionEventArgs e)
        {
            var first = e.Results?.FirstOrDefault();
            if (first is not null)
            {
                Dispatcher.Dispatch(() =>
                {
                    var result = first.Value;
                    var format = first.Format.ToString();

                    // Update UI
                    resultLabel.Text = result;
                    formatLabel.Text = $"รูปแบบ: {format}";

                    // Add to history
                    AddToHistory($"{DateTime.Now:HH:mm:ss} - {format}: {result}");

                    // Play sound or vibration feedback
                    HapticFeedback.Perform(HapticFeedbackType.Click);
                });
            }
        }

        private void AddToHistory(string scanResult)
        {
            scanHistory.Insert(0, scanResult);

            // Keep only last 10 items
            if (scanHistory.Count > 10)
            {
                scanHistory.RemoveAt(scanHistory.Count - 1);
            }

            UpdateHistoryDisplay();
        }

        private void UpdateHistoryDisplay()
        {
            historyStack.Children.Clear();

            foreach (var item in scanHistory)
            {
                var label = new Label
                {
                    Text = item,
                    FontSize = 12,
                    TextColor = Colors.DarkSlateGray,
                    Margin = new Thickness(0, 2)
                };
                historyStack.Children.Add(label);
            }
        }

        private void OnToggleClicked(object sender, EventArgs e)
        {
            IsDetecting = !IsDetecting;
            toggleButton.Text = IsDetecting ? "หยุดสแกน" : "เริ่มสแกน";
            toggleButton.BackgroundColor = IsDetecting ? Colors.DodgerBlue : Colors.Green;
        }

        private void OnClearClicked(object sender, EventArgs e)
        {
            resultLabel.Text = "กรุณาสแกน barcode";
            formatLabel.Text = "";
            scanHistory.Clear();
            historyStack.Children.Clear();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            IsDetecting = true;
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            IsDetecting = false;
        }
    }

}
