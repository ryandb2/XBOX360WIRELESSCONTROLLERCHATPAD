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
        private static uint deadZone = 40;
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
            MyUsbDevice = UsbDevice.OpenUsbDevice(MyUsbFinder);
            if (MyUsbDevice == null) //support other product id (knock-off?)
            {
                MyUsbFinder = new UsbDeviceFinder(1118, 657);
                MyUsbDevice = UsbDevice.OpenUsbDevice(MyUsbFinder);
                if (MyUsbDevice == null)
                {
                    ErrorLogging.WriteLogEntry("USB Device not found: ", ErrorLogging.LogLevel.Fatal);
                }
            }

            IUsbDevice wholeUsbDevice = MyUsbDevice as IUsbDevice;

            timer.Tick += new EventHandler(dispatcherTimer_Tick);
            timer.Interval = new TimeSpan(0, 0, 0, 1, 0);

            if (!ReferenceEquals(wholeUsbDevice, null))
            {
                try
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
                catch (Exception e)
                {
                    ErrorLogging.WriteLogEntry(String.Format("Error opening endpoints: {0}", e.InnerException), ErrorLogging.LogLevel.Fatal);
                }
            }
            else
            {
                ErrorLogging.WriteLogEntry("Whole USB device is not implemented", ErrorLogging.LogLevel.Error);
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
                    OutputValidation.OutputMappingForJoyStick(e.Buffer, jController, controllerIndex,deadZone);
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
            try
            {
                ErrorCode ec = writer.Write(dataToSend, 2000, out bytesWritten);
                if (ec != ErrorCode.None)
                {
                    ErrorLogging.WriteLogEntry(String.Format("General error during SendDataToDevice of MainWindow class, Error Code: {0}",ec.ToString()), ErrorLogging.LogLevel.Error);
                }
            }
            catch (Exception e)
            {

                ErrorLogging.WriteLogEntry(String.Format("General error during SendDataToDevice of MainWindow class: {0}", e.InnerException), ErrorLogging.LogLevel.Error);
            }
            lastEvent = DateTime.Now;
        }
        //Handle Non-UI Working Events
        public void KeepAliveInitial(){ SendDataToDevice(InputValidation.KeepAliveInitial(), "");}
        public void KeepAliveAlternate() { SendDataToDevice(InputValidation.KeepAliveAlternate(), ""); }
        //Handle UI Working Events
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
            /*RegistryKey reg = Registry.LocalMachine;
            RegistryKey rk = reg.OpenSubKey(@"SYSTEM\CurrentControlSet\services\vjoy\Parameters\Device01", true);
            string s = BitConverter.ToString((byte[])rk.GetValue("HidReportDesctiptor")); //Yes it is spelled wrong
            if (s != "05-01-15-00-09-04-A1-01-05-01-85-01-09-01-15-00-26-FF-7F-75-20-95-01-A1-00-09-30-81-02-09-31-81-02-09-32-81-02-09-33-81-02-09-34-81-02-09-35-81-02-81-01-81-01-C0-75-20-95-04-81-01-05-09-15-00-25-01-55-00-65-00-19-01-29-20-75-01-95-20-81-02-C0")
            {
                //rk.SetValue("HidReportDesctiptor", "05-01-15-00-09-04-A1-01-05-01-85-01-09-01-15-00-26-FF-7F-75-20-95-01-A1-00-09-30-81-02-09-31-81-02-09-32-81-02-09-33-81-02-09-34-81-02-09-35-81-02-81-01-81-01-C0-75-20-95-04-81-01-05-09-15-00-25-01-55-00-65-00-19-01-29-20-75-01-95-20-81-02-C0",RegistryValueKind.Binary);
            }*/
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
            ErrorLogging.logFile.Close();
            System.Windows.Application.Current.Shutdown();
        }

        private void sliderDeadZone_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            showDeadzone();
        }
        private void leftDeadzone_Initialized(object sender, EventArgs e)
        {
            showDeadzone();
        }
        private void showDeadzone()
        {
            deadZone = (uint)sliderDeadZone.Value;
            try
            {
                leftDeadzone.Width = 108 * (sliderDeadZone.Value / 255);
                leftDeadzone.Height = 108 * (sliderDeadZone.Value / 255);
                Canvas.SetTop(leftDeadzone, 135 + (108 / 2) - leftDeadzone.Height / 2);
                Canvas.SetLeft(leftDeadzone, 98 + (108 / 2) - leftDeadzone.Width / 2);

                rightDeadzone.Width = 108 * (sliderDeadZone.Value / 255);
                rightDeadzone.Height = 108 * (sliderDeadZone.Value / 255);
                Canvas.SetTop(rightDeadzone, 226 + (108 / 2) - rightDeadzone.Height / 2);
                Canvas.SetLeft(rightDeadzone, 318 + (108 / 2) - rightDeadzone.Width / 2);
            }
            catch (Exception ex)
            {
                ErrorLogging.WriteLogEntry(String.Format("UI Elements not available for deadzone init: {0}", ex.InnerException), ErrorLogging.LogLevel.Warning);
            }
        }

        private void rightDeadzone_Initialized(object sender, EventArgs e)
        {
            showDeadzone();
        }
    }
}









