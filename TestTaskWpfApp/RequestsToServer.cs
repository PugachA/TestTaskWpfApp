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
using System.Data.SqlClient;
using NLog;

namespace TestTaskWpfApp
{
    class RequestsToServer
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        public static bool IsCertificateFound = false;
        //Создадим делегат для передачи метода разбора ответа из разных справочников в качестве параметра
        public delegate void ParseXml(HttpWebResponse response, RichTextBox _richTextBox, TextBlock _textBlock); 

        internal static X509Certificate2 GetCertificate(string serialNumber, TextBlock _textBlock)
        {
            X509Certificate2 certificate = null;

            // Открываем хранилище сертификатов
            X509Store store = new X509Store("My", StoreLocation.LocalMachine);
            logger.Info("Открываем хранилище сертификатов");
            store.Open(OpenFlags.ReadOnly);
            try
            {
                // Находим нужный сертификат
                X509Certificate2Collection Results = store.Certificates.Find(X509FindType.FindBySerialNumber, serialNumber, false); // ищем по серийному номеру
                if (Results.Count == 0)
                {
                    throw new Exception("Сертификат не найден");
                }
                _textBlock.Foreground = Brushes.Green;
                _textBlock.Text = "Сертификат успешно найден";
                logger.Info($"Сертификат c номером {serialNumber} успешно найден");
                IsCertificateFound = true;
                certificate = Results[0];
                return certificate;
            }

            catch (Exception e)
            {
                _textBlock.Foreground = Brushes.Red;
                _textBlock.Text = "Ошибка: " + e.Message;
                logger.Error(e.Message);
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
                byte[] rawdata = certificate.RawData;
                string Info="";
                Info = String.Concat(Info, "Информация о найденном сертификате:\n");
                Info = String.Concat(Info, $"Content Type: {X509Certificate2.GetCertContentType(rawdata)}\n");
                Info = String.Concat(Info, $"Friendly Name: {certificate.FriendlyName}\n");
                Info = String.Concat(Info, $"Simple Name: {certificate.GetNameInfo(X509NameType.SimpleName, true)}\n");
                Info = String.Concat(Info, $"Signature Algorithm: {certificate.SignatureAlgorithm.FriendlyName}\n");
                Info = String.Concat(Info, $"Serial Number: {certificate.SerialNumber}\n");
                Info = String.Concat(Info, $"Thumbprint: {certificate.Thumbprint}\n");

                _richTextBox.Document.Blocks.Clear();
                _richTextBox.AppendText(Info);
                //logger.Info(Info);
            }
        }

        public static void ProcessingRequest(X509Certificate2 certificate,string requestUri, RichTextBox _richTextBox, TextBlock _textBlock,ParseXml parseXmlAnswer)
        {
            try
            {
                if (IsCertificateFound)
                {
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(requestUri);
                    request.ClientCertificates.Add(certificate);
                    logger.Info($"Отправляем GET запрос {requestUri}");
                    HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                    parseXmlAnswer(response, _richTextBox, _textBlock); //вызываем переданный метод
                }
                else
                {
                    _textBlock.Foreground = Brushes.Red;
                }
            }
            catch (Exception e)
            {
                _textBlock.Foreground = Brushes.Red;
                _textBlock.Text = "Ошибка: " + e.Message;
                logger.Error(e.Message);
            }

        }

        public static void ParseXmlGetErrorCodes(HttpWebResponse response, RichTextBox _richTextBox, TextBlock _textBlock)
        {
            string[] Values = new string[2];

            using (Stream stream = response.GetResponseStream())
            {
                using (XmlTextReader reader = new XmlTextReader(stream))
                {
                    XmlDocument xDoc = new XmlDocument();
                    xDoc.Load(reader);
                    logger.Info($"Получен ответ {xDoc.OuterXml}"); //добавляется все в одну строчку не удаётся исправить!

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
                            {
                                _richTextBox.AppendText($"Код ошибки: {code.Value}");
                                Values[0] = code.Value;
                            }

                            XmlNode text = xnode.Attributes.GetNamedItem("text");
                            if (text != null)
                            {
                                _richTextBox.AppendText($"\tТекст ошибки: {text.Value}\n");
                                Values[1] = text.Value;
                            }

                        }

                        //запишем занчение в базу
                        string[] ParametersStoredProcudere = { "@code", "@text" };//параметры хранимой процедуры
                        WriteToDatabase("SQLEXPRESS", "InfoDirectory", "sp_AddErrorCode", Values, ParametersStoredProcudere);

                    }
                    _textBlock.Foreground = Brushes.Green;
                    logger.Info("Ответ [Список ошибок] разобран и записан в БД");
                    _textBlock.Text = "Список ошибок успешно получен и записан в БД";
                    response.Close();
                }
            }
        }

        public static void ParseXmlGetCategories(HttpWebResponse response, RichTextBox _richTextBox, TextBlock _textBlock)
        {
            string[] Values = new string[4];

            using (Stream stream = response.GetResponseStream())
            {
                using (XmlTextReader reader = new XmlTextReader(stream))
                {
                    XmlDocument xDoc = new XmlDocument();
                    xDoc.Load(reader);
                    logger.Info($"Получен ответ {xDoc.OuterXml}"); //добавляется все в одну строчку не удаётся исправить!

                    // получим корневой элемент
                    XmlElement xRoot = xDoc.DocumentElement;

                    _richTextBox.Document.Blocks.Clear();

                    // обход всех узлов в корневом элементе
                    foreach (XmlNode xnode in xRoot)
                    {
 
                        if (xnode.Attributes.Count > 0)
                        {
                            XmlNode id = xnode.Attributes.GetNamedItem("id");
                            if (id != null)
                            {
                                _richTextBox.AppendText($"ID: {id.Value}");
                                Values[0] = id.Value;
                            }
                                
                            XmlNode name = xnode.Attributes.GetNamedItem("name");
                            if (name != null)
                            {
                                _richTextBox.AppendText($"\t Name: {name.Value}");
                                Values[1] = name.Value;
                            }

                            XmlNode parent = xnode.Attributes.GetNamedItem("parent");
                            if (parent != null)
                            {
                                _richTextBox.AppendText($"\t Parent: {parent.Value}");
                                Values[2] = parent.Value;
                            }  

                            XmlNode image = xnode.Attributes.GetNamedItem("image");
                            if (image != null)
                            {
                                _richTextBox.AppendText($"\t Image: {image.Value}\n");
                                Values[3] = image.Value;
                            }

                            //запишем занчение в базу
                            string[] ParametersStoredProcudere = { "@id", "@name", "@parent", "@image" };//параметры хранимой процедуры
                            WriteToDatabase("SQLEXPRESS", "InfoDirectory", "sp_AddCategory", Values, ParametersStoredProcudere);

                        }
                    }
                    logger.Info("Ответ [Список категорий] разобран и записан в БД");
                    _textBlock.Foreground = Brushes.Green;
                    _textBlock.Text = "Список категорий успешно получен и записан в БД";
                    response.Close();
                }
            }
        }

        private static void WriteToDatabase(string SqlServer, string Database, string SqlStoredProcedure, string[] Values, string[] ParametersStoredProcedure)
        {
            string connectionString = $@"Data Source=.\{SqlServer};Initial Catalog={Database};Integrated Security=True"; //подключаемся к базе

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    SqlCommand command = new SqlCommand(SqlStoredProcedure, connection);
                    // указываем, что команда представляет хранимую процедуру
                    command.CommandType = System.Data.CommandType.StoredProcedure;

                    //пройдемся по всем параметрам и добавим их к команде
                    for (int i = 0; i < Values.Length; i++)
                    {
                        SqlParameter Param = new SqlParameter
                        {
                            ParameterName = ParametersStoredProcedure[i],
                            Value = Values[i]
                        };
                        //добавляем параметр
                        command.Parameters.Add(Param);

                    }

                    var result = command.ExecuteNonQuery(); //результат количество добавленных или обновленных строк
                }
        }

    }
}
