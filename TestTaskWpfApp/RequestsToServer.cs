using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography.X509Certificates;
using System.Net;
using System.Windows.Controls;
using System.Windows.Media;
using System.Xml;
using System.IO;

namespace TestTaskWpfApp
{
    class RequestsToServer
    {
        public static bool IsCertificateFound = false;
        //Создадим делегат для передачи метода разбора ответа из разных справочников в качестве параметра
        public delegate void ParseXml(HttpWebResponse response, RichTextBox _richTextBox, TextBlock _textBlock); 

        internal static X509Certificate2 GetCertificate(string serialNumber, TextBlock _textBlock)
        {
            X509Certificate2 certificate = null;

            // Открываем хранилище сертификатов
            X509Store store = new X509Store("Root", StoreLocation.LocalMachine);
            store.Open(OpenFlags.ReadOnly);
            try
            {
                // Находим нужный сертификат
                X509Certificate2Collection Results = store.Certificates.Find(
                    X509FindType.FindBySerialNumber, serialNumber, false); // ищем по серийному номеру
                if (Results.Count == 0)
                {
                    throw new Exception("Сертификат не найден");
                }
                _textBlock.Foreground = Brushes.Red;
                _textBlock.Text = "Сертификат успешно найден";
                IsCertificateFound = true;
                certificate = Results[0];
                return certificate;
            }

            catch (Exception e)
            {
                _textBlock.Foreground = Brushes.Red;
                _textBlock.Text = "Ошибка: " + e.Message;
                return null;
            }

            finally
            {
                store.Close();
            }

        }

        public static void GetCertificateInfo(X509Certificate2 certificate,RichTextBox _richTextBox)
        {
            if(IsCertificateFound)
            {
                _richTextBox.Document.Blocks.Clear();
                _richTextBox.AppendText("Информация о найденном сертификате:\n");
                byte[] rawdata = certificate.RawData;
                _richTextBox.AppendText($"\nContent Type: {X509Certificate2.GetCertContentType(rawdata)}\n");
                _richTextBox.AppendText($"Friendly Name: {certificate.FriendlyName}\n");
                _richTextBox.AppendText($"Simple Name: {certificate.GetNameInfo(X509NameType.SimpleName, true)}\n");
                _richTextBox.AppendText($"Signature Algorithm: {certificate.SignatureAlgorithm.FriendlyName}\n");
                _richTextBox.AppendText($"Serial Number: {certificate.SerialNumber}\n");
                _richTextBox.AppendText($"Thumbprint: {certificate.Thumbprint}\n");
            }
        }

        public static void ProcessingRequest(X509Certificate2 certificate,string requestUri, RichTextBox _richTextBox, TextBlock _textBlock,ParseXml parseXmlAnswer)
        {
            if(IsCertificateFound)
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(requestUri);
                request.ClientCertificates.Add(certificate);
                _textBlock.Text = "Ждём ответа...";
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                parseXmlAnswer(response, _richTextBox, _textBlock); //вызываем переданный метод
            }
            else _textBlock.Foreground = Brushes.Red;

        }

        public static void ParseXmlGetErrorCodes(HttpWebResponse response, RichTextBox _richTextBox, TextBlock _textBlock)
        {
            using (Stream stream = response.GetResponseStream())
            {
                using (XmlTextReader reader = new XmlTextReader(stream))
                {
                    XmlDocument xDoc = new XmlDocument();
                    xDoc.Load(reader);
                    // получим корневой элемент
                    XmlElement xRoot = xDoc.DocumentElement;

                    _richTextBox.Document.Blocks.Clear();

                    // обход всех узлов в корневом элементе
                    foreach (XmlNode xnode in xRoot)
                    {
                        // получаем атрибут name
                        if (xnode.Attributes.Count > 0)
                        {
                            XmlNode code = xnode.Attributes.GetNamedItem("code");
                            if (code != null)
                                _richTextBox.AppendText($"Код ошибки: {code.Value}");

                            XmlNode text = xnode.Attributes.GetNamedItem("text");
                            if (text != null)
                                _richTextBox.AppendText($"\tТекст ошибки: {text.Value}\n");


                        }
                    }
                    _textBlock.Text = "Список ошибок успешно получен";
                    response.Close();
                }
            }
        }

        public static void ParseXmlGetCategories(HttpWebResponse response, RichTextBox _richTextBox, TextBlock _textBlock)
        {
            using (Stream stream = response.GetResponseStream())
            {
                using (XmlTextReader reader = new XmlTextReader(stream))
                {
                    XmlDocument xDoc = new XmlDocument();
                    xDoc.Load(reader);
                    // получим корневой элемент
                    XmlElement xRoot = xDoc.DocumentElement;

                    _richTextBox.Document.Blocks.Clear();

                    // обход всех узлов в корневом элементе
                    foreach (XmlNode xnode in xRoot)
                    {
 
                        if (xnode.Attributes.Count > 0)
                        {
                            XmlNode code = xnode.Attributes.GetNamedItem("id");
                            if (code != null)
                                _richTextBox.AppendText($"ID: {code.Value}");

                            XmlNode name = xnode.Attributes.GetNamedItem("name");
                            if (name != null)
                                _richTextBox.AppendText($"\t Name: {name.Value}");

                            XmlNode parent = xnode.Attributes.GetNamedItem("parent");
                            if (parent != null)
                                _richTextBox.AppendText($"\t Parent: {parent.Value}");

                            XmlNode image = xnode.Attributes.GetNamedItem("image");
                            if (image != null)
                                _richTextBox.AppendText($"\t Image: {image.Value}\n");

                        }
                    }
                    _textBlock.Text = "Список категорий успешно получен";
                    response.Close();
                }
            }
        }

    }
}
