using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using vJoyInterfaceWrap;
using System.Data;

namespace Driver360WChatPad
{   
    public class JoystickController
    {
        public List<vJoy> joysticks;
        private int maxJoysticks = 1;
        public DataTable dt = new DataTable("JoystickButton");
        private DataView dv;
        public Dictionary<int, HID_USAGES> usages = new Dictionary<int, HID_USAGES>();
        public JoystickController()
        {
            try
            {
                joysticks = new List<vJoy>();
                for (int i = 0; i < maxJoysticks; i++)
                {
                    joysticks.Add(new vJoy());
                    joysticks[i].AcquireVJD((uint)i + 1);
                    ValidateJoystick(i);
                }
            }
            catch (Exception e)
            {
                ErrorLogging.WriteLogEntry(String.Format("General error during vJoy initializing of JoystickController class: {0}",e.InnerException), ErrorLogging.LogLevel.Fatal);
            }
            try
            {
                dt.Columns.Add(new DataColumn("ID", typeof(string)));
                dt.Columns.Add(new DataColumn("Map", typeof(int)));
                dt.Columns.Add(new DataColumn("ButtonGroup", typeof(int)));
                dt.Columns.Add(new DataColumn("Description", typeof(string)));
                dt.ReadXml("JoystickMappings.xml");
                dv = new DataView(dt);
            }
            catch (Exception e)
            {
                ErrorLogging.WriteLogEntry(String.Format("General error during XML Mapping initializing JoystickController class: {0}", e.InnerException), ErrorLogging.LogLevel.Fatal);
            }

            //TODO: Does one Z trigger win over the other, or are the additive?
            //TODO: XBox reads right and left as positive and negative Z, I could change this as I get different 0 to 255 values for each trigger.
            usages.Add(8, HID_USAGES.HID_USAGE_Z);//Left Trigger
            usages.Add(9, HID_USAGES.HID_USAGE_Z);//Right Trigger
            usages.Add(11, HID_USAGES.HID_USAGE_X);
            usages.Add(13, HID_USAGES.HID_USAGE_Y);
            usages.Add(15, HID_USAGES.HID_USAGE_RX);
            usages.Add(17, HID_USAGES.HID_USAGE_RY);
        }
        private void ValidateJoystick(int joystickID)
        {
            if (!joysticks[joystickID].vJoyEnabled())
            {
                ErrorLogging.WriteLogEntry("vJoy Not Enabled", ErrorLogging.LogLevel.Fatal);
            }
            else
            {
                ErrorLogging.WriteLogEntry(String.Format("Vendor: {0} Product :{1} Version Number:{2} ", joysticks[joystickID].GetvJoyManufacturerString(), joysticks[joystickID].GetvJoyProductString(), joysticks[joystickID].GetvJoySerialNumberString()), ErrorLogging.LogLevel.Information);
            }
            VjdStat status = joysticks[joystickID].GetVJDStatus((uint)joystickID);
            switch (status)
            {
                case VjdStat.VJD_STAT_OWN:
                    ErrorLogging.WriteLogEntry(String.Format("vJoy Device {0} is already owned by this feeder", joystickID),ErrorLogging.LogLevel.Information);
                    break;
                case VjdStat.VJD_STAT_FREE:
                    ErrorLogging.WriteLogEntry(String.Format("vJoy Device {0} is free", joystickID), ErrorLogging.LogLevel.Warning);
                    break;
                case VjdStat.VJD_STAT_BUSY:
                    ErrorLogging.WriteLogEntry(String.Format("vJoy Device {0} is owned by another feeder", joystickID), ErrorLogging.LogLevel.Error);
                    return;
                case VjdStat.VJD_STAT_MISS:
                    ErrorLogging.WriteLogEntry(String.Format("vJoy Device {0} is not installed", joystickID), ErrorLogging.LogLevel.Fatal);
                    return;
                default:
                    ErrorLogging.WriteLogEntry(String.Format("vJoy Device {0} unknown error", joystickID), ErrorLogging.LogLevel.Error);
                    return;
            };
            if ((status == VjdStat.VJD_STAT_OWN) || ((status == VjdStat.VJD_STAT_FREE) && (!joysticks[joystickID].AcquireVJD((uint)joystickID))))
            {
                ErrorLogging.WriteLogEntry(String.Format("Failed to acquire vJoy device {0}", joystickID), ErrorLogging.LogLevel.Error);
            }
            else
            {
                ErrorLogging.WriteLogEntry(String.Format("Acquired vJoy device {0}", joystickID), ErrorLogging.LogLevel.Information);
            }
            joysticks[joystickID].ResetVJD((uint)joystickID);
        }
        public uint GetJoystickKeyValue(string value, int buttonGroup) 
        {
            try
            {
                dv.RowFilter = String.Format("(ID='{0}' AND ButtonGroup={1})",value,buttonGroup);
                return (uint)(int)dv[0]["Map"];
            }
            catch (IndexOutOfRangeException iorex)
            {
                //Value from chat pad not recognized
                ErrorLogging.WriteLogEntry("Joystick data not recognized {0}", ErrorLogging.LogLevel.Error);
                throw new Exception("Joystick data not recognized", iorex);
            }
        }
        public void ButtonToggle(uint joystickIndex, uint value, bool isPressed)
        {
            joysticks[(int)joystickIndex - 1].SetBtn(isPressed, joystickIndex, value);
        }
        public void AxisSet(uint joystickIndex, int value, int usageIndex)
        {
            joysticks[(int)joystickIndex - 1].SetAxis(value, joystickIndex, usages[usageIndex]);
        }
    }
}
