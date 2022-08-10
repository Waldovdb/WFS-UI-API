using System;
using System.Collections.Generic;
using Newtonsoft;
using System.Threading.Tasks;

namespace WFS_UI_API.Data
{
    public class DataCopy
    {
        public string DestinationTable { get; set; }
        public string SourceTable { get; set; }
        public string InputDatabase { get; set; }
        public string OutputDatabase { get; set; }
        public bool MapUnmapped { get; set; }
        public string ColumnMapping { get; set; }

        public bool VerifyInputs(string Input1, string Input2)
        {
            return (!String.IsNullOrEmpty(Input1) & !String.IsNullOrEmpty(Input2));
        }

        public List<ColumnMap> GetColumnMaps()
        {
            try
            {
                var temp = Newtonsoft.Json.JsonConvert.DeserializeObject<List<ColumnMap>>(ColumnMapping);
                return temp;
            }
            catch(Exception)
            {
                this.MapUnmapped = true;
                return new List<ColumnMap>();
            }
        }
    }

    public class ColumnMap
    {
        public string source { get; set; }
        public string target { get; set; }
    }

    public class DataSchema
    {
        public string COLUMN_NAME { get; set; }
        public int ORDINAL_POSITION { get; set; }
        public string DATA_TYPE { get; set; }
    }
}
