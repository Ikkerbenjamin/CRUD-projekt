using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Newtonsoft.Json;

namespace WpfApp1
{
    public partial class MainWindow : Window
    {
        private List<Package> packages;
        private string dataFile = "packages.json";
        private string originalDataFile = "data/data.json";

        public MainWindow()
        {
            InitializeComponent();
            InitializeData();
            LoadPackageTypes();
            LoadStatusTypes();
            RefreshListBox();
            ClearForm();
            UpdateStats();
        }

        public class Package
        {
            public int Id { get; set; }
            public string PackageId { get; set; } = "";
            public string SenderName { get; set; } = "";
            public string RecipientName { get; set; } = "";
            public string Address { get; set; } = "";
            public string Phone { get; set; } = "";
            public string PackageType { get; set; } = "";
            public string Status { get; set; } = "";
            public DateTime Date { get; set; }
            public string Notes { get; set; } = "";
            public decimal Weight { get; set; }
            public decimal Price { get; set; }

            public string StatusColor
            {
                get
                {
                    return Status switch
                    {
                        "Feladva" => "#FF4A6DA7",
                        "Raktáron" => "#FF6B8E23",
                        "Szállítás alatt" => "#FFFFC107",
                        "Kiszállítva" => "#FF28A745",
                        "Probléma" => "#FFDC3545",
                        "Visszaküldve" => "#FF6C757D",
                        _ => "#FF6C757D"
                    };
                }
            }
        }

        public class JsonData
        {
            public List<string> packageTypes { get; set; }
            public List<JsonShipment> shipments { get; set; }
        }

        public class JsonShipment
        {
            public int PackageId { get; set; }
            public string RecipientName { get; set; }
            public string address { get; set; }
            public string packageType { get; set; }
            public string deliveryMode { get; set; }
            public bool Status { get; set; }
            public string sendDate { get; set; }
            public string phoneNumber { get; set; }
            public string sender { get; set; }
        }

