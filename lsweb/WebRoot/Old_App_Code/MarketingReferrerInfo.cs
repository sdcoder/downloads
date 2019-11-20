using System;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

[Serializable]
public class MarketingReferrerInfo
{
    private const string REFERRER_NAME_KEY = "ReferrerName";
    private const string FACT_KEY = "FACT";
    private NameValueCollection _metaData = new NameValueCollection();

    public MarketingReferrerInfo()
    {
        ReferrerName = string.Empty;
        FACT = 0;
    }

    [MaxLength(255)]
    [ReadOnly(true)]
    public string ReferrerName{ get; set; }

    [ReadOnly(true)]
    public int FACT { get; set; }

    public void SetDataNameValue(string name, string value)
    {
        if (_metaData.GetValues(name) != null && _metaData.GetValues(name).Length == 1)
            _metaData.Remove(name);
        _metaData.Set(name, value);
    }

    public string GetDataValue(string name)
    {
        var val = _metaData.GetValues(name);
        return val == null ? string.Empty : val.First();
    }

    public string SerializeToString()
    {
        StringBuilder sb = new StringBuilder();
        _addNameValue(sb,REFERRER_NAME_KEY,this.ReferrerName);
        _addNameValue(sb,FACT_KEY, this.FACT.ToString());
        foreach (string key in _metaData.AllKeys)
        {
            if( _metaData[key] != null )
                _addNameValue(sb, key, _metaData[key]);
        }

        return sb.ToString();
    }

    public static MarketingReferrerInfo FromSerializedString(string s)
    {
        MarketingReferrerInfo mri = null;

        if( ! String.IsNullOrEmpty(s) )
        {
            string[] data = s.Split(',');
            if (data.Length >= 2)
            {
                mri = new MarketingReferrerInfo();

                for (int i = 0; i < data.Length; i++)
                {
                    if (data[i] == REFERRER_NAME_KEY)
                        mri.ReferrerName = data[++i];
                    else
                    if (data[i] == FACT_KEY)
                        mri.FACT = int.Parse(data[++i]);
                    else
                        mri.SetDataNameValue(data[i], data[++i]);
                }
            }
        }

        return mri;
    }

    private void _addNameValue(StringBuilder sb, string name, string value)
    {
        if (sb.Length > 0)
            sb.Append(",");
        sb.AppendFormat("{0},{1}", name, value);
    }
}