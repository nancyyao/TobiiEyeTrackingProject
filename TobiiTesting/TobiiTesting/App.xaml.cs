using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using EyeXFramework.Wpf;

namespace TobiiTesting
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        // Keep a reference to the host so it's not garbage collected.
        private WpfEyeXHost _eyeXHost;
        public App ()
        {
            _eyeXHost = new WpfEyeXHost();
            _eyeXHost.Start();
        }
        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
            _eyeXHost.Dispose(); // always dispose on exit
        }

    }
}
