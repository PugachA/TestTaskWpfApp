using System;
using System.Security.Cryptography.X509Certificates;
using System.Net;

public class RequestsToServer
{
    public static X509Certificate2 GetCertificate(string serialNumber)
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
            certificate = Results[0];
            _textBlock.Text("Успешно");
        }

        catch (Exception e)
        {
            _textBlock.Text("Ошибка: " + e.Message);
        }

        finally
        {
            store.Close();
        }

    }
}
