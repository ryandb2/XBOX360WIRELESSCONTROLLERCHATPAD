using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using LibUsbDotNet;
using LibUsbDotNet.Main;
using System.Windows.Threading;
using System.Threading;
using System.Data;
using System.Runtime.Serialization.Formatters.Binary;
using System.Diagnostics;
using Microsoft.Win32;
using System.Windows.Forms;
using System.Drawing;
namespace Driver360WChatPad
{
    public partial class MainWindow : Window
    {
        //vendor id - 0x045e  -- 1118 int
        //product id - 0x0719 -- 1817 int
        private static UsbDeviceFinder MyUsbFinder;
        private static UsbDevice MyUsbDevice;
        private static DispatcherTimer timer = new DispatcherTimer();
        private static int timerToggle = 0;
        private static UsbEndpointWriter writer;
        private static UsbEndpointReader reader;
        private static bool enableKeepAlive = true;
        private static DateTime lastEvent = DateTime.Now;
        private static DateTime lastProcessedResponse = DateTime.Now;
        public static ChatpadController cController;
        //private static byte[] previousResponse; //Do I still need to store previous response to prevent multiples? Seems like the delay has fixed my issues, but might not be robust
        private static bool chatPadEventsFlowing = false;
        private static bool controllerEventsFlowing = false;
        private static JoystickController jController;
        private static uint controllerIndex = 1;
        NotifyIcon ni = new NotifyIcon();
        //private static List<UsbEndpointWriter> writers; //TODO: Implement multiple controller writers
        //private static List<UsbEndpointReader> readers; //TODO: Implement multiple controller writers