        private void InitializeData()
        {
            packages = new List<Package>();

            if (File.Exists(dataFile))
            {
                try
                {
                    string json = File.ReadAllText(dataFile);
                    packages = JsonConvert.DeserializeObject<List<Package>>(json) ?? new List<Package>();
                }
                catch
                {
                    packages = new List<Package>();
                }
            }
            else if (File.Exists(originalDataFile))
            {
                try
                {
                    string json = File.ReadAllText(originalDataFile);
                    JsonData jsonData = JsonConvert.DeserializeObject<JsonData>(json);

                    if (jsonData != null && jsonData.shipments != null)
                    {
                        foreach (JsonShipment shipment in jsonData.shipments)
                        {
                            Package package = new Package
                            {
                                Id = shipment.PackageId,
                                PackageId = "PKG" + shipment.PackageId.ToString("D4"),
                                RecipientName = shipment.RecipientName,
                                Address = shipment.address,
                                PackageType = shipment.packageType,
                                Date = DateTime.TryParse(shipment.sendDate, out DateTime sendDate) ? sendDate : DateTime.Now,
                                Phone = shipment.phoneNumber,
                                SenderName = shipment.sender,
                                Notes = "Automatikusan betöltött adat",
                                Weight = 0,
                                Price = 0
                            };

                            if (shipment.Status)
                            {
                                package.Status = "Kiszállítva";
                            }
                            else
                            {
                                package.Status = "Szállítás alatt";
                            }

                            packages.Add(package);
                        }
                    }

                    if (jsonData != null && jsonData.packageTypes != null)
                    {
                        PackageTypeComboBox.ItemsSource = jsonData.packageTypes;
                        if (jsonData.packageTypes.Count > 0)
                            PackageTypeComboBox.SelectedIndex = 0;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Hiba történt az adatok betöltésekor: {ex.Message}");
                    packages = new List<Package>();
                }
            }
            else
            {
                packages = new List<Package>();
            }
        }

        private void LoadPackageTypes()
        {
            if (PackageTypeComboBox.ItemsSource == null || PackageTypeComboBox.Items.Count == 0)
            {
                List<string> packageTypes = new List<string>
                {
                    "Dokumentum",
                    "Kis csomag",
                    "Közepes csomag",
                    "Nagy csomag",
                    "Törékeny csomag",
                    "Hűtött áru"
                };

                PackageTypeComboBox.ItemsSource = packageTypes;
                if (packageTypes.Count > 0)
                    PackageTypeComboBox.SelectedIndex = 0;
            }
        }

        private void LoadStatusTypes()
        {
            List<string> statusTypes = new List<string>
            {
                "Feladva",
                "Raktáron",
                "Szállítás alatt",
                "Kiszállítva",
                "Probléma",
                "Visszaküldve"
            };

            StatusComboBox.ItemsSource = statusTypes;
            if (statusTypes.Count > 0)
                StatusComboBox.SelectedIndex = 0;
        }

        private void SaveData()
        {
            try
            {
                string json = JsonConvert.SerializeObject(packages, Newtonsoft.Json.Formatting.Indented);
                File.WriteAllText(dataFile, json);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Hiba történt az adatok mentésekor: {ex.Message}");
            }
        }

        private void RefreshListBox()
        {
            ItemsListBox.ItemsSource = null;
            ItemsListBox.ItemsSource = packages;
            UpdateStats();
        }

        private void UpdateStats()
        {
            if (packages == null) return;

            int total = packages.Count;
            int inProgress = packages.Count(p => p.Status == "Szállítás alatt");
            int delivered = packages.Count(p => p.Status == "Kiszállítva");

            TotalPackagesText.Text = $"Összes: {total}";
            InProgressText.Text = $"Folyamatban: {inProgress}";
            DeliveredText.Text = $"Kiszállítva: {delivered}";

            PackageCountText.Text = $"{total} aktív csomag";
        }

        private void ClearForm()
        {
            // Automatikusan generált csomag ID
            int nextId = packages.Count > 0 ? packages.Max(p => p.Id) + 1 : 1;
            PackageIdTextBox.Text = $"CSM{DateTime.Now:yyMMdd}{nextId:D4}";

            SenderTextBox.Clear();
            RecipientTextBox.Clear();
            AddressTextBox.Clear();
            PhoneTextBox.Clear();
            if (PackageTypeComboBox.Items.Count > 0)
                PackageTypeComboBox.SelectedIndex = 0;
            if (StatusComboBox.Items.Count > 0)
                StatusComboBox.SelectedIndex = 0;
            DatePicker.SelectedDate = DateTime.Now;
            NotesTextBox.Clear();
            FormTitle.Text = "Új csomag felvétele";
            DeleteButton.Visibility = Visibility.Collapsed;
            UpdateButton.Visibility = Visibility.Collapsed;
            MarkDeliveredButton.Visibility = Visibility.Collapsed;
            UpdateStatusButton.Visibility = Visibility.Collapsed;
            AddButton.Visibility = Visibility.Visible;
        }

        private string GeneratePackageId()
        {
            int nextId = packages.Count > 0 ? packages.Max(p => p.Id) + 1 : 1;
            return $"CSM{DateTime.Now:yyMMdd}{nextId:D4}";
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(RecipientTextBox.Text) ||
                string.IsNullOrWhiteSpace(AddressTextBox.Text))
            {
                MessageBox.Show("Címzett és Cím kötelező!",
                    "Figyelem", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            Package newPackage = new Package
            {
                Id = packages.Count > 0 ? packages.Max(p => p.Id) + 1 : 1,
                PackageId = PackageIdTextBox.Text,
                SenderName = SenderTextBox.Text,
                RecipientName = RecipientTextBox.Text,
                Address = AddressTextBox.Text,
                Phone = PhoneTextBox.Text,
                PackageType = PackageTypeComboBox.SelectedItem as string ?? "",
                Status = StatusComboBox.SelectedItem as string ?? "",
                Date = DatePicker.SelectedDate ?? DateTime.Now,
                Notes = NotesTextBox.Text
            };

            packages.Add(newPackage);
            SaveData();
            RefreshListBox();
            ClearForm();
        }

        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            if (ItemsListBox.SelectedItem is Package selectedPackage)
            {
                if (string.IsNullOrWhiteSpace(RecipientTextBox.Text) ||
                    string.IsNullOrWhiteSpace(AddressTextBox.Text))
                {
                    MessageBox.Show("Címzett és Cím kötelező!",
                        "Figyelem", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Csomag ID-t nem változtatjuk, az automatikusan generált marad
                selectedPackage.SenderName = SenderTextBox.Text;
                selectedPackage.RecipientName = RecipientTextBox.Text;
                selectedPackage.Address = AddressTextBox.Text;
                selectedPackage.Phone = PhoneTextBox.Text;
                selectedPackage.PackageType = PackageTypeComboBox.SelectedItem as string ?? "";
                selectedPackage.Status = StatusComboBox.SelectedItem as string ?? "";
                selectedPackage.Date = DatePicker.SelectedDate ?? DateTime.Now;
                selectedPackage.Notes = NotesTextBox.Text;

                SaveData();
                RefreshListBox();
                ClearForm();
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (ItemsListBox.SelectedItem is Package selectedPackage)
            {
                MessageBoxResult result = MessageBox.Show(
                    "Biztosan törölni szeretnéd ezt a csomagot?",
                    "Megerősítés",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    packages.Remove(selectedPackage);
                    SaveData();
                    RefreshListBox();
                    ClearForm();
                }
            }
        }

        private void MarkDeliveredButton_Click(object sender, RoutedEventArgs e)
        {
            if (ItemsListBox.SelectedItem is Package selectedPackage)
            {
                selectedPackage.Status = "Kiszállítva";
                SaveData();
                RefreshListBox();
                ClearForm();

                MessageBox.Show(
                    "Csomag státusza sikeresen frissítve: Kiszállítva",
                    "Sikeres",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
        }

        private void UpdateStatusButton_Click(object sender, RoutedEventArgs e)
        {
            if (ItemsListBox.SelectedItem is Package selectedPackage)
            {
                if (selectedPackage.Status == "Feladva")
                {
                    selectedPackage.Status = "Szállítás alatt";
                }
                else if (selectedPackage.Status == "Szállítás alatt")
                {
                    selectedPackage.Status = "Probléma";
                }
                else if (selectedPackage.Status == "Probléma")
                {
                    selectedPackage.Status = "Visszaküldve";
                }
                else
                {
                    selectedPackage.Status = "Feladva";
                }

                SaveData();
                RefreshListBox();

                MessageBox.Show(
                    "Státusz sikeresen frissítve!",
                    "Sikeres",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
        }

        private void ItemsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ItemsListBox.SelectedItem is Package selectedPackage)
            {
                PackageIdTextBox.Text = selectedPackage.PackageId;
                SenderTextBox.Text = selectedPackage.SenderName;
                RecipientTextBox.Text = selectedPackage.RecipientName;
                AddressTextBox.Text = selectedPackage.Address;
                PhoneTextBox.Text = selectedPackage.Phone;
                PackageTypeComboBox.SelectedItem = selectedPackage.PackageType;
                StatusComboBox.SelectedItem = selectedPackage.Status;
                DatePicker.SelectedDate = selectedPackage.Date;
                NotesTextBox.Text = selectedPackage.Notes;

                FormTitle.Text = "Csomag szerkesztése";
                DeleteButton.Visibility = Visibility.Visible;
                UpdateButton.Visibility = Visibility.Visible;
                MarkDeliveredButton.Visibility = Visibility.Visible;
                UpdateStatusButton.Visibility = Visibility.Visible;
                AddButton.Visibility = Visibility.Collapsed;
            }
        }
    }
}