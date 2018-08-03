using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestTaskWpfApp
{
    class DataXmlSql
    {
        public string[] xmlAttributes;
        public string[,] value;
        public string sqlStoredProcedure;
        public string[] parametersStoredProcedure;
        public string TableType;
        public string name;
        public string TableTypeValues;

        public void ErrorCodesValues()
        {
            name = "Список ошибок";
            xmlAttributes = new string[2];
            xmlAttributes[0] = "code";
            xmlAttributes[1] = "text";

            sqlStoredProcedure = "sp_AddErrorCodes";
            TableType = "ErrorCodeTableType";
            TableTypeValues = "code, [text]";
            parametersStoredProcedure = new string[2];
            parametersStoredProcedure[0] = "@code";
            parametersStoredProcedure[1] = "@text";
        }

        public void CategoriesValues()
        {
            name = "Список категорий";
            xmlAttributes = new string[4];
            xmlAttributes[0] = "id";
            xmlAttributes[1] = "name";
            xmlAttributes[2] = "parent";
            xmlAttributes[3] = "image";

            sqlStoredProcedure = "sp_AddCategories";
            TableType = "CategoryTableType";
            TableTypeValues = "id, [name], parent, [image]";
            parametersStoredProcedure = new string[4];
            parametersStoredProcedure[0] = "@id";
            parametersStoredProcedure[1] = "@name";
            parametersStoredProcedure[2] = "@parent";
            parametersStoredProcedure[3] = "@image";
        }

        public string GetInfo()
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