        public MainWindow()
        {
            InitializeComponent();
            ValidateRegistrySettings();
            jController = new JoystickController();
            cController = new ChatpadController();
            MyUsbFinder = new UsbDeviceFinder(1118, 1817);
            timer.Tick += new EventHandler(dispatcherTimer_Tick);
            timer.Interval = new TimeSpan(0, 0, 0, 1, 0);
            MyUsbDevice = UsbDevice.OpenUsbDevice(MyUsbFinder);
            if (MyUsbDevice == null) throw new Exception("Device Not Found");
            IUsbDevice wholeUsbDevice = MyUsbDevice as IUsbDevice;

            if (!ReferenceEquals(wholeUsbDevice, null))
            {
                wholeUsbDevice.SetConfiguration(1);
                wholeUsbDevice.ClaimInterface(0);
                reader = MyUsbDevice.OpenEndpointReader(ReadEndpointID.Ep01);
                writer = MyUsbDevice.OpenEndpointWriter(WriteEndpointID.Ep01);
                reader.DataReceived += new EventHandler<EndpointDataEventArgs>(reader_DataReceived);
                reader.DataReceivedEnabled = true;
                timer.Start();
                InitializeChatpad();
                InitializeController();
            }
            else
            {
                throw new Exception("Whole USB Device is not implemented");
            }
            ni.Icon = new Icon(@"Images\controller.ico");
            ni.Visible = true;
            ni.Click +=
                delegate(object sender, EventArgs args)
                {
                    this.Show();
                    this.WindowState = WindowState.Normal;
                };
        }
        protected override void OnStateChanged(EventArgs e)
        {
            if (WindowState == WindowState.Minimized)
            {
                this.Hide();
                ni.Visible = true;
            }
            else
            {
                ni.Visible = false;
            }
            

            base.OnStateChanged(e);
        }
        private void InitializeChatpad()
        {
            SendDataToDevice(InputValidation.PeopleOff(),"");
            SendDataToDevice(InputValidation.OrangeCircleOff(), "");
            SendDataToDevice(InputValidation.GreenSquareOff(), "");
            SendDataToDevice(InputValidation.CapsLockOff(), "");
            InitializeBeginChatPadEvents();
        }
        private void InitializeController()
        {
            SendDataToDevice(InputValidation.Player(1), "");
            controllerEventsFlowing = true;
        }
        private void InitializeBeginChatPadEvents()
        {
            SendDataToDevice(InputValidation.InitializationOne(), "");
            SendDataToDevice(InputValidation.InitializationTwo(), "");
            SendDataToDevice(InputValidation.InitializationThree(), "");
            SendDataToDevice(InputValidation.InitializationFour(), "");
            SendDataToDevice(InputValidation.InitializationFive(), "");
            SendDataToDevice(InputValidation.InitializationSix(), "");
            SendDataToDevice(InputValidation.InitializationSeven(), "");
            SetSpecialKeys();
            enableKeepAlive = true;
            chatPadEventsFlowing = true;
        }
        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            if (writer != null && enableKeepAlive)
            {
                if (timerToggle == 0)
                {
                    KeepAliveInitial();
                    timerToggle++;
                }
                else
                {
                    KeepAliveAlternate();
                    timerToggle = 0;
                }
            }
            if (!chatPadEventsFlowing)
            {
                //Chat Pad Events can halt for a number of reasons, if you haven't seen any keep alives or keys in a minute re-init
                InitializeBeginChatPadEvents();
            }
            else
            {
                chatPadEventsFlowing = false;
            }
            if (!controllerEventsFlowing)
            {
                InitializeController();
            }
            else
            {
                controllerEventsFlowing = false;
            }
        }
        void reader_DataReceived(object sender, EndpointDataEventArgs e)
        {
            StringBuilder updateLog = new StringBuilder();
            //Lots of Magic Numbers Below
            //index 26 seems to have an incrementing 00-0F, but once I send an event I get a constant 128??? why? oh well ignore it
            //TODO: Guess Less
            if (e.Buffer[1] == 2 && e.Buffer[3] == 240 && e.Buffer[24] == 240 && e.Buffer[25] == 4 && (e.Buffer[26] <= 15 || e.Buffer[26]==128))
            {
                //Do nothing, these are normal keep alive responses, I think :)
                chatPadEventsFlowing = true;
            }
            else if (e.Buffer[1] == 2 && e.Buffer[3] == 240 && e.Buffer[24] == 0 && e.Buffer[25] == 0 && (e.Buffer[26] <= 15 || e.Buffer[26] == 128))
            {
                //Do nothing, these are normal keep alive responses, I think :)
                chatPadEventsFlowing = true;
            }
            else if (e.Buffer[1] == 0 && e.Buffer[3] == 240 && e.Buffer[24] == 240 && e.Buffer[25] == 3 && (e.Buffer[26] <= 15 || e.Buffer[26] == 128))
            {
                //Do nothing, these are normal keep alive responses, I think :)
                //chatPadEventsFlowing = true;
            }
            else if (e.Buffer[1] == 0 && e.Buffer[3] == 240 && e.Buffer[24] == 0 && e.Buffer[25] == 0 && (e.Buffer[26] <= 15 || e.Buffer[26] == 128))
            {
                //Do nothing, these are normal keep alive responses, I think :)
                //chatPadEventsFlowing = true;
            }
            else if (e.Buffer[1] == 2 && e.Buffer[3] == 240 && e.Buffer[24] == 0 && e.Buffer[25] == 3 && (e.Buffer[26] <= 15 || e.Buffer[26] == 128))
            {
                //Do nothing, these are normal keep alive responses, I think :)
                chatPadEventsFlowing = true;
            }
            else if (e.Buffer[1] == 0 && e.Buffer[3] == 240 && e.Buffer[24] == 0 && e.Buffer[25] == 4 && (e.Buffer[26] <= 15 || e.Buffer[26] == 128))
            {
                //Do nothing, these are normal keep alive responses, I think :)
                //chatPadEventsFlowing = true;
            }
            else if (e.Buffer[1] == 2 && e.Buffer[3] == 240 && e.Buffer[24] == 240 && e.Buffer[25] == 3 && (e.Buffer[26] <= 15 || e.Buffer[26] == 128))
            {
                //Do nothing, these are normal keep alive responses, I think :)
                //Wish I knew what I was doing instead of guessing
                chatPadEventsFlowing = false;
                //Call these false? something is wrong here
            }
            else
            {
                if (e.Buffer[1] == 1) //Joystick Data
                {
                    OutputValidation.OutputMappingForJoyStick(e.Buffer, jController, controllerIndex);
                    controllerEventsFlowing = true;
                }
                else if (e.Buffer[1] == 2)
                {
                    if (lastProcessedResponse.AddMilliseconds(120) <= DateTime.Now)// || previousResponse == null)//Make this magic number a config, lower = better response, but more likely to get repeating values
                    {
                        OutputValidation.OutputMappingForChatPad(e.Buffer, cController, controllerIndex,this);
                        lastProcessedResponse = DateTime.Now;
                        chatPadEventsFlowing = true;
                    }                    
                }
                updateLog.Append("Response - ");
                for (int i = 0; i < 48; i++)
                {
                    updateLog.Append(e.Buffer[i].ToString());
                    updateLog.Append(" ");
                }
                ThreadStart startLog = delegate()
                {
                    Dispatcher.Invoke(DispatcherPriority.Normal, new Action<string>(SetLog), updateLog.ToString());
                };
                new Thread(startLog).Start();
            }
        }
        public void SetSpecialKeys()
        {
            SendDataToDevice(InputValidation.OrangeCircleOff(), "");
            OutputValidation.orangeModifier = false;
            SendDataToDevice(InputValidation.GreenSquareOff(), "");
            OutputValidation.greenModifier = false;
            SendDataToDevice(InputValidation.CapsLockOff(), "");
            OutputValidation.capsLockModifier = false;
            SendDataToDevice(InputValidation.PeopleOff(), "");
            OutputValidation.peopleModifier = false;            
        }
        public void SetLog(string text)
        {
            if (text.Length!=0)
            {
                textBlock1.Text = text + System.Environment.NewLine + textBlock1.Text;
                //textBoxLog.Text = text + System.Environment.NewLine + textBoxLog.Text;
            }
        }        
        public void SendDataToDevice(byte[] dataToSend,string requestDescription)
        {
            if (lastEvent <= DateTime.Now.AddMilliseconds(50))
            {
                //Thread.Sleep(DateTime.Now.AddMilliseconds(50)-lastEvent);
            }
            int bytesWritten = 0;
            ErrorCode ec = writer.Write(dataToSend, 2000, out bytesWritten);
            if (ec != ErrorCode.None)
            {
                throw new Exception(UsbDevice.LastErrorString);
            }
            if (requestDescription != "" && requestDescription != String.Empty)
            {
                //SetLog(requestDescription);
            }
            lastEvent = DateTime.Now;
        }
        private void LoopArray()
        {
            byte[] bytearray = new byte[4];
            bytearray[0] = 0;
            bytearray[1] = 0;
            bytearray[2] = 0;
            bytearray[3] = 0;
            for (int i = Convert.ToInt32(textBox1.Text); i < 255; i++)
            {
                for (int j = Convert.ToInt32(textBox2.Text); j < 255; j++)
                {
                    for (int k = Convert.ToInt32(textBox3.Text); k < 255; k++)
                    {
                        for (int l = Convert.ToInt32(textBox4.Text); l < 255; l++)
                        {
                            bytearray[3] = (byte)l;
                            if (bytearray[2] != 8 && bytearray[3] != 192)
                            {
                                SendDataToDevice(bytearray, "");
                            }
                        }
                        bytearray[2] = (byte)k;
                        if (bytearray[2] != 8 && bytearray[3] != 192)
                        {
                            SendDataToDevice(bytearray, "");
                        }
                    }
                    bytearray[1] = (byte)j;
                    if (bytearray[2] != 8 && bytearray[3] != 192)
                    {
                        SendDataToDevice(bytearray, "");
                    }
                }
                bytearray[0] = (byte)i;
                if (bytearray[2] != 8 && bytearray[3] != 192)
                {
                    SendDataToDevice(bytearray, "");
                }
            }
        }
        //Handle Non-UI Working Events
        public void KeepAliveInitial(){ SendDataToDevice(InputValidation.KeepAliveInitial(), "");}
        public void KeepAliveAlternate() { SendDataToDevice(InputValidation.KeepAliveAlternate(), ""); }
        //Handle UI Working Events
        private void LoopAndSendToDevice_Click(object sender, RoutedEventArgs e)
        {
            ThreadStart startLog = delegate()
            {
                Dispatcher.Invoke(DispatcherPriority.Normal, new Action(LoopArray));
            };
            new Thread(startLog).Start();
        }
        private void SendTextBoxToDevice_Click(object sender, RoutedEventArgs e)
        {
            byte[] bytearray = new byte[8];
            bytearray[0] = (byte)int.Parse(textBox1.Text, System.Globalization.NumberStyles.HexNumber);
            bytearray[1] = (byte)int.Parse(textBox2.Text, System.Globalization.NumberStyles.HexNumber);
            bytearray[2] = (byte)int.Parse(textBox3.Text, System.Globalization.NumberStyles.HexNumber);
            bytearray[3] = (byte)int.Parse(textBox4.Text, System.Globalization.NumberStyles.HexNumber);
            bytearray[4] = (byte)int.Parse(textBox5.Text, System.Globalization.NumberStyles.HexNumber);
            bytearray[5] = (byte)int.Parse(textBox6.Text, System.Globalization.NumberStyles.HexNumber);
            bytearray[6] = (byte)int.Parse(textBox7.Text, System.Globalization.NumberStyles.HexNumber);
            bytearray[7] = (byte)int.Parse(textBox8.Text, System.Globalization.NumberStyles.HexNumber);
            SendDataToDevice(bytearray, "");
        }
        private void Player1_Click(object sender, RoutedEventArgs e){SendDataToDevice(InputValidation.Player(1), "Select Player 1");}
        private void Player2_Click(object sender, RoutedEventArgs e){SendDataToDevice(InputValidation.Player(2), "Select Player 2");}
        private void Player3_Click(object sender, RoutedEventArgs e){SendDataToDevice(InputValidation.Player(3), "Select Player 3");}
        private void Player4_Click(object sender, RoutedEventArgs e){SendDataToDevice(InputValidation.Player(4), "Select Player 4");}
        private void PlayerSearch_Click(object sender, RoutedEventArgs e){SendDataToDevice(InputValidation.PlayerSearch(), "Select Player");}
        private void Backlight_Click(object sender, RoutedEventArgs e){SendDataToDevice(InputValidation.BacklightOn(), "Illuminate Backlight");}
        public void EnableBacklight_Click(object sender, RoutedEventArgs e){SendDataToDevice(InputValidation.EnableBacklight(), "Illuminate Backlight on Key Press");}
        public void CapsLockModifier()
        {
            if (OutputValidation.capsLockModifier)
            {
                SendDataToDevice(InputValidation.CapsLockOn(), "Illuminate Caps Lock");
            }
            else
            {
                SendDataToDevice(InputValidation.CapsLockOff(), "Deluminate Caps Lock");
            }
        }
        private void CapsLock_Click(object sender, RoutedEventArgs e)
        {
            CapsLockModifier();
        }
        public void GreenModifier()
        {
            if (OutputValidation.greenModifier)
            {
                SendDataToDevice(InputValidation.GreenSquareOn(), "Illuminate Green Square");
            }
            else
            {
                SendDataToDevice(InputValidation.GreenSquareOff(), "Deluminate Green Square");
            }
        }
        private void ValidateRegistrySettings()
        {
            RegistryKey reg = Registry.LocalMachine;
            RegistryKey rk = reg.OpenSubKey(@"SYSTEM\CurrentControlSet\services\vjoy\Parameters\Device01", true);
            string s = BitConverter.ToString((byte[])rk.GetValue("HidReportDesctiptor")); //Yes it is spelled wrong
            if (s != "05-01-15-00-09-04-A1-01-05-01-85-01-09-01-15-00-26-FF-7F-75-20-95-01-A1-00-09-30-81-02-09-31-81-02-09-32-81-02-09-33-81-02-09-34-81-02-09-35-81-02-81-01-81-01-C0-75-20-95-04-81-01-05-09-15-00-25-01-55-00-65-00-19-01-29-20-75-01-95-20-81-02-C0")
            {
                //rk.SetValue("HidReportDesctiptor", "05-01-15-00-09-04-A1-01-05-01-85-01-09-01-15-00-26-FF-7F-75-20-95-01-A1-00-09-30-81-02-09-31-81-02-09-32-81-02-09-33-81-02-09-34-81-02-09-35-81-02-81-01-81-01-C0-75-20-95-04-81-01-05-09-15-00-25-01-55-00-65-00-19-01-29-20-75-01-95-20-81-02-C0",RegistryValueKind.Binary);
            }
        }
        private void GreenSquare_Click(object sender, RoutedEventArgs e)
        {
            GreenModifier();            
        }
        public void PeopleModifier()
        {
            if (OutputValidation.peopleModifier)
            {
                SendDataToDevice(InputValidation.PeopleOn(), "Illuminate People");
            }
            else
            {
                SendDataToDevice(InputValidation.PeopleOff(), "Deluminate People");
            }
        }
        private void People_Click(object sender, RoutedEventArgs e)
        {
            PeopleModifier();
        }
        public void OrangeModifier()
        {
            if (OutputValidation.orangeModifier)
            {
                SendDataToDevice(InputValidation.OrangeCircleOn(), "Illuminate Orange Circle");
            }
            else
            {
                SendDataToDevice(InputValidation.OrangeCircleOff(), "Deluminate Orange Circle");
            }
        }
        private void OrangeCircle_Click(object sender, RoutedEventArgs e)
        {
            OrangeModifier();
        }
        private void TurnOffController_Click(object sender, RoutedEventArgs e){SendDataToDevice(InputValidation.TurnOffController(), "Controller Off");}
        private void DisableKeepAlive_Click(object sender, RoutedEventArgs e){enableKeepAlive = !enableKeepAlive;}

