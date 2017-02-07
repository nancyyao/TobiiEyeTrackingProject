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
        private bool SenderOn = true;
        private bool ReceiverOn = true;
        private static int ReceiverPort = 11000, SenderPort = 11000;//ReceiverPort is the port used by Receiver, SenderPort is the port used by Sender
        private bool communication_started_Receiver = false;//indicates whether the Receiver is ready to receive message(coordinates). Used for thread control
        private bool communication_started_Sender = false;//Indicate whether the program is sending its coordinates to others. Used for thread control
        private System.Threading.Thread communicateThread_Receiver; //Thread for receiver
        private System.Threading.Thread communicateThread_Sender;   //Thread for sender
        private static string SenderIP = "", ReceiverIP = ""; //The IP's for sender and receiver.
        private static string defaultSenderIP = "10.105.91.168"; //The default IP for sending messages.
                                                                   //SenderIP = 129.105.146.201, 10.105.91.168
                                                                   // private static int x_received, y_received;
        private static string IPpat = @"(\d+)(\.)(\d+)(\.)(\d+)(\.)(\d+)\s+"; // regular expression used for matching ip address
        private Regex r = new Regex(IPpat, RegexOptions.IgnoreCase);//regular expression variable
        private static string NumPat = @"(\d+)\s+";
        private Regex regex_num = new Regex(NumPat, RegexOptions.IgnoreCase);
        private System.Windows.Threading.DispatcherTimer dispatcherTimer;
        private static int[] cards_arr;
        private static int[] received_cards_arr = {2,3,4,5,6};
        private static String sending;
        private static String received;

        int left_coord, top_coord, start_ind, middle_ind;

        //UI
        bool drag = false; //True when card is being moved
        int score = 0;
        int endscore = 0;
        //Timer
        int lastTime = DateTime.Now.TimeOfDay.Seconds;
        bool firstClick = true;
        int workTime = 0;
        //Set cards as fish or leaves: f for fish, l for leaves
        String set = "1";
        //Keeps track of original card position
        double startX;
        double startY;


        Rectangle lastClicked;
        #endregion

        public MainWindow()
        {
            InitializeComponent();
            Message = string.Empty;

            //  DispatcherTimer setup
            dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
            dispatcherTimer.Tick += new EventHandler(update);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 25);
            shuffleCards();

            cards_arr = new int[5] {6,7,8,9,10};

            if (ReceiverOn)
            {
                IPHostEntry ipHostInfo = Dns.GetHostByName(Dns.GetHostName());
                IPAddress ipAddress = ipHostInfo.AddressList[0];
                Receive_Status_Text.Text = "Receiving Data at\nIP:" + ipAddress.ToString();
                Receive_Status_Text.Visibility = Visibility.Visible;
            }
            if (SenderOn)
            {
                SenderIP = defaultSenderIP;
                Share_Status_Text.Text = "Sharing Data to\nIP:" + SenderIP.ToString();
                Share_Status_Text.Visibility = Visibility.Visible;
                communication_started_Sender = false;
            }
        }

        public string Message { get; private set; }

        private void shuffleCards() {
            Random rand = new Random();
            Rectangle rect;
            bool left = true;
            foreach (UIElement child in canvas.Children)
            {
                if (Canvas.GetZIndex(child) < 50) {
                    rect = child as Rectangle;
                    Canvas.SetTop(rect,200);
                    Canvas.SetZIndex(rect, rand.Next(20));
                    if (left & rect.Name.Substring(1, 1).CompareTo(set) == 1)
                    {
                        Canvas.SetLeft(rect, 225);
                    }
                    else if (rect.Name.Substring(1, 1).CompareTo(set) == 1)
                    {
                        Canvas.SetLeft(rect, 1000);
                    }
                    else
                    {
                        Canvas.SetLeft(rect, -200);
                    }
                    left = !left;
                }
            }
        }

        private void StackPanel_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (firstClick) {
                firstClick = false;
                dispatcherTimer.Start();

            }
            Rectangle obj = sender as Rectangle;
            lastClicked = obj;
            if (!drag)
            {
                Zarrange(obj);
                startX = Canvas.GetLeft(obj);
                startY = Canvas.GetTop(obj);
                if (Canvas.GetTop(obj) > 500)
                {
                    if (Canvas.GetLeft(obj) < 700)
                    {
                        if (obj.Name.Substring(2,1).CompareTo("1") == 1)
                        {
                            score--;
                            endscore--;
                        }
                    }
                    else
                    {
                        if (obj.Name.Substring(2,1).CompareTo("2") != 1)
                        {
                            score--;
                            endscore--;
                        }
                    }
                }
            }
            else
            {
                if (Canvas.GetTop(obj) > 500) {
                    if (Canvas.GetLeft(obj) < 700)
                    {
                        if (obj.Name.Substring(2, 1).CompareTo("1") == 0)
                        {
                            score++;
                            endscore++;
                        }
                        else
                        {
                            Canvas.SetLeft(obj, startX);
                            Canvas.SetTop(obj, startY);
                            score--;
                        }
                    }
                    else
                    {
                        if (obj.Name.Substring(2,1).CompareTo("2") == 0)
                        {
                            score++;
                            endscore++;
                        }
                        else
                        {
                            Canvas.SetLeft(obj, startX);
                            Canvas.SetTop(obj, startY);
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

        void updateWorkTime() {
            if (DateTime.Now.TimeOfDay.Seconds != lastTime & !firstClick & endscore < 16)
            {
                lastTime = DateTime.Now.TimeOfDay.Seconds;
                workTime++;
            }
            if (workTime % 60 < 10)
            {
                time.Text = "Time: " + workTime / 60 + ":0" + workTime % 60;
            }
            else
            {
                time.Text = "Time: " + workTime / 60 + ":" + workTime % 60;
            }
            if (endscore == 16)
            {
                background.Fill = new SolidColorBrush(System.Windows.Media.Colors.LimeGreen);
                Canvas.SetZIndex(background, 0);
            }
        }
        
        void update(object sender, EventArgs e) //sender/receiver
        {
            if (!firstClick) {
                sending = lastClicked.Name + ":" + Canvas.GetLeft(lastClicked).ToString() + "|" + Canvas.GetTop(lastClicked).ToString();
            }

            //If user pressed Receiver or Cursor button but communication haven't started yet or has terminated, start a thread on tryCommunicateReceiver()
            if (ReceiverOn && communication_started_Receiver == false)
            {
                communication_started_Receiver = true;
                communicateThread_Receiver = new System.Threading.Thread(new ThreadStart(() => tryCommunicateReceiver(sending)));
                communicateThread_Receiver.Start();
            }

            //If user pressed Sender button but communication haven't started yet or has terminated, start a thread on tryCommunicateSender()
            if (SenderOn && communication_started_Sender == false)
            {
                communication_started_Sender = true;
                communicateThread_Sender = new System.Threading.Thread(new ThreadStart(() => tryCommunicateSender(sending)));
                communicateThread_Sender.Start();
            }
            // Invoke thread
            //Console.WriteLine(System.Windows.Forms.Control.MousePosition.ToString());
            //Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(() => UpdateUI(sending)));

            if (received != null) {
                Console.WriteLine(received);
                test.Text = received.ToString();
                Rectangle partnerClicked = FindName(received.Substring(0, 4)) as Rectangle;
                start_ind = received.IndexOf(":");
                middle_ind = received.IndexOf("|");
                left_coord = Convert.ToInt32(received.Substring(start_ind + 1, middle_ind - start_ind - 1));
                top_coord = Convert.ToInt32(received.Substring(middle_ind + 1, received.Length - middle_ind - 1));
                Canvas.SetLeft(partnerClicked,  left_coord);
                Canvas.SetTop(partnerClicked, top_coord);
            }

            updateWorkTime();
        }
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
        public void tryCommunicateReceiver(String x)
        {
            IPHostEntry ipHostInfo = Dns.Resolve(Dns.GetHostName());
            ReceiverIP = ipHostInfo.AddressList[0].ToString();

            while (ReceiverIP == "")
            {
                System.Threading.Thread.Sleep(1000);
            }
            AsynchronousSocketListener.StartListening();
        }
        public void tryCommunicateSender(String x)
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
                                //received_cards_arr = s.Split(',').Select(str => int.Parse(str)).ToArray(); ;
                                // received = Convert.ToInt32(content.Substring(x_start_ind + 3, x_end_ind - (x_start_ind + 3)));
                                received = s;
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

            public static void StartClient(String x)
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

            
        }
        #endregion
    }
}