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
        int score = 0;

        public string Message { get; private set; }

        /// <summary>
        /// Handler for Behavior.HasGazeChanged events for the instruction text block.
        /// </summary>

        private void StackPanel_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            Rectangle obj = sender as Rectangle;
            if (!drag)
            {
                Zarrange(obj);
                if (Canvas.GetTop(obj) > 500)
                {
                    if (Canvas.GetLeft(obj) < 700)
                    {
                        if (obj.Name.Substring(0, 1).CompareTo("A") == 1)
                        {
                            score--;
                        }
                    }
                    else
                    {
                        if (obj.Name.Substring(0, 1).CompareTo("B") != 1)
                        {
                            score--;
                        }
                    }
                }
            }
            else
            {
                if (Canvas.GetTop(obj) > 500) {
                    if (Canvas.GetLeft(obj) < 700)
                    {
                        if (obj.Name.Substring(0, 1).CompareTo("A") == 0)
                        {
                            score++;
                        }
                        else
                        {
                            score--;
                        }
                    }
                    else
                    {
                        if (obj.Name.Substring(0, 1).CompareTo("B") == 0)
                        {
                            score++;
                        }
                        else
                        {
                            score--;
                        }
                    }
                }
                scr.Text = score.ToString();
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