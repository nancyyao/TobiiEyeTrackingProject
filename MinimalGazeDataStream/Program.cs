//-----------------------------------------------------------------------
// Copyright 2014 Tobii Technology AB. All rights reserved.
//-----------------------------------------------------------------------

namespace MinimalGazeDataStream
//for recording data
{
    using EyeXFramework;
    using System;
    using Tobii.EyeX.Framework;

    public static class Program
    {
        public static void Main(string[] args)
        {
            string datapoint;
            string x;
            string y;
            string time;
            double seconds;
            int recordEveryXTime = 15;
            int recordEveryXTimeCounter = 0;
            string filepath = "C:/Users/Master/Documents/GitHub/TobiiEyeTrackingProject/MinimalGazeDataStream/gazedata.txt";
            using (var eyeXHost = new EyeXHost())
            {
                System.IO.StreamWriter startfile = new System.IO.StreamWriter(filepath, true);
                startfile.WriteLine("STARTING DATA RECORDING");
                startfile.Close();
                // Create a data stream: lightly filtered gaze point data.
                // Other choices of data streams include EyePositionDataStream and FixationDataStream.
                using (var lightlyFilteredGazeDataStream = eyeXHost.CreateGazePointDataStream(GazePointDataMode.LightlyFiltered))
                {
                    eyeXHost.Start();

                    lightlyFilteredGazeDataStream.Next += (s, e) =>
                    {
                        if ((recordEveryXTimeCounter % recordEveryXTime == 0))
                        {
                            x = Convert.ToString(e.X);
                            y = Convert.ToString(e.Y);
                            seconds = e.Timestamp / 1000; //time in seconds
                            time = Convert.ToString(seconds);
                            Console.WriteLine("Gaze point at ({0:0.0}, {1:0.0}) @{2:0}", e.X, e.Y, e.Timestamp);
                            datapoint = "Gaze point at (" + x + ", " + y + ") @" + time;
                            System.IO.StreamWriter file = new System.IO.StreamWriter(filepath, true);
                            file.WriteLine(datapoint);
                            file.Close();
                        }
                        recordEveryXTimeCounter++;
                    };

                    // Let it run until a key is pressed.
                    Console.WriteLine("Listening for gaze data, press any key to exit...");
                    Console.In.Read();
                }
            }
        }
    }
}
