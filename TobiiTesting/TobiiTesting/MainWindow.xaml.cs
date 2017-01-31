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

using MessageBox = System.Windows.MessageBox;
//using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;
using System.Text.RegularExpressions;

namespace TobiiTesting
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 

    public partial class MainWindow : Window
    {
        #region Variables
        //send/receive
        private bool SenderOn = false;
        private bool ReceiverOn = false;
        private static int ReceiverPort = 11000, SenderPort = 11000;//ReceiverPort is the port used by Receiver, SenderPort is the port used by Sender
        private bool communication_started_Receiver = false;//indicates whether the Receiver is ready to receive message(coordinates). Used for thread control
        private bool communication_started_Sender = false;//Indicate whether the program is sending its coordinates to others. Used for thread control
        private System.Threading.Thread communicateThread_Receiver; //Thread for receiver
        private System.Threading.Thread communicateThread_Sender;   //Thread for sender
        private static string SenderIP = "", ReceiverIP = ""; //The IP's for sender and receiver.
        private static string defaultSenderIP = "169.254.50.139"; //The default IP for sending messages.
                                                                  //SenderIP = "169.254.50.139"; //seahorse laptop.//SenderIP = "169.254.41.115"; //Jellyfish laptop
        // private static int x_received, y_received;
        private static string IPpat = @"(\d+)(\.)(\d+)(\.)(\d+)(\.)(\d+)\s+"; // regular expression used for matching ip address
        private Regex r = new Regex(IPpat, RegexOptions.IgnoreCase);//regular expression variable
        private static string NumPat = @"(\d+)\s+";
        private Regex regex_num = new Regex(NumPat, RegexOptions.IgnoreCase);
        private System.Windows.Threading.DispatcherTimer dispatcherTimer;
        private static int[] cards_arr;
        private static int[] received_cards_arr;
        #endregion

        public MainWindow()
        {
            InitializeComponent();
            Message = string.Empty;

            //  DispatcherTimer setup
            dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
            dispatcherTimer.Tick += new EventHandler(update);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 25);
            dispatcherTimer.Start();
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
                scr.Text = "Score: " + score;
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






        void update(object sender, EventArgs e) //sender/receiver
        {
            //If user pressed Receiver or Cursor button but communication haven't started yet or has terminated, start a thread on tryCommunicateReceiver()
            if (ReceiverOn && communication_started_Receiver == false)
            {
                communication_started_Receiver = true;
                communicateThread_Receiver = new System.Threading.Thread(new ThreadStart(() => tryCommunicateReceiver(cards_arr)));
                communicateThread_Receiver.Start();
            }

            //If user pressed Sender button but communication haven't started yet or has terminated, start a thread on tryCommunicateSender()
            if (SenderOn && communication_started_Sender == false)
            {
                communication_started_Sender = true;
                communicateThread_Sender = new System.Threading.Thread(new ThreadStart(() => tryCommunicateSender(cards_arr)));
                communicateThread_Sender.Start();
            }
            // Invoke thread
            //Console.WriteLine(System.Windows.Forms.Control.MousePosition.ToString());
            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(() => UpdateUI(cards_arr)));
        }
        private void UpdateUI(int[] cards)
        {
            int x;
            int y;
            if (ReceiverOn)
            {
                for (int i = 0; i < cards.Length; i += 2)
                {
                    x = cards[i];
                    y = cards[i + 1];
                    //set positions of boxes 0 to i!
                }
            }
        }
        //private void ExecuteSelectedButton(string selectedButtonName)
        //{
        //    if (selectedButtonName == null) return;
        //    switch (selectedButtonName)
        //    {
        //        case "Receive":  //receive from other computer
        //            ReceiverOn = !ReceiverOn;
        //            if (ReceiverOn)
        //            {
        //                IPHostEntry ipHostInfo = Dns.GetHostByName(Dns.GetHostName());
        //                IPAddress ipAddress = ipHostInfo.AddressList[0];
        //                Receive_Text.Text = "Receiver On\nIP:" + ipAddress.ToString();
        //                Receive_Status_Text.Text = "Receiving Data\nIP:" + ipAddress.ToString();
        //                Receive_Status_Text.Visibility = Visibility.Visible;
        //                //Receiver_Pop.IsOpen = true;
        //                //Receiver_Pop_TextBox.Text = "Please enter your IP address";
        //                //Receiver_Pop_TextBox.SelectAll();
        //            }
        //            else
        //            {
        //                Receive_Text.Text = "Receive Off";
        //                Receive_Status_Text.Visibility = Visibility.Hidden;
        //                //if wrap below???
        //                ReceiverIP = "";
        //                try
        //                {
        //                    communicateThread_Receiver.Abort();

        //                }
        //                catch (Exception e)
        //                {
        //                    Console.WriteLine(e.ToString());
        //                }
        //                communication_started_Receiver = false;
        //            }
        //            break;
        //        case "Share": //send to other computer
        //            SenderOn = !SenderOn;
        //            if (SenderOn)
        //            {
        //               // Share_Text.Text = "Share On";
        //                SenderIP = defaultSenderIP;
        //                Share_Status_Text.Text = "Sharing Data\nIP:" + SenderIP.ToString();
        //                Share_Status_Text.Visibility = Visibility.Visible;
        //                communication_started_Sender = false;
        //            }
        //            else
        //            {
        //               // Share_Text.Text = "Share Off";
        //                SenderIP = "";
        //                Share_Status_Text.Visibility = Visibility.Hidden;
        //                try
        //                {
        //                    communicateThread_Sender.Abort();
        //                }
        //                catch (Exception e)
        //                {
        //                    Console.WriteLine(e.ToString());
        //                }
        //                communication_started_Sender = false;
        //            }
        //            break;
        //    }
        //}

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            //CleanUp();
            SenderOn = false;
            ReceiverOn = false;
            try
            {
                communicateThread_Receiver.Abort();
                communicateThread_Sender.Abort();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            base.OnClosing(e);
        }

        #region Sender/Receiver Methods
        public void tryCommunicateReceiver(int[] x)
        {
            IPHostEntry ipHostInfo = Dns.Resolve(Dns.GetHostName());
            ReceiverIP = ipHostInfo.AddressList[0].ToString();

            while (ReceiverIP == "")
            {
                System.Threading.Thread.Sleep(1000);
            }
            AsynchronousSocketListener.StartListening();
        }
        public void tryCommunicateSender(int[] x)
        {
            while (SenderIP == "")
            {
                System.Threading.Thread.Sleep(1000);
            }
            SynchronousClient.StartClient(x);
            communication_started_Sender = false;

            //AsynchronousSocketListener.StartListening();
        }
        public class StateObject
        {
            // Client  socket.
            public Socket workSocket = null;
            // Size of receive buffer.
            public const int BufferSize = 1024;
            // Receive buffer.
            public byte[] buffer = new byte[BufferSize];
            // Received data string.
            public StringBuilder sb = new StringBuilder();
        }
        //THis is the Receiver function (Asyncronous)
        // Citation: https://msdn.microsoft.com/en-us/library/fx6588te%28v=vs.110%29.aspx
        public class AsynchronousSocketListener
        {
            // Thread signal.
            public static ManualResetEvent allDone = new ManualResetEvent(false);
            public AsynchronousSocketListener()
            {
            }
            public static void StartListening()
            {
                if (ReceiverIP != "")
                {
                    // Data buffer for incoming data.
                    byte[] bytes = new Byte[1024];

                    // Establish the local endpoint for the socket.
                    // The DNS name of the computer
                    IPHostEntry ipHostInfo = Dns.Resolve(Dns.GetHostName());
                    IPAddress ipAddress = IPAddress.Parse(ReceiverIP);
                    IPEndPoint localEndPoint = new IPEndPoint(ipAddress, ReceiverPort);

                    // Create a TCP/IP socket.
                    Socket listener = new Socket(AddressFamily.InterNetwork,
                        SocketType.Stream, ProtocolType.Tcp);

                    // Bind the socket to the local endpoint and listen for incoming connections.
                    try
                    {
                        listener.Bind(localEndPoint);
                        listener.Listen(100);
                        //ommunication_received==false
                        while (true)
                        {
                            // Set the event to nonsignaled state.
                            allDone.Reset();

                            // Start an asynchronous socket to listen for connections.
                            //Console.WriteLine("Waiting for a connection...");
                            listener.BeginAccept(
                                new AsyncCallback(AcceptCallback),
                                listener);

                            allDone.WaitOne();

                            // Wait until a connection is made before continuing.
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                    }
                    //Console.WriteLine("\nPress ENTER to continue...");
                    //Console.Read();
                }
            }
            public static void AcceptCallback(IAsyncResult ar)
            {
                // Signal the main thread to continue.
                allDone.Set();

                // Get the socket that handles the client request.
                Socket listener = (Socket)ar.AsyncState;
                Socket handler = listener.EndAccept(ar);

                // Create the state object.
                StateObject state = new StateObject();
                state.workSocket = handler;
                handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                    new AsyncCallback(ReadCallback), state);
            }
            public static void ReadCallback(IAsyncResult ar)
            {
                String content = String.Empty;

                // Retrieve the state object and the handler socket
                // from the asynchronous state object.
                StateObject state = (StateObject)ar.AsyncState;
                Socket handler = state.workSocket;

                // Read data from the client socket. 
                int bytesRead = handler.EndReceive(ar);

                if (bytesRead > 0)
                {
                    // There  might be more data, so store the data received so far.
                    state.sb.Append(Encoding.ASCII.GetString(
                        state.buffer, 0, bytesRead));

                    // Check for end-of-file tag. If it is not there, read more data.
                    content = state.sb.ToString();
                    if (content.IndexOf("<EOF>") > -1)
                    {
                        // All the data has been read from the client. Display it on the console.
                        int x_start_ind = content.IndexOf("x: "), x_end_ind = content.IndexOf("xend ");
                        // int x_start_ind = content.IndexOf("x: "), x_end_ind = content.IndexOf("xend ");
                        // int y_start_ind = content.IndexOf("y: "), y_end_ind = content.IndexOf("yend ");

                        if (x_start_ind > -1 && x_end_ind > -1)
                        {
                            try
                            {
                                //convert the received string into x and y                                
                                // x_received = Convert.ToInt32(content.Substring(x_start_ind + 3, x_end_ind - (x_start_ind + 3)));
                                // y_received = Convert.ToInt32(content.Substring(y_start_ind + 3, y_end_ind - (y_start_ind + 3)));
                                string s = content.Substring(x_start_ind + 3, x_end_ind - (x_start_ind + 3));
                                received_cards_arr = s.Split(',').Select(str => int.Parse(str)).ToArray(); ;
                            }
                            catch (FormatException)
                            {
                                Console.WriteLine("Input string is not a sequence of digits.");
                            }
                            catch (OverflowException)
                            {
                                Console.WriteLine("The number cannot fit in an Int32.");
                            }
                        }
                        // Show the data on the console.
                        //Console.WriteLine("x : {0}  y: {1}", x_received, y_received);

                        // Echo the data back to the client.
                        Send(handler, content);
                    }
                    else
                    {
                        // Not all data received. Get more.
                        handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                        new AsyncCallback(ReadCallback), state);
                    }
                }
            }

            private static void Send(Socket handler, String data)
            {
                // Convert the string data to byte data using ASCII encoding.
                byte[] byteData = Encoding.ASCII.GetBytes(data);

                // Begin sending the data to the remote device.
                handler.BeginSend(byteData, 0, byteData.Length, 0,
                    new AsyncCallback(SendCallback), handler);
            }

            private static void SendCallback(IAsyncResult ar)
            {
                try
                {
                    // Retrieve the socket from the state object.
                    Socket handler = (Socket)ar.AsyncState;

                    // Complete sending the data to the remote device.
                    int bytesSent = handler.EndSend(ar);
                    //Console.WriteLine("Sent {0} bytes to client.", bytesSent);x

                    handler.Shutdown(SocketShutdown.Both);
                    handler.Close();

                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
            }
        }
        //This is the sending function (Syncronous)
        public class SynchronousClient
        {

            public static void StartClient(int[] x)
            {
                // Data buffer for incoming data.
                byte[] bytes = new byte[1024];

                // Connect to a remote device.
                try
                {
                    // Establish the remote endpoint for the socket.
                    // This example uses port 11000 on the local computer.
                    IPHostEntry ipHostInfo = Dns.GetHostByName(Dns.GetHostName());
                    IPAddress ipAddress = IPAddress.Parse(SenderIP);
                    IPEndPoint remoteEP = new IPEndPoint(ipAddress, SenderPort);

                    // Create a TCP/IP  socket.
                    Socket sender = new Socket(AddressFamily.InterNetwork,
                        SocketType.Stream, ProtocolType.Tcp);

                    // Connect the socket to the remote endpoint. Catch any errors.
                    try
                    {
                        sender.Connect(remoteEP);

                        Console.WriteLine("Socket connected to {0}",
                            sender.RemoteEndPoint.ToString());
                        //
                        string array_to_string = string.Join(",", x);
                        string message_being_sent = "x: " + x + "xend <EOF>";
                        //string message_being_sent = "x: " + x + "xend y: " + y + "yend cursorx: " +
                        //    System.Windows.Forms.Cursor.Position.X + "cursorxend cursory: " +
                        //    System.Windows.Forms.Cursor.Position.Y + "cursoryend <EOF>";
                        // Encode the data string into a byte array.
                        byte[] msg = Encoding.ASCII.GetBytes(message_being_sent);

                        // Send the data through the socket.
                        int bytesSent = sender.Send(msg);

                        // Receive the response from the remote device.
                        int bytesRec = sender.Receive(bytes);
                        Console.WriteLine("Echoed test = {0}",
                            Encoding.ASCII.GetString(bytes, 0, bytesRec));

                        // Release the socket.
                        sender.Shutdown(SocketShutdown.Both);
                        sender.Close();

                    }
                    catch (ArgumentNullException ane)
                    {
                        Console.WriteLine("ArgumentNullException : {0}", ane.ToString());
                    }
                    catch (SocketException se)
                    {
                        Console.WriteLine("SocketException : {0}", se.ToString());
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Unexpected exception : {0}", e.ToString());
                    }

                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
            }

            public static string data = null;

            //public static void StartListening()
            //{
            //    // Data buffer for incoming data.
            //    byte[] bytes = new Byte[1024];

            //    // Establish the local endpoint for the socket.
            //    // Dns.GetHostName returns the name of the 
            //    // host running the application.
            //    IPHostEntry ipHostInfo = Dns.GetHostByName(Dns.GetHostName());
            //    IPAddress ipAddress = IPAddress.Parse(SenderIP);
            //    IPEndPoint localEndPoint = new IPEndPoint(ipAddress, SenderPort);

            //    // Create a TCP/IP socket.
            //    Socket listener = new Socket(AddressFamily.InterNetwork,
            //        SocketType.Stream, ProtocolType.Tcp);

            //    // Bind the socket to the local endpoint and 
            //    // listen for incoming connections.
            //    try
            //    {
            //        listener.Bind(localEndPoint);
            //        listener.Listen(10);

            //        // Start listening for connections.
            //        while (true)
            //        {
            //            Console.WriteLine("Waiting for a connection...");
            //            // Program is suspended while waiting for an incoming connection.
            //            Socket handler = listener.Accept();
            //            data = null;

            //            // An incoming connection needs to be processed.
            //            while (true)
            //            {
            //                bytes = new byte[1024];
            //                int bytesRec = handler.Receive(bytes);
            //                data += Encoding.ASCII.GetString(bytes, 0, bytesRec);
            //                if (data.IndexOf("<EOF>") > -1)
            //                {
            //                    break;
            //                }
            //            }
            //            int x_start_ind = data.IndexOf("x: "), x_end_ind = data.IndexOf("xend ");
            //            //int y_start_ind = data.IndexOf("y: "), y_end_ind = data.IndexOf("yend ");
            //            //int cursor_x_start_ind = data.IndexOf("cursorx: "), cursor_x_end_ind = data.IndexOf("cursorxend ");
            //            //int cursor_y_start_ind = data.IndexOf("cursory: "), cursor_y_end_ind = data.IndexOf("cursoryend ");
            //            if (x_start_ind > -1 && x_end_ind > -1)
            //            {
            //                try
            //                {
            //                    //x_received = Convert.ToInt32(data.Substring(x_start_ind + 2, x_end_ind - 1));
            //                    //y_received = Convert.ToInt32(data.Substring(y_start_ind + 2, y_end_ind - 1));
            //                }
            //                catch (FormatException)
            //                {
            //                    Console.WriteLine("Input string is not a sequence of digits.");
            //                }
            //                catch (OverflowException)
            //                {
            //                    Console.WriteLine("The number cannot fit in an Int32.");
            //                }
            //            }                        
            //            // Echo the data back to the client.
            //            byte[] msg = Encoding.ASCII.GetBytes(data);

            //            handler.Send(msg);
            //            handler.Shutdown(SocketShutdown.Both);
            //            handler.Close();
            //        }
            //    }
            //    catch (Exception e)
            //    {
            //        Console.WriteLine(e.ToString());
            //    }
            //    Console.WriteLine("\nPress ENTER to continue...");
            //    Console.Read();
            //}
        }
        #endregion
    }
}