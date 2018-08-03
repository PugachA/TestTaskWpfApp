using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using NLog;

namespace TestTaskWpfApp
{
    class DataManager
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        #region Certificate
        public X509Certificate2 GetCertificate(string serialNumber)
        {
            X509Certificate2 certificate = null;

            // Открываем хранилище сертификатов
            using (X509Store store = new X509Store("My", StoreLocation.LocalMachine))
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
            logger.Info($"Сертификат c номером {serialNumber} успешно найден");
   
            return certificate;
        }

        public string GetCertificateInfo(X509Certificate2 certificate)
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

            return Info;
            //logger.Info(Info);
        }
        #endregion

        public XmlDocument RequestToServer(X509Certificate2 certificate, string requestUri)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(requestUri);
                request.ClientCertificates.Add(certificate);
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                using (Stream stream = response.GetResponseStream())
                {
                    using (XmlTextReader reader = new XmlTextReader(stream))
                    {
                        XmlDocument xmlDoc = new XmlDocument();
                        xmlDoc.Load(reader);
                        return xmlDoc;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                return null;
            }
        }
        public DataXmlSql ParseXml(XmlDocument xmlDoc, string formatXml)
        {
            try
            {
                // получим корневой элемент
                XmlElement xRoot = xmlDoc.DocumentElement;

                //int n = xRoot.Attributes.Count; //получаем количество узлов
                DataXmlSql Data = new DataXmlSql();

                if (formatXml == "ErrorCodes")
                {
                    Data.ErrorCodesValues();
                }
                else if (formatXml == "Categories")
                {
                    Data.CategoriesValues();
                }
                else throw new Exception("Передан неверный формата разбора XML");

                int i = 0;
                int n = xRoot.ChildNodes.Count; //количество узлов в корневом элементе
                int m = Data.xmlAttributes.Length;
                Data.value = new string[n, m];
                // обход всех узлов в корневом элементе
                foreach (XmlNode xnode in xRoot)
                {
                    if (xnode.Attributes.Count > 0)
                    {
                        for (int j = 0; j < m; j++)
                        {
                            XmlNode attribute = xnode.Attributes.GetNamedItem(Data.xmlAttributes[j]);
                            // получаем атрибут
                            if (attribute != null)
                                Data.value[i, j] = attribute.Value;
                        }
                    }
                    i++;
                }
                return Data;
            }
            catch(Exception ex)
            {
                logger.Error(ex.Message);
                return null;
                
            }
        }
        public void WriteToDatabase(string SqlServer, string Database, DataXmlSql Data)
        {
            try
            {
                string connectionString = $@"Data Source=.\{SqlServer};Initial Catalog={Database};Integrated Security=True"; //подключаемся к базе

                string commandText = $"DECLARE @table AS {Data.TableType};";

                for (int i = 0; i < Data.value.GetUpperBound(0) + 1; i++)
                {
                    commandText = String.Concat(commandText, $"INSERT INTO @table ({Data.TableTypeValues}) Values  ({Data.value[i,0]}");
                    for (int j = 1; j < Data.value.GetUpperBound(1) + 1; j++)
                    {
                        commandText = String.Concat(commandText, $",'{Data.value[i,j]}'");
                    }
                    commandText = String.Concat(commandText, ");");
                }
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    commandText = String.Concat(commandText,$"Exec {Data.sqlStoredProcedure} @table");
                    SqlCommand command = new SqlCommand(commandText, connection);
                    int number = command.ExecuteNonQuery();
                }
                
                logger.Info($"{Data.name} успешно записан в БД");
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
            }
        }
    }
}
