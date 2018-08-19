using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Windows.Media;
using System.Xml;
using NLog;

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
                string Info = "";

                Info = String.Concat(Info, "Информация о найденном сертификате:\n");
                Info = String.Concat(Info, $"Content Type: {X509Certificate2.GetCertContentType(rawdata)}\n");
                Info = String.Concat(Info, $"Friendly Name: {certificate.FriendlyName}\n");
                Info = String.Concat(Info, $"Simple Name: {certificate.GetNameInfo(X509NameType.SimpleName, true)}\n");
                Info = String.Concat(Info, $"Signature Algorithm: {certificate.SignatureAlgorithm.FriendlyName}\n");
                Info = String.Concat(Info, $"Serial Number: {certificate.SerialNumber}\n");
                Info = String.Concat(Info, $"Thumbprint: {certificate.Thumbprint}\n");

                logger.Info(Info);
                return Info;
                
            }
            else return null;
        }
        #endregion

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

                if (formatXml == "ErrorCodes") //Если парсим ErrorCodes
                {
                    Data.ErrorCodesValues(); //выставляем все необходимые настройки и задаёт вспомогательные переменные для разбора Errorcodes
                }
                else if (formatXml == "Categories") //Если парсим Categories
                {
                    Data.CategoriesValues();//выставляем все необходимые настройки и задаёт вспомогательные переменные для разбора Categories
                }
                else throw new Exception("Передан неверный формата разбора XML");

                // получим корневой элемент
                XmlElement xRoot = xmlDoc.DocumentElement;
                int i = 0; //счетчик в цикле
                int n = xRoot.ChildNodes.Count; //количество узлов в корневом элементе
                int m = Data.xmlAttributes.Length; //количество атрибутов в одном узле
                Data.value = new string[n, m]; // задеём размерность двумерного массива
                // обход всех узлов в корневом элементе
                foreach (XmlNode xnode in xRoot)
                {
                    if (xnode.Attributes.Count > 0) // если есть атрибуты в узле
                    {
                        for (int j = 0; j < m; j++) // пройдёмся по всем атрибутам
                        {
                            XmlNode attribute = xnode.Attributes.GetNamedItem(Data.xmlAttributes[j]); // получаем атрибут
                            // получаем значение атрибута
                            if (attribute != null)
                                Data.value[i, j] = attribute.Value;
                        }
                    }
                    i++;
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

        #region Database
        public void WriteToDatabase(string SqlServer, string Database, DataXmlSql Data) //записываем из класса в БД
        {
            try
            {
                string connectionString = $@"Data Source=.\{SqlServer};Initial Catalog={Database};Integrated Security=True"; // строка подключения к базе

                string commandText = $"DECLARE @table AS {Data.TableType};"; //текстовая команда объявления переменной табличного типа

                for (int i = 0; i < Data.value.GetUpperBound(0) + 1; i++) //пройдёмся по всем строкам массива в классе
                {
                    //создадим скрипт для записи значений переменную табличного типа
                    commandText = String.Concat(commandText, $"INSERT INTO @table ({Data.TableTypeValues}) Values  ({Data.value[i,0]}");  //добавляем команду и первый элемент строки
                    for (int j = 1; j < Data.value.GetUpperBound(1) + 1; j++)
                    {
                        commandText = String.Concat(commandText, $",'{Data.value[i,j]}'"); //добавляем остальные элементы строки
                    }
                    commandText = String.Concat(commandText, ");"); //закрываем скобки
                }

                commandText = String.Concat(commandText, $"Exec {Data.sqlStoredProcedure} @table");//команда для записи данных в БД через хранимую процедуру

                //подключаемся к БД  
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();//открываем соединение
                    SqlCommand command = new SqlCommand(commandText, connection); //создаем скрипт
                    int number = command.ExecuteNonQuery(); //выполняем скрипт
                }

                result = $"WriteToDatabase: [{Data.name}] успешно записан в БД";
                logger.Info(result);
                resultColor = Brushes.Green;
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
            string sql = $"SELECT * FROM {TableName}"; //выбираем данные из таблицы
            
            string connectionString = $@"Data Source=.\{SqlServer};Initial Catalog={Database};Integrated Security=True"; //строка подключения
            SqlConnection connection = new SqlConnection(connectionString); //подключаемся к БД
            SqlCommand command = new SqlCommand(sql, connection);//команда SELECT 
            SqlDataAdapter adapter = new SqlDataAdapter(command); //создадим обьект SqlDataAdapter для заполнения DataTable
            DataTable Table = new DataTable(); //создаём таблицу

            connection.Open(); //открываем подключение
            adapter.Fill(Table); //заполняем таблицу
            return Table.DefaultView; //представление таблицы, которое можно записать в DataGrid
        }
        #endregion
    }
}