        private void image3_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Close();
        }

        private void image4_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (WindowState == WindowState.Maximized)
            {
                WindowState = WindowState.Normal;
            }
            else
            {
                WindowState = WindowState.Maximized;
            }
        }

        private void image5_MouseDown(object sender, MouseButtonEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void image2_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //Go to 
            Process.Start("https://github.com/ryandb2/");
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            ni.Visible = false;
        }
    }

        ////   private void ChatPadAuthorization()
        ////{
        ////    // 1. -> Vendor request IN EP0 (0x01) 
        ////    //       -> SETUP C0 01 00 00 00 00 04 00
        ////    //       <- IN    80 03 0D 47
        ////    byte[] bytearray8 = new byte[8];
        ////    bytearray8[0] = 192;
        ////    bytearray8[1] = 1;
        ////    bytearray8[2] = 0;
        ////    bytearray8[3] = 0;
        ////    bytearray8[4] = 0;
        ////    bytearray8[5] = 0;
        ////    bytearray8[6] = 4;
        ////    bytearray8[7] = 0;
        ////    SendDataToDevice(bytearray8, "Authorize");
        ////    // 2. -> Vendor request OUT EP0 (0xA9)
        ////    //       -> 40 A9 0C A3 23 44 00
        ////    //       <- Expect STALLED no data
        ////    byte[] bytearray4 = new byte[4];
        ////    bytearray4[0] = 128;
        ////    bytearray4[1] = 3;
        ////    bytearray4[2] = 13;
        ////    bytearray4[3] = 71;
        ////    SendDataToDevice(bytearray4, "Authorize");
        ////    // 3. -> Vendor request OUT EP0 (0xA9)
        ////    //       -> 40 A9 44 23 03 7F 00 00
        ////    //       <- Expect STALLED no data
        ////    byte[] bytearray7 = new byte[7];
        ////    bytearray7[0] = 64;
        ////    bytearray7[1] = 169;
        ////    bytearray7[2] = 12;
        ////    bytearray7[3] = 163;
        ////    bytearray7[4] = 35;
        ////    bytearray7[5] = 68;
        ////    bytearray7[6] = 0;
        ////    SendDataToDevice(bytearray7, "Authorize");
        ////    // 4. -> Vendor request OUT EP0 (0xA9)
        ////    //       -> 40 A9 39 58 32 68 00 00
        ////    //       <- Expect STALLED no data
        ////    bytearray8 = new byte[8];
        ////    bytearray8[0] = 64;
        ////    bytearray8[1] = 169;
        ////    bytearray8[2] = 68;
        ////    bytearray8[3] = 35;
        ////    bytearray8[4] = 3;
        ////    bytearray8[5] = 127;
        ////    bytearray8[6] = 0;
        ////    bytearray8[7] = 0;
        ////    SendDataToDevice(bytearray8, "Authorize");

        ////    bytearray8 = new byte[8];
        ////    bytearray8[0] = 64;
        ////    bytearray8[1] = 169;
        ////    bytearray8[2] = 57;
        ////    bytearray8[3] = 88;
        ////    bytearray8[4] = 50;
        ////    bytearray8[5] = 104;
        ////    bytearray8[6] = 0;
        ////    bytearray8[7] = 0;
        ////    SendDataToDevice(bytearray8, "Authorize");

        ////    // 5. <- IN IF:0 EP2 3 bytes 01 03 0E
        ////    byte[] bytearray3 = new byte[3];
        ////    bytearray3[0] = 1;
        ////    bytearray3[1] = 3;
        ////    bytearray3[2] = 14;
        ////    SendDataToDevice(bytearray3, "Authorize");
        ////    // 6. -> Vendor request IN EP0 (0xA1) 
        ////    //       -> SETUP C0 A1 00 00 16 E4 02 00
        ////    //       <- IN    01 00
        ////    bytearray8 = new byte[8];
        ////    bytearray8[0] = 192;
        ////    bytearray8[1] = 161;
        ////    bytearray8[2] = 0;
        ////    bytearray8[3] = 0;
        ////    bytearray8[4] = 22;
        ////    bytearray8[5] = 228;
        ////    bytearray8[6] = 2;
        ////    bytearray8[7] = 0;
        ////    SendDataToDevice(bytearray8, "Authorize");
            
        ////    byte[] bytearray2 = new byte[2];
        ////    bytearray2[0] = 1;
        ////    bytearray2[1] = 0;
        ////    SendDataToDevice(bytearray2, "Authorize");
        ////    // 7. -> OUT IF:0 EP1 3 bytes 01 03 01
        ////    bytearray3 = new byte[3];
        ////    bytearray3[0] = 1;
        ////    bytearray3[1] = 3;
        ////    bytearray3[2] = 1;
        ////    SendDataToDevice(bytearray3, "Authorize");
        ////    // 8. -> Vendor request OUT EP0 (0xA1)
        ////    //       -> SETUP 40 A1 00 00 16 E4 02 00
        ////    //       -> OUT   09 00 
        ////    bytearray8 = new byte[8];
        ////    bytearray8[0] = 64;
        ////    bytearray8[1] = 161;
        ////    bytearray8[2] = 0;
        ////    bytearray8[3] = 0;
        ////    bytearray8[4] = 22;
        ////    bytearray8[5] = 228;
        ////    bytearray8[6] = 2;
        ////    bytearray8[7] = 0;
        ////    SendDataToDevice(bytearray8, "Authorize");

        ////    bytearray2 = new byte[2];
        ////    bytearray2[0] = 9;
        ////    bytearray2[1] = 0;
        ////    SendDataToDevice(bytearray2, "Authorize");
        ////    // 9. -> Vendor request IN EP0 (0xA1)
        ////    //       -> SETUP C0 A1 00 00 16 E4 02 00
        ////    //       -> IN    09 00                   (echo previous OUT?)
        ////    bytearray8 = new byte[8];
        ////    bytearray8[0] = 192;
        ////    bytearray8[1] = 161;
        ////    bytearray8[2] = 0;
        ////    bytearray8[3] = 0;
        ////    bytearray8[4] = 22;
        ////    bytearray8[5] = 228;
        ////    bytearray8[6] = 2;
        ////    bytearray8[7] = 0;
        ////    SendDataToDevice(bytearray8, "Authorize");

        ////    bytearray2 = new byte[2];
        ////    bytearray2[0] = 9;
        ////    bytearray2[1] = 0;
        ////    SendDataToDevice(bytearray2, "Authorize");
        ////    //10. <- IN IF:0 EP 1 3 bytes 02 03 00
        ////    bytearray3 = new byte[3];
        ////    bytearray3[0] = 2;
        ////    bytearray3[1] = 3;
        ////    bytearray3[2] = 0;
        ////    SendDataToDevice(bytearray3, "Authorize");
        ////    //11. <- IN IF:0 EP 1 3 bytes 03 03 03
        ////    bytearray3 = new byte[3];
        ////    bytearray3[0] = 3;
        ////    bytearray3[1] = 3;
        ////    bytearray3[2] = 3;
        ////    SendDataToDevice(bytearray3, "Authorize");
        ////    //12. <- IN IF:0 EP 1 3 bytes 08 03 00
        ////    bytearray3 = new byte[3];
        ////    bytearray3[0] = 8;
        ////    bytearray3[1] = 3;
        ////    bytearray3[2] = 0;
        ////    SendDataToDevice(bytearray3, "Authorize");
        ////    //13. <- IN IF:0 EP 1 3 bytes 01 03 01
        ////    bytearray3 = new byte[3];
        ////    bytearray3[0] = 1;
        ////    bytearray3[1] = 3;
        ////    bytearray3[2] = 1;
        ////    SendDataToDevice(bytearray3, "Authorize");
        ////    //14. -> Vendor request OUT EP0 (0x00)
        ////    //       -> SETUP 41 00 1F 00 02 00 00 00
        ////    //       -> IN No data
        ////    bytearray8 = new byte[8];
        ////    bytearray8[0] = 65;
        ////    bytearray8[1] = 0;
        ////    bytearray8[2] = 31;
        ////    bytearray8[3] = 0;
        ////    bytearray8[4] = 2;
        ////    bytearray8[5] = 0;
        ////    bytearray8[6] = 0;
        ////    bytearray8[7] = 0;
        ////    SendDataToDevice(bytearray8, "Authorize");
        ////    //15. -> Vendor request OUT EP0 (0x00)
        ////    //       -> SETUP 41 00 1E 00 02 00 00 00
        ////    //       -> IN No data
        ////    bytearray8 = new byte[8];
        ////    bytearray8[0] = 65;
        ////    bytearray8[1] = 0;
        ////    bytearray8[2] = 30;
        ////    bytearray8[3] = 0;
        ////    bytearray8[4] = 2;
        ////    bytearray8[5] = 0;
        ////    bytearray8[6] = 0;
        ////    bytearray8[7] = 0;
        ////    SendDataToDevice(bytearray8, "Authorize");

        ////    bytearray2 = new byte[2];
        ////    bytearray2[0] = 94;
        ////    bytearray2[1] = 233;
        ////    SendDataToDevice(bytearray2, "Authorize");

        ////}

        ////private void button8_Click(object sender, RoutedEventArgs e)
        ////{
        ////    //40 A9 0C A3 23 44 00 00
        ////    byte[] bytearray8 = new byte[8];
        ////    bytearray8[0] = 64;
        ////    bytearray8[1] = 169;
        ////    bytearray8[2] = 12;
        ////    bytearray8[3] = 163;
        ////    bytearray8[4] = 35;
        ////    bytearray8[5] = 68;
        ////    bytearray8[6] = 0;
        ////    bytearray8[7] = 0;
        ////    SendDataToDevice(bytearray8, "Authorize");
        ////    //40 A9 44 23 03 7F 00 00
        ////    bytearray8 = new byte[8];
        ////    bytearray8[0] = 64;
        ////    bytearray8[1] = 169;
        ////    bytearray8[2] = 68;
        ////    bytearray8[3] = 35;
        ////    bytearray8[4] = 3;
        ////    bytearray8[5] = 127;
        ////    bytearray8[6] = 0;
        ////    bytearray8[7] = 0;
        ////    SendDataToDevice(bytearray8, "Authorize");
        ////    //40 A9 39 58 32 68 00 00
        ////    bytearray8 = new byte[8];
        ////    bytearray8[0] = 64;
        ////    bytearray8[1] = 169;
        ////    bytearray8[2] = 57;
        ////    bytearray8[3] = 88;
        ////    bytearray8[4] = 50;
        ////    bytearray8[5] = 104;
        ////    bytearray8[6] = 0;
        ////    bytearray8[7] = 0;
        ////    SendDataToDevice(bytearray8, "Authorize");
        ////    //C0 A1 00 00 16 E4 02 00
        ////    bytearray8 = new byte[8];
        ////    bytearray8[0] = 192;
        ////    bytearray8[1] = 161;
        ////    bytearray8[2] = 0;
        ////    bytearray8[3] = 0;
        ////    bytearray8[4] = 22;
        ////    bytearray8[5] = 228;
        ////    bytearray8[6] = 2;
        ////    bytearray8[7] = 0;
        ////    SendDataToDevice(bytearray8, "Authorize");
        ////}

        ////private void button9_Click(object sender, RoutedEventArgs e)
        ////{
        ////    //87 02 8C 1F CC
        ////    byte[] bytearray5 = new byte[5];
        ////    bytearray5[0] = 135;
        ////    bytearray5[1] = 2;
        ////    bytearray5[2] = 140;
        ////    bytearray5[3] = 31;
        ////    bytearray5[4] = 204;
        ////    SendDataToDevice(bytearray5, "Authorize");
        ////    //87 02 8C 1B D0
        ////    bytearray5 = new byte[5];
        ////    bytearray5[0] = 135;
        ////    bytearray5[1] = 2;
        ////    bytearray5[2] = 140;
        ////    bytearray5[3] = 27;
        ////    bytearray5[4] = 208;
        ////    SendDataToDevice(bytearray5, "Authorize");
        ////}
    //public void StartCodes()
    //    {
    //        //SendControlRequest(hChatpadDevice, MAIN_INTERFACE, 0x40, 0xa9, 0xa30c, 0x4423, 0x0000);
    //        //SendControlRequest(hChatpadDevice, MAIN_INTERFACE, 0x40, 0xa9, 0x2344, 0x7f03, 0x0000);
    //        //SendControlRequest(hChatpadDevice, MAIN_INTERFACE, 0x40, 0xa9, 0x5839, 0x6832, 0x0000);
    //        //40 A9 A3 0C 44 23 00 00
    //        byte[] temparray = new byte[10];
    //        temparray[0] = 64;
    //        temparray[1] = 169;
    //        temparray[2] = 12;
    //        temparray[3] = 30;
    //        temparray[4] = 0;
    //        temparray[5] = 0;
    //        temparray[6] = 0;
    //        temparray[7] = 0;
    //        temparray[8] = 0;
    //        temparray[9] = 0;
    //        SendDataToDevice(temparray,"Start Code 1");

    //        temparray = new byte[10];
    //        temparray[0] = 64;
    //        temparray[1] = 169;
    //        temparray[2] = 35;
    //        temparray[3] = 68;
    //        temparray[4] = 127;
    //        temparray[5] = 3;
    //        temparray[6] = 0;
    //        temparray[7] = 0;
    //        temparray[8] = 0;
    //        temparray[9] = 0;
    //        SendDataToDevice(temparray, "Start Code 2");

    //        temparray = new byte[10];
    //        temparray[0] = 64;
    //        temparray[1] = 169;
    //        temparray[2] = 88;
    //        temparray[3] = 57;
    //        temparray[4] = 104;
    //        temparray[5] = 50;
    //        temparray[6] = 0;
    //        temparray[7] = 0;
    //        temparray[8] = 0;
    //        temparray[9] = 0;
    //        SendDataToDevice(temparray, "Start Code 3");
    //    }
}









