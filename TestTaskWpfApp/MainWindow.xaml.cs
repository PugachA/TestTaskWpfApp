using System;
using System.Windows;
using System.Windows.Media;
using System.Security.Cryptography.X509Certificates;
using NLog;
using System.Threading;
using System.Xml;
using System.Threading.Tasks;

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

                if (certificate == null)
                    throw new Exception("Перед тем как запросить список, нужно проверить  сертификат");

                // создаем новый поток
                Task errorCodeTask = Task.Factory.StartNew(ProcessingErrorCodes); //запускаем задачу по получению списка ошибок в отдельном потоке
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                _textBlock.Text = ex.Message;
                _textBlock.Foreground = Brushes.Red;
            }

        }

        private void GetCategories_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                logger.Info($"Пользователь нажал кнопку [{GetCategories.Content}]");

                if (certificate == null)
                    throw new Exception("Перед тем как запросить список, нужно проверить  сертификат");

                // создаем новый поток
                Task categoriesTask = Task.Factory.StartNew(ProcessingCategories); //запускаем задачу по получению списка категорий в отдельном потоке
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                _textBlock.Text = ex.Message;
                _textBlock.Foreground = Brushes.Red;
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
                        dataManager.WriteToDatabase(Properties.Settings.Default.SqlServer, Properties.Settings.Default.Database, Data);
                        Dispatcher.Invoke(delegate { _dataGrid.ItemsSource = dataManager.GetDataFromDatabase(Properties.Settings.Default.SqlServer, Properties.Settings.Default.Database, Data.TableName); });
                    }
                }
                Dispatcher.Invoke(delegate { _textBlock.Foreground = dataManager.resultColor; });
                Dispatcher.Invoke(delegate { _textBlock.Text = dataManager.result; });
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(delegate { _textBlock.Foreground = Brushes.Red; });
                Dispatcher.Invoke(delegate { _textBlock.Text = $"ProcessingErrorCodes: {ex.Message}"; });
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
                        dataManager.WriteToDatabase(Properties.Settings.Default.SqlServer, Properties.Settings.Default.Database, Data);
                        Dispatcher.Invoke(delegate { _dataGrid.ItemsSource = dataManager.GetDataFromDatabase(Properties.Settings.Default.SqlServer, Properties.Settings.Default.Database, Data.TableName); });
                    }
                }
                Dispatcher.Invoke(delegate { _textBlock.Foreground = dataManager.resultColor; });
                Dispatcher.Invoke(delegate { _textBlock.Text = dataManager.result; });
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(delegate { _textBlock.Foreground = Brushes.Red; });
                Dispatcher.Invoke(delegate { _textBlock.Text = $"ProcessingCategories: {ex.Message}"; });
                logger.Error($"ProcessingCategories: {ex.Message}");
            }
        }
    }
}
