using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Futarszolgalat_csomagkezelo
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
        }

        public class Package
        {
            public int Id { get; set; }
            public string PackageId { get; set; }
            public string SenderName { get; set; }
            public string RecipientName { get; set; }
            public string Address { get; set; }
            public string Phone { get; set; }
            public string PackageType { get; set; }
            public string Status { get; set; }
            public DateTime Date { get; set; }
            public string Notes { get; set; }

            public Package()
            {
                PackageId = "";
                SenderName = "";
                RecipientName = "";
                Address = "";
                Phone = "";
                PackageType = "";
                Status = "";
                Notes = "";
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
        }
    }
}