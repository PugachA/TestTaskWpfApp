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
using NLog;
using System.Threading;
using System.Net;
using System.Xml;
using System.Data;
using System.Data.SqlClient;

namespace TestTaskWpfApp
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        X509Certificate2 certificate = null;

        public MainWindow()
        {
            InitializeComponent();
            logger.Info("---------------------------------------------------------------------------------------------------------");
            logger.Info("Запущено приложение");
        }

        private void GetCercificate_Click(object sender, RoutedEventArgs e)
        {
            DataManager dataManager = new DataManager();
            logger.Info($"Пользователь нажал кнопку [{GetCercificate.Content}]");
            certificate = dataManager.GetCertificate(Properties.Settings.Default.CertSerialNumber);
            if (certificate != null)
                GetCercificate.IsEnabled = false;
            _textBlock.Text = dataManager.result;
            _textBlock.Foreground = dataManager.resultColor;
        }

        private void GetErrorCodes_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                logger.Info($"Пользователь нажал кнопку [{GetErrorCodes.Content}]");
                // создаем новый поток
                Thread myThread = new Thread(new ThreadStart(ProcessingErrorCodes));
                myThread.Start(); // запускаем поток
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                _textBlock.Text = ex.Message;
            }

        }

        private void GetCategories_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                logger.Info($"Пользователь нажал кнопку [{GetCategories.Content}]");
                // создаем новый поток
                Thread myThread = new Thread(new ThreadStart(ProcessingCategories));
                myThread.Start(); // запускаем поток
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                _textBlock.Text = ex.Message;
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            logger.Info("Пользователь нажал кнопку [Закрыть]");
            logger.Info("Приложение закрыто");
        }

        public void ProcessingErrorCodes()
        {
            try
            {
                DataManager dataManager = new DataManager();
                XmlDocument xmlDoc = dataManager.RequestToServer(certificate, Properties.Settings.Default.ErrorCodesUri);
                if (xmlDoc != null)
                {
                    DataXmlSql Data = dataManager.ParseXml(xmlDoc, "ErrorCodes");
                    if (Data != null)
                    {
                        dataManager.WriteToDatabase(Properties.Settings.Default.SqlServer, "InfoDirectory", Data);
                        Dispatcher.BeginInvoke((Action)(delegate { _dataGrid.ItemsSource = dataManager.GetDataFromDatabase(Properties.Settings.Default.SqlServer, "InfoDirectory", Data.TableName); }));
                    }
                }
                Dispatcher.BeginInvoke((Action)(delegate { _textBlock.Foreground = dataManager.resultColor; }));
                Dispatcher.BeginInvoke((Action)(delegate { _textBlock.Text = dataManager.result; }));
            }
            catch (Exception ex)
            {
                Dispatcher.BeginInvoke((Action)(delegate { _textBlock.Foreground = Brushes.Red; }));
                Dispatcher.BeginInvoke((Action)(delegate { _textBlock.Text = $"ProcessingErrorCodes: {ex.Message}"; }));
                logger.Error($"ProcessingErrorCodes: {ex.Message}");
            }
        }

        public void ProcessingCategories()
        {
            try
            {
                DataManager dataManager = new DataManager();
                XmlDocument xmlDoc = dataManager.RequestToServer(certificate, Properties.Settings.Default.CategoriesUri);
                if (xmlDoc != null)
                {
                    DataXmlSql Data = dataManager.ParseXml(xmlDoc, "Categories");
                    if (Data != null)
                    {
                        dataManager.WriteToDatabase(Properties.Settings.Default.SqlServer, "InfoDirectory", Data);
                        Dispatcher.BeginInvoke((Action)(delegate { _dataGrid.ItemsSource = dataManager.GetDataFromDatabase(Properties.Settings.Default.SqlServer, "InfoDirectory", Data.TableName); }));
                    }
                }
                Dispatcher.BeginInvoke((Action)(delegate { _textBlock.Foreground = dataManager.resultColor; }));
                Dispatcher.BeginInvoke((Action)(delegate { _textBlock.Text = dataManager.result; }));
            }
            catch (Exception ex)
            {
                Dispatcher.BeginInvoke((Action)(delegate { _textBlock.Foreground = Brushes.Red; }));
                Dispatcher.BeginInvoke((Action)(delegate { _textBlock.Text = $"ProcessingCategories: {ex.Message}"; }));
                logger.Error($"ProcessingCategories: {ex.Message}");
            }
        }
    }
}
