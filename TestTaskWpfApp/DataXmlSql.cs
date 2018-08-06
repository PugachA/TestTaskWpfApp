using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public string[,] value; //двумерный массив для хранения данных XML ответа
        public string sqlStoredProcedure; //название хранимой процедуры для  записи в БД
        public string TableType; //название табличного типа
        public string name; //название списка
        public string TableName; //название таблицы в БД
        public string TableTypeValues;//название столбцов в табличном типе

        public void ErrorCodesValues() //заполняет все поля для ErrorCodes
        {
            name = "Список ошибок";
            xmlAttributes = new string[2];
            xmlAttributes[0] = "code";
            xmlAttributes[1] = "text";

            sqlStoredProcedure = "sp_AddErrorCodes";
            TableType = "ErrorCodeTableType";
            TableName = "ErrorCode";
            TableTypeValues = "code, [text]";
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
            TableType = "CategoryTableType";
            TableName = "Category";
            TableTypeValues = "id, [name], parent, [image]";
        }

        public string GetInfo() //информация о содержимом в переменной value
        {
            string str = "";

            for (int i = 0; i < value.GetUpperBound(0) + 1; i++)
            {
                for (int j = 0; j < value.GetUpperBound(1) + 1; j++)
                {
                    str = String.Concat(str, $"{value[i, j]}\t");
                }
                str = String.Concat(str, "\n");
            }
            return str;
        }
    }
}
