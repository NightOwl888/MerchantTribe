
using System.Xml;
using System;

namespace MerchantTribe.Shipping.FedEx
{

    public class DimensionsBlock
    {

        private decimal _Length = 0m;
        private decimal _Width = 0m;
        private decimal _Height = 0m;
        private DimensionType _Units = DimensionType.IN;

        public decimal Length
        {
            get { return _Length; }
            set { _Length = Math.Round(value, 1, MidpointRounding.AwayFromZero); }
        }
        public decimal Width
        {
            get { return _Width; }
            set { _Width = Math.Round(value, 1, MidpointRounding.AwayFromZero); }
        }
        public decimal Height
        {
            get { return _Height; }
            set { _Height = Math.Round(value, 1, MidpointRounding.AwayFromZero); }
        }
        public DimensionType Units
        {
            get { return _Units; }
            set { _Units = value; }
        }

        public void WriteToXml(XmlTextWriter xw, string elementName)
        {
            if ((_Length > 0m) | (_Width > 0m) | (_Height > 0m))
            {
                xw.WriteStartElement(elementName);
                WriteToXml(xw);
                xw.WriteEndElement();
            }
        }

        public void WriteToXml(XmlTextWriter xw)
        {
            XmlHelper.WriteIfNotEmpty(xw, "Length", Math.Round(_Length, 0, MidpointRounding.AwayFromZero).ToString());
            XmlHelper.WriteIfNotEmpty(xw, "Width", Math.Round(_Width, 0, MidpointRounding.AwayFromZero).ToString());
            XmlHelper.WriteIfNotEmpty(xw, "Height", Math.Round(_Height, 0, MidpointRounding.AwayFromZero).ToString());
            XmlHelper.WriteIfNotEmpty(xw, "Units", _Units.ToString());
        }

    }

 
}