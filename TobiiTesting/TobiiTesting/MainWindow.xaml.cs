using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using EyeXFramework.Wpf;
using Tobii.EyeX.Framework;
using EyeXFramework;

namespace TobiiTesting
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 

    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Message = string.Empty;
        }

        bool drag = false;

        public string Message { get; private set; }

        /// <summary>
        /// Handler for Behavior.HasGazeChanged events for the instruction text block.
        /// </summary>
        private void Instruction_OnHasGazeChanged(object sender, RoutedEventArgs e)
        {
            var textBlock = e.Source as TextBlock;
            if (null == textBlock) { return; }

            var model = (MainWindowModel)DataContext;
            var hasGaze = textBlock.GetHasGaze();
            //model.NotifyInstructionHasGazeChanged(hasGaze);
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Q)
            {
                Close();
            }
            else if (e.Key == Key.C)
            {
                var model = (MainWindowModel)DataContext;
                model.CloseInstruction();
            }
        }

        private void Instruction_OnMouseEnter(object sender, MouseEventArgs e)
        {
            var model = (MainWindowModel)DataContext;
           // model.NotifyInstructionHasGazeChanged(true);
        }

        private void StackPanel_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!drag) {
                Zarrange(sender as Rectangle);
            }
            drag = !drag;
        }
        

        private void StackPanel_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            Rectangle over = sender as Rectangle;
            
            if (drag)
            {
                Canvas.SetLeft(over, e.GetPosition(background).X - over.Width/2);
                Canvas.SetTop(over, e.GetPosition(background).Y - over.Height/2);
            }
        }

        private void Zarrange(Rectangle top) {
            foreach (UIElement child in canvas.Children) {
                if (Canvas.GetZIndex(child) > Canvas.GetZIndex(top)) {
                    Canvas.SetZIndex(child, Canvas.GetZIndex(child) - 1);
                }
            }
            Canvas.SetZIndex(top, canvas.Children.Count);
        }
    }
}