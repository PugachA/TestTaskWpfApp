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
            try
            {
                DataManager dataManager = new DataManager();
                logger.Info($"Пользователь нажал кнопку [{GetCercificate.Content}]");
                certificate = dataManager.GetCertificate("490000071cca86ffb77b3d794700020000071c");
                string CertInfo=dataManager.GetCertificateInfo(certificate);
                _richTextBox.Document.Blocks.Clear();
                _richTextBox.AppendText(CertInfo);
                _textBlock.Foreground = Brushes.Green;
                _textBlock.Text = "Сертификат успешно найден";
            }
            catch (Exception ex)
            {
                _textBlock.Foreground = Brushes.Red;
                _textBlock.Text = "Ошибка: " + ex.Message;
                logger.Error(ex.Message);
            }
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
                XmlDocument xmlDoc = dataManager.RequestToServer(certificate, "https://mkassa.byro.ru/payment2/?CmdId=get_errorscode");
                DataXmlSql Data = dataManager.ParseXml(xmlDoc, "ErrorCodes");
                if (Data != null)
                {
                    Dispatcher.BeginInvoke((Action)(delegate { _richTextBox.Document.Blocks.Clear(); }));
                    Dispatcher.BeginInvoke((Action)(delegate { _richTextBox.AppendText(Data.GetInfo()); }));
                }
                else throw new Exception("Ошибка смотри логи");
                dataManager.WriteToDatabase("SQLEXPRESS", "InfoDirectory", Data);

                Dispatcher.BeginInvoke((Action)(delegate { _textBlock.Foreground = Brushes.Green; }));
                Dispatcher.BeginInvoke((Action)(delegate { _textBlock.Text = $"{Data.name} успешно получен и записан в БД"; }));
            }
            catch (Exception ex)
            {
                Dispatcher.BeginInvoke((Action)(delegate { _textBlock.Foreground = Brushes.Red; }));
                Dispatcher.BeginInvoke((Action)(delegate { _textBlock.Text = ex.Message; }));
                logger.Error(ex.Message);
            }
        }

        public void ProcessingCategories()
        {
            try
            {
                DataManager dataManager = new DataManager();
                XmlDocument xmlDoc = dataManager.RequestToServer(certificate, "https://mkassa.byro.ru/payment2/?CmdId=GET_CATEGORIES");
                DataXmlSql Data = dataManager.ParseXml(xmlDoc, "Categories");
                if (Data != null)
                {
                    Dispatcher.BeginInvoke((Action)(delegate { _richTextBox.Document.Blocks.Clear(); }));
                    Dispatcher.BeginInvoke((Action)(delegate { _richTextBox.AppendText(Data.GetInfo()); }));
                }
                else throw new Exception("Ошибка смотри логи");
                dataManager.WriteToDatabase("SQLEXPRESS", "InfoDirectory", Data);

                Dispatcher.BeginInvoke((Action)(delegate { _textBlock.Foreground = Brushes.Green; }));
                Dispatcher.BeginInvoke((Action)(delegate { _textBlock.Text = $"{Data.name} успешно получен и записан в БД"; }));
            }
            catch (Exception ex)
            {
                Dispatcher.BeginInvoke((Action)(delegate { _textBlock.Foreground = Brushes.Red; }));
                Dispatcher.BeginInvoke((Action)(delegate { _textBlock.Text = ex.Message; }));
                logger.Error(ex.Message);
            }
        }
    }
}
