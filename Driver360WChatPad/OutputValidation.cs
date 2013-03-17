using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using System.Windows;
using System.Threading;
using System.Windows.Threading;
using System.Windows.Forms;
using System.Xml;
using System.Runtime.InteropServices;
using vJoyInterfaceWrap;
namespace Driver360WChatPad
{
    public static class OutputValidation
    {
        public static bool shiftModifier = false;
        public static bool orangeModifier = false;
        public static bool capsLockModifier = false;
        public static bool greenModifier = false;
        public static bool peopleModifier = false; //Not currently used other than for on/off toggle
        public static Queue<string> previousDPad = new Queue<string>();
        [Flags]
        enum Bits
        {
            LB = 1,
            RB = 2,
            GUIDE = 4,
            NA = 8,
            A = 16,
            B = 32,
            X = 64,
            Y = 128
        }
        [Flags]
        enum Bits2
        {
            UP = 1,
            DOWN = 2,
            LEFT = 4,
            RIGHT = 8,
            START = 16,
            BACK = 32,
            LT = 64,
            RT = 128
        }
        public static void OutputMappingForChatPad(byte[] response,ChatpadController cController, uint controllerIndex)
        {
            if (response[1] == 2 && response[3] == 240)
            {
                if (response[26] != 0)
                {
                    string key = cController.GetChatPadKeyValue(response[26].ToString(), orangeModifier, greenModifier);
                    if (key.Length != 0)
                    {
                        if (!SpecialKeyAssignment(key))
                        {
                            if (!capsLockModifier && !shiftModifier)
                            {
                                SendKeys.SendWait(key);
                            }
                            else
                            {
                                SendKeys.SendWait(key.ToUpper());
                            }
                        }
                    }

                    if (response[26] != response[27] && response[27]!=0)
                    {
                        if (key.Length != 0)
                        {
                            key = cController.GetChatPadKeyValue(response[27].ToString(), orangeModifier, greenModifier);
                            if (!SpecialKeyAssignment(key))
                            {
                                if (!capsLockModifier && !shiftModifier)
                                {
                                    SendKeys.SendWait(key);
                                }
                                else
                                {
                                    SendKeys.SendWait(key.ToUpper());
                                }
                            }
                        }
                    }
                }
            }
            shiftModifier = false;
        }
        public static void OutputMappingForJoyStick(byte[] response, JoystickController jController, uint controllerIndex)
        {
            if (response[1] == 1 && response[3] == 240 && response[5] == 19)
            {
                if (response[8] == 0)
                {
                    jController.AxisSet(controllerIndex, 0, 8);
                }
                else if (response[9] == 0)
                {
                    jController.AxisSet(controllerIndex, 0, 9);
                }
                else if (response[10] == 0 && response[12] == 0)
                {
                    //jController.AxisSet(controllerIndex, 0, 10);
                    //jController.AxisSet(controllerIndex, 255, 12);
                }
                else if (response[14] == 0 && response[16] == 0)
                {
                    //jController.AxisSet(controllerIndex, 0, 14);
                    //jController.AxisSet(controllerIndex, 255, 16);
                }
                //0 1 0 240 0 19 0 XX 0 0 32 12 92 0 44 6 163 247
                //a=16, b=32, x=64, y=128
                if (response[7] > 0)
                {
                    var b = (Bits)response[7];

                    string[] sArray = b.ToString().Replace(" ", string.Empty).Split(',');
                    foreach (string s in sArray)
                    {
                        jController.ButtonToggle(controllerIndex, jController.GetJoystickKeyValue(s, 1), true);
                    }
                }
                else if (response[6] > 0)
                {
                    var b = (Bits2)response[6];

                    string[] sArray = b.ToString().Replace(" ", string.Empty).Split(',');
                    foreach (string s in sArray)
                    {
                        if (s.Contains("UP") || s.Contains("DOWN") || s.Contains("LEFT") || s.Contains("RIGHT"))
                        {
                            previousDPad.Enqueue(s);
                            if (previousDPad.Count > 2)
                            {
                                //Store a queue of dpad events as you can only press two directions on the dpad at the same time
                                //But an evant to release the dpad isn't send until all directions are released
                                string dequeudDPad = previousDPad.Dequeue();
                                jController.ButtonToggle(controllerIndex, jController.GetJoystickKeyValue(dequeudDPad, 2), false);
                            }
                        }
                        jController.ButtonToggle(controllerIndex, jController.GetJoystickKeyValue(s, 2), true);
                    }
                }
                else if (response[8] > 0)
                {
                    //Left Trigger
                    int v = response[8];
                    if (v < 0)
                    {
                        v = 0;
                    }
                    jController.AxisSet(controllerIndex, -response[8]/2, 8);
                }
                else if (response[9] > 0)
                {
                    int v = response[9];
                    if (v > 0)
                    {
                        v = 0;
                    }
                    jController.AxisSet(controllerIndex, response[9]/2, 9);
                }
                else if (response[11] > 0 || response[13] > 0)
                {
                    int x = response[11];
                    if (x >= 128) //What why? Why does MS provide 4 points to position in 2d space? Why do the values jump at a certain point? Why... Why...
                    {
                        x = x - 255;
                    }
                    jController.AxisSet(controllerIndex, x, 11);
                    int y = response[13];
                    if (y >= 128) //What why? Why does MS provide 4 points to position in 2d space? Why do the values jump at a certain point? Why... Why...
                    {
                        y = y - 255; //Why is the y stick inverted?
                    }
                    jController.AxisSet(controllerIndex, -y, 13);
                }
                else if (response[15] > 0 || response[17] > 0)
                {
                    int x = response[15];
                    if (x >= 128) //What why? Why does MS provide 4 points to position in 2d space? Why do the values jump at a certain point? Why... Why...
                    {
                        x = x - 255;
                    }
                    jController.AxisSet(controllerIndex, x, 15);
                    int y = response[17];
                    if (y >= 128) //What why? Why does MS provide 4 points to position in 2d space? Why do the values jump at a certain point? Why... Why...
                    {
                        y = y - 255; //Why is the y stick inverted?
                    }
                    jController.AxisSet(controllerIndex, -y, 17);
                }
                else if (response[7] == 0 || response[4] == 0)
                {
                    var b = (Bits)255;
                    string[] sArray = b.ToString().Replace(" ", string.Empty).Split(',');
                    foreach (string s in sArray)
                    {
                        if (s != "NA")
                        {
                            jController.ButtonToggle(controllerIndex, jController.GetJoystickKeyValue(s, 1), false);
                        }
                    }
                    var b2 = (Bits2)255;
                    sArray = b2.ToString().Replace(" ", string.Empty).Split(',');
                    foreach (string s in sArray)
                    {
                        if (s != "NA")
                        {
                            jController.ButtonToggle(controllerIndex, jController.GetJoystickKeyValue(s, 2), false);
                        }
                    }
                    previousDPad.Clear();
                }
            }
        }
        public static bool SpecialKeyAssignment(string key)
        {
            bool specialKeyAssigned = false;
            switch (key)
            {
                case "orange":
                    {
                        orangeModifier = !orangeModifier;
                        if (orangeModifier)
                        {
                            greenModifier = false;
                            capsLockModifier = false;
                        }
                        specialKeyAssigned = true;
                        break;
                    }
                case "people":
                    {
                        peopleModifier = !peopleModifier;
                        specialKeyAssigned = true;
                        break;
                    }
                case "green":
                    {
                        greenModifier = !greenModifier;
                        if (greenModifier)
                        {
                            orangeModifier = false;
                            capsLockModifier = false;
                        }
                        specialKeyAssigned = true;
                        break;
                    }
                case "shift":
                    {
                        if (orangeModifier)
                        {
                            capsLockModifier = !capsLockModifier;
                            if (capsLockModifier)
                            {
                                greenModifier = false;
                                orangeModifier = false;
                            }
                        }
                        else
                        {
                            shiftModifier = true;
                        }
                        specialKeyAssigned = true;
                        break;
                    }
                case "space":
                    {
                        SendKeys.SendWait(" ");
                        specialKeyAssigned = true;
                        break;
                    }
            }
            return specialKeyAssigned;
        }
    }
}
