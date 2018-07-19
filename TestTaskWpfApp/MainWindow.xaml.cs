using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Security.Cryptography.X509Certificates;

namespace TestTaskWpfApp
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        X509Certificate2 certificate = null;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void GetCercificate_Click(object sender, RoutedEventArgs e)
        {
            certificate = RequestsToServer.GetCertificate("490000071cca86ffb77b3d794700020000071c", _textBlock);
            RequestsToServer.GetCertificateInfo(certificate, _richTextBox);
        }

        private void GetErrorCodes_Click(object sender, RoutedEventArgs e)
        {
            RequestsToServer.ProcessingRequest(certificate, "https://mkassa.byro.ru/payment2/?CmdId=get_errorscode", _richTextBox,_textBlock, RequestsToServer.ParseXmlGetErrorCodes);
        }

        private void GetCategories_Click(object sender, RoutedEventArgs e)
        {
            RequestsToServer.ProcessingRequest(certificate, "https://mkassa.byro.ru/payment2/?CmdId=GET_CATEGORIES", _richTextBox, _textBlock, RequestsToServer.ParseXmlGetCategories);
        }
    }
}
