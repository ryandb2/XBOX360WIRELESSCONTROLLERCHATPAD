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
        private int maxJoysticks = 4;
        public DataTable dt = new DataTable("JoystickButton");
        private DataView dv;
        public Dictionary<int, HID_USAGES> usages = new Dictionary<int, HID_USAGES>();
        public JoystickController()
        {
            joysticks = new List<vJoy>();
            for (uint i = 0; i < maxJoysticks; i++)
            {
                joysticks.Add(new vJoy());
                joysticks[(int)i].AcquireVJD(i+1);
            }
            dt.Columns.Add(new DataColumn("ID", typeof(string)));
            dt.Columns.Add(new DataColumn("Map", typeof(int)));
            dt.Columns.Add(new DataColumn("ButtonGroup", typeof(int)));
            dt.Columns.Add(new DataColumn("Description", typeof(string)));
            dt.ReadXml("JoystickMappings.xml");
            dv = new DataView(dt);

            //TODO: Does one Z trigger win over the other, or are the additive?
            //TODO: XBox reads right and left as positive and negative Z, I could change this as I get different 0 to 255 values for each trigger.
            usages.Add(8, HID_USAGES.HID_USAGE_Z);//Left Trigger
            usages.Add(9, HID_USAGES.HID_USAGE_Z);//Right Trigger
            usages.Add(11, HID_USAGES.HID_USAGE_X);
            usages.Add(13, HID_USAGES.HID_USAGE_Y);
            usages.Add(15, HID_USAGES.HID_USAGE_RX);
            usages.Add(17, HID_USAGES.HID_USAGE_RY);
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
                throw new Exception("Joystick Data Not Recognized", iorex);
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
        /*
        public void ButtonPressed_A(uint joystickIndex, uint value)
        {
            joysticks[(int)joystickIndex - 1].SetBtn(true, joystickIndex, value);
            joysticks[(int)joystickIndex - 1].SetBtn(false, joystickIndex, value);
        }
        public void ButtonPressed_B(uint joystickIndex, uint value)
        {
            joysticks[(int)joystickIndex - 1].SetBtn(true, joystickIndex, value);
            joysticks[(int)joystickIndex - 1].SetBtn(false, joystickIndex, value);
        }
        public void ButtonPressed_X(uint joystickIndex, uint value)
        {
            joysticks[(int)joystickIndex - 1].SetBtn(true, joystickIndex, value);
            joysticks[(int)joystickIndex - 1].SetBtn(false, joystickIndex, value);
        }
        public void ButtonPressed_Y(uint joystickIndex, uint value)
        {
            joysticks[(int)joystickIndex - 1].SetBtn(true, joystickIndex, value);
            joysticks[(int)joystickIndex - 1].SetBtn(false, joystickIndex, value);
        }
        public void ButtonPressed_Guide(uint joystickIndex, uint value)
        {
            joysticks[(int)joystickIndex - 1].SetBtn(true, joystickIndex, value);
            joysticks[(int)joystickIndex - 1].SetBtn(false, joystickIndex, value);
        }
        public void ButtonPressed_RightBumper(uint joystickIndex, uint value)
        {
            joysticks[(int)joystickIndex - 1].SetBtn(true, joystickIndex, value);
            joysticks[(int)joystickIndex - 1].SetBtn(false, joystickIndex, value);
        }
        public void ButtonPressed_LeftBumper(uint joystickIndex, uint value)
        {
            joysticks[(int)joystickIndex - 1].SetBtn(true, joystickIndex, value);
            joysticks[(int)joystickIndex - 1].SetBtn(false, joystickIndex, value);
        }
        public void ButtonPressed_Start(uint joystickIndex, uint value)
        {
            joysticks[(int)joystickIndex - 1].SetBtn(true, joystickIndex, value);
            joysticks[(int)joystickIndex - 1].SetBtn(false, joystickIndex, value);
        }
        public void ButtonPressed_Back(uint joystickIndex, uint value)
        {
            joysticks[(int)joystickIndex - 1].SetBtn(true, joystickIndex, value);
            joysticks[(int)joystickIndex - 1].SetBtn(false, joystickIndex, value);
        }
        public void ButtonPressed_RightThumbstick(uint joystickIndex, uint value)
        {
            joysticks[(int)joystickIndex - 1].SetBtn(true, joystickIndex, value);
            joysticks[(int)joystickIndex - 1].SetBtn(false, joystickIndex, value);
        }
        public void ButtonPressed_LeftThumbstick(uint joystickIndex, uint value)
        {
            joysticks[(int)joystickIndex - 1].SetBtn(true, joystickIndex, value);
            joysticks[(int)joystickIndex - 1].SetBtn(false, joystickIndex, value);
        }
        public void Thumbstick_Left(uint joystickIndex,int x, int y)
        {
            joysticks[(int)joystickIndex - 1].SetAxis(x, joystickIndex, HID_USAGES.HID_USAGE_X);
            joysticks[(int)joystickIndex - 1].SetAxis(y, joystickIndex, HID_USAGES.HID_USAGE_Y);
        }
        public void Thumbstick_Right(uint joystickIndex,int x, int y)
        {
            joysticks[(int)joystickIndex - 1].SetAxis(x, joystickIndex, HID_USAGES.HID_USAGE_RX);
            joysticks[(int)joystickIndex - 1].SetAxis(y, joystickIndex, HID_USAGES.HID_USAGE_RY);
        }
        public void Trigger_Left(uint joystickIndex, int value)
        {
            joysticks[(int)joystickIndex - 1].SetAxis(value, joystickIndex, HID_USAGES.HID_USAGE_Z);
        }
        public void Trigger_Right(uint joystickIndex, int value)
        {
            joysticks[(int)joystickIndex - 1].SetAxis(value, joystickIndex, HID_USAGES.HID_USAGE_RZ);
        }
        public void Pad_Right(uint joystickIndex, uint value)
        {
            joysticks[(int)joystickIndex - 1].SetBtn(true, joystickIndex, value);
            joysticks[(int)joystickIndex - 1].SetBtn(false, joystickIndex, value);
        }
        public void Pad_Left(uint joystickIndex, uint value)
        {
            joysticks[(int)joystickIndex - 1].SetBtn(true, joystickIndex, value);
            joysticks[(int)joystickIndex - 1].SetBtn(false, joystickIndex, value);
        }
        public void Pad_Up(uint joystickIndex, uint value)
        {
            joysticks[(int)joystickIndex - 1].SetBtn(true, joystickIndex, value);
            joysticks[(int)joystickIndex - 1].SetBtn(false, joystickIndex, value);
        }
        public void Pad_Down(uint joystickIndex, uint value)
        {
            joysticks[(int)joystickIndex - 1].SetBtn(true, joystickIndex, value);
            joysticks[(int)joystickIndex - 1].SetBtn(false, joystickIndex, value);
        }*/
    }
}
