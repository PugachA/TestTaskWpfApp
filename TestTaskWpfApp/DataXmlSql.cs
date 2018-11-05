using System;
using System.Data;

namespace TestTaskWpfApp
{
    class DataXmlSql
    {
        /// <summary>
        /// Класс для хранения разобранного XML ответа сервера, 
        /// настройки работы с данными XML и записи их в БД,
        /// содержит вспомогательгые переменные
        /// </summary>
        
        public string[] xmlAttributes; //атрибуты XML узла
        public string sqlStoredProcedure; //название хранимой процедуры для  записи в БД
        public string name; //название списка
        public string TableName; //название таблицы в БД
        public DataTable table = new DataTable();

        public void ErrorCodesValues() //заполняет все поля для ErrorCodes
        {
            name = "Список ошибок";
            xmlAttributes = new string[2];
            xmlAttributes[0] = "code";
            xmlAttributes[1] = "text";

            sqlStoredProcedure = "sp_AddErrorCodes";
            TableName = "ErrorCode";

            for (int i=0;i<xmlAttributes.Length;i++)
            {
                DataColumn column = new DataColumn(xmlAttributes[i]);
                table.Columns.Add(column);
            }
        }

        public void CategoriesValues() //заполняет все поля для Categories
        {
            name = "Список категорий";
            xmlAttributes = new string[4];
            xmlAttributes[0] = "id";
            xmlAttributes[1] = "name";
            xmlAttributes[2] = "parent";
            xmlAttributes[3] = "image";

            sqlStoredProcedure = "sp_AddCategories";
            TableName = "Category";

            for (int i = 0; i < xmlAttributes.Length; i++)
            {
                DataColumn column = new DataColumn(xmlAttributes[i]);
                table.Columns.Add(column);
            }
        }
    }
}
