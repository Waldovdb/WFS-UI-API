using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WFS_UI_API
{
    public class Packet
    {
        public List<KVPair> kVPairs { get; set; }
        public List<NestedCollection> kVColls { get; set; }

        public Packet()
        {
            kVPairs = new List<KVPair>();
            kVColls = new List<NestedCollection>();
        }
    }

    public class LookupSourceModel
    {
        public string RepeatingControl { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }
    }

    public class KVPair
    {
        public string Key { get; set; }
        public string Value { get; set; }

        public KVPair()
        {

        }

        public KVPair(string inKey, string inValue)
        {
            this.Key = inKey;
            this.Value = inValue;
        }
    }

    public class NestedCollection
    {
        public string RepeatingControl { get; set; }
        public List<KVPairListNested> KVNested { get; set; }
        public NestedCollection()
        {
            KVNested = new();
        }
    }

    public class BankCollection
    {
        public KVCollDDLResult ddlPaymentBankName { get; set; }
        public KVCollDDLResult ddlPaymentBankAccountType { get; set; }
        public string txtPaymentBankBranch { get; set; }
        public string txtPaymentBankCode { get; set; }
        public string txtPaymentBankAccountNumber { get; set; }
    }

    public class KVPairListNested
    {
        public List<KVPair> Values { get; set; }
        public KVColl NestedColl { get; set; }
        public KVPairListNested()
        {
            NestedColl = new();
        }
    }

    public class KVCollDDLResult
    {
        public string RepeatingControl { get; set; }
        public KVPair Values { get; set; }
    }

    public class KVCollDDL
    {
        public string RepeatingControl { get; set; }
        public List<KVPair> Values { get; set; }
        public KVCollDDL()
        {
            Values = new List<KVPair>();
        }
    }

    public class KVColl
    {
        public string RepeatingControl { get; set; }
        public List<List<KVPair>> Values { get; set; }

        public KVColl()
        {
            Values = new List<List<KVPair>>();
        }

        public void AddKVPairList(List<KVPair> inList)
        {
            this.Values.Add(inList);
        }
    }
}
