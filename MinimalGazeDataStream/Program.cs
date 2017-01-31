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
            string time;
            int recordEveryXTime = 15; //don't need to record every point...
            int recordEveryXTimeCounter = 0;
            string path = "C:/Users/Master/Documents/GitHub/TobiiEyeTrackingProject/datastreams/" + "A_"+DateTime.Now.ToString("MM-dd_hh-mm") + ".txt";
            using (var eyeXHost = new EyeXHost())
            {
                // Create a data stream: lightly filtered gaze point data.
                // Other choices of data streams include EyePositionDataStream and FixationDataStream.
                using (var lightlyFilteredGazeDataStream = eyeXHost.CreateGazePointDataStream(GazePointDataMode.LightlyFiltered))
                {
                    eyeXHost.Start();

                    lightlyFilteredGazeDataStream.Next += (s, e) =>
                    {
                        if ((recordEveryXTimeCounter % recordEveryXTime == 0))
                        {
                            // Convert ms to readable time format
                            time = DateTime.Now.ToString("hh:mm:ss.fff");
                            Console.WriteLine("Gaze point at ({0:0.00}, {1:0.00}) @ {2:0}", e.X, e.Y, time);
                            datapoint = string.Format("Gaze point at ({0:0.00}, {1:0.00}) @ {2:0}", e.X, e.Y, time) + "\n";
                            System.IO.StreamWriter file = new System.IO.StreamWriter(path, true);
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
