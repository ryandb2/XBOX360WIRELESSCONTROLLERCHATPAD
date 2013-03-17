using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Data;

namespace Driver360WChatPad
{
    public class ChatpadController
    {
        public DataTable dt = new DataTable("ChatPadKey");
        private DataView dv;
        public ChatpadController()
        {
            dt.Columns.Add(new DataColumn("ID", typeof(int)));
            dt.Columns.Add(new DataColumn("Map", typeof(string)));
            dt.Columns.Add(new DataColumn("OrangeModifer", typeof(string)));
            dt.Columns.Add(new DataColumn("GreenModifer", typeof(string)));
            dt.Columns.Add(new DataColumn("ShiftModifer", typeof(string)));
            dt.ReadXml("ChatPadMappings.xml");
            dv = new DataView(dt);
        }
        public string GetChatPadKeyValue(string value,bool orangeModifer, bool greenModifer) 
        {
            try
            {
                dv.RowFilter = String.Format("(ID={0})", Convert.ToInt16(value));
                return dv[0]["Map"].ToString();
            }
            catch (IndexOutOfRangeException iorex)
            {
                //Value from chat pad not recognized, should not be possible, yeah right
                throw new Exception("Chatpad Data Not Recognized", iorex);
            }
        }
    }
}
