using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Windows.Media;
using System.Xml;
using NLog;
using System.Text;

namespace TestTaskWpfApp
{
    class DataManager
    {
        private static Logger logger = LogManager.GetCurrentClassLogger(); //объявляем обьект класса LOgger для записи логов
        public string result; //здесь будет храниться вся информация об ошибках и результатах выполнения методов
        public SolidColorBrush resultColor; //здесь будет храниться цвет результата

        #region Certificate
        public X509Certificate2 GetCertificate(string serialNumber) //получение сертификата
        {
            try
            {
                X509Certificate2 certificate = null;

                // Открываем хранилище сертификатов
                using (X509Store store = new X509Store("My", StoreLocation.CurrentUser))
                {
                    logger.Info("Открываем хранилище сертификатов");
                    store.Open(OpenFlags.ReadOnly);
                    // Находим нужный сертификат
                    X509Certificate2Collection Results = store.Certificates.Find(X509FindType.FindBySerialNumber, serialNumber, false); // ищем по серийному номеру
                    if (Results.Count == 0)
                    {
                        throw new Exception("Сертификат не найден");
                    }
                    certificate = Results[0];
                }

                //вывод информации
                result = "GetCertificate: Сертификат успешно найден";
                logger.Info(result);
                resultColor = Brushes.Green;

                return certificate;
            }
            catch(Exception ex)
            {
                result = $"GetCertificate: {ex.Message}";
                logger.Error(result);
                resultColor = Brushes.Red;
                return null;
            }
        }

        public string GetCertificateInfo(X509Certificate2 certificate) //метод для получения информации о найденном сертификате
        {

            if (certificate != null)
            {
                byte[] rawdata = certificate.RawData;
                StringBuilder certInfo = new StringBuilder("Информация о найденном сертификате:\n");

                certInfo.Append($"Content Type: {X509Certificate2.GetCertContentType(rawdata)}\n");
                certInfo.Append($"Friendly Name: {certificate.FriendlyName}\n");
                certInfo.Append($"Simple Name: {certificate.GetNameInfo(X509NameType.SimpleName, true)}\n");
                certInfo.Append($"Signature Algorithm: {certificate.SignatureAlgorithm.FriendlyName}\n");
                certInfo.Append($"Serial Number: {certificate.SerialNumber}\n");
                certInfo.Append($"Thumbprint: {certificate.Thumbprint}\n");

                logger.Info(certInfo.ToString());
                return certInfo.ToString(); 
            }
            else return null;
        }
        #endregion

        #region Data

        public XmlDocument RequestToServer(X509Certificate2 certificate, string requestUri) //отправляет запрос и получает XML ответ
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(requestUri); //отправляем запрос
                request.ClientCertificates.Add(certificate); //прикрепляем сертификат
                HttpWebResponse response = (HttpWebResponse)request.GetResponse(); //получаем ответ
                using (Stream stream = response.GetResponseStream()) //получаем поток ответа
                {
                    using (XmlTextReader reader = new XmlTextReader(stream)) //поток ответа в формате XML
                    {
                        XmlDocument xmlDoc = new XmlDocument();//создаем XML документ
                        xmlDoc.Load(reader);// записываем поток в XML документ
                        logger.Info($"RequestToServer: Получили XML ответ от сервера по запросу {requestUri}");
                        logger.Info(XmlToLog(xmlDoc));
                        return xmlDoc;
                    }
                }
            }
            catch (Exception ex)
            {
                result = $"RequestToServer: {ex.Message}";
                logger.Error(result);
                resultColor = Brushes.Red;
                return null;
            }
        }
        public DataXmlSql ParseXml(XmlDocument xmlDoc, string formatXml) //парсим XML ответ и записываем в класс
        {
            try
            {
                DataXmlSql Data = new DataXmlSql(); //обьект класса, в котором будет храниться информация XML ответа

                switch (formatXml)
                {
                    case "ErrorCodes": //Если парсим ErrorCode
                        Data.ErrorCodesValues(); //выставляем все необходимые настройки и задаёт вспомогательные переменные для разбора Errorcodes
                        break;
                    case "Categories": //Если парсим Categories
                        Data.CategoriesValues();//выставляем все необходимые настройки и задаёт вспомогательные переменные для разбора Categories
                        break;
                    default:
                        throw new Exception("Передан неверный формата разбора XML");
                }

                // получим корневой элемент
                XmlElement xRoot = xmlDoc.DocumentElement;
                int m = Data.xmlAttributes.Length; //количество атрибутов в одном узле
                // обход всех узлов в корневом элементе
                foreach (XmlNode xnode in xRoot)
                {
                    if (xnode.Attributes.Count > 0) // если есть атрибуты в узле
                    {
                        DataRow row=Data.table.NewRow();
                        for (int j = 0; j < m; j++) // пройдёмся по всем атрибутам
                        {
                            XmlNode attribute = xnode.Attributes.GetNamedItem(Data.xmlAttributes[j]); // получаем атрибут
                            // получаем значение атрибута
                            if (attribute != null)
                                row[Data.xmlAttributes[j]] = attribute.Value;
                        }
                        Data.table.Rows.Add(row);
                    }
                }
                logger.Info($"ParseXml: XML ответ [{Data.name}] успешно спарсили");
                return Data;
            }
            catch(Exception ex)
            {
                result = $"ParseXml: {ex.Message}";
                logger.Error(result);
                resultColor = Brushes.Red;
                return null;
            }
        }
        private string XmlToLog(XmlDocument xmlDoc) //метод для записи Xml в лог
        {
            string result = "";
            StringWriter stringWriter = new StringWriter(); //создаем объект записи Xml и для создания объекта XmlTextWriter
            try
            {
                XmlTextWriter xmlTextWriter = new XmlTextWriter(stringWriter); // создаем для форматирования xml-документа
                xmlTextWriter.Formatting = Formatting.Indented; //дочерние элементы разделяются отступами
                xmlDoc.WriteContentTo(xmlTextWriter);
                //xmlTextWriter.Flush(); //записывает в базовый поток данные из буфера
                result = stringWriter.ToString();
                xmlTextWriter.Close();
                return result;
            }
            catch(Exception ex)
            {
                logger.Error($"XmlToLog: {ex.Message}");
                return null;
            }
        }
        #endregion

        #region Database

        public void WriteToDatabase(string SqlServer, string Database, DataXmlSql Data) //записываем из класса в БД
        {
            try
            {
                string connectionString = $@"Data Source=.\{SqlServer};Initial Catalog={Database};Integrated Security=True"; // строка подключения к базе

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    SqlCommand command = new SqlCommand(Data.sqlStoredProcedure, connection);
                    // указываем, что команда представляет хранимую процедуру
                    command.CommandType = CommandType.StoredProcedure;
                    // параметр для ввода имени
                    SqlParameter tableParam = new SqlParameter
                    {
                        ParameterName = "@table",
                        Value = Data.table
                    };
                    // добавляем параметр
                    command.Parameters.Add(tableParam);
                    var res = command.ExecuteNonQuery();

                    result = $"WriteToDatabase: [{Data.name}] успешно записан в БД.";
                    logger.Info(result);
                    resultColor = Brushes.Green;
                }
            }
            catch (Exception ex)
            {
                result = $"WriteToDatabase: {ex.Message}";
                resultColor = Brushes.Red;
                logger.Error(result);
            }
        }

        public DataView GetDataFromDatabase (string SqlServer, string Database, string TableName) //возвращает таблицы из БД для записи в DataGrid
        {
            try
            {
                string sql = $"SELECT * FROM {TableName}"; //выбираем данные из таблицы

                string connectionString = $@"Data Source=.\{SqlServer};Initial Catalog={Database};Integrated Security=True"; //строка подключения
                using (SqlConnection connection = new SqlConnection(connectionString)) //подключаемся к БД
                {
                    SqlCommand command = new SqlCommand(sql, connection);//команда SELECT 
                    SqlDataAdapter adapter = new SqlDataAdapter(command); //создадим обьект SqlDataAdapter для заполнения DataTable
                    DataTable Table = new DataTable(); //создаём таблицу

                    connection.Open(); //открываем подключение
                    adapter.Fill(Table); //заполняем таблицу
                    logger.Info("GetDataFromDatabase: Успех");
                    return Table.DefaultView; //представление таблицы, которое можно записать в DataGrid
                }
            }
            catch (Exception ex)
            {
                result = $"WriteToDatabase: {ex.Message}";
                resultColor = Brushes.Red;
                logger.Error(result);
                return null;
            }
        }
        #endregion
    }
}
