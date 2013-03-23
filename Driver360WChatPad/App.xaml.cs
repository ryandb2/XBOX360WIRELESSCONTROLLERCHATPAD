using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;

namespace Driver360WChatPad
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
            : base()
        {
            this.Dispatcher.UnhandledException += OnDispatcherUnhandledException;
        }
        void OnDispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            ErrorLogging.WriteLogEntry(String.Format("General uncaught exception: {0} {1}", e.Exception, e.Exception.InnerException), ErrorLogging.LogLevel.Fatal);
            ErrorLogging.logFile.Close();
        }
    }
}
