/******************************************************************************\
* Copyright (C) 2012-2014 Leap Motion, Inc. All rights reserved.               *
* Leap Motion proprietary and confidential. Not for distribution.              *
* Use subject to the terms of the Leap Motion SDK Agreement available at       *
* https://developer.leapmotion.com/sdk_agreement, or another agreement         *
* between Leap Motion and you, your company or other organization.             *
\******************************************************************************/
using System;
using System.Threading;
using Leap;
namespace exam
{
    class SampleListener : Listener
    {
        private Object thisLock = new Object();

        private void SafeWriteLine(String line)
        {
            lock (thisLock)
            {
                Console.WriteLine(line);
            }
        }

        public override void OnInit(Controller controller)
        {
            SafeWriteLine("Initialized");
        }

        public override void OnConnect(Controller controller)
        {
            SafeWriteLine("Connected");
            controller.EnableGesture(Gesture.GestureType.TYPE_CIRCLE);
            controller.EnableGesture(Gesture.GestureType.TYPE_KEY_TAP);
            controller.EnableGesture(Gesture.GestureType.TYPE_SCREEN_TAP);
            controller.EnableGesture(Gesture.GestureType.TYPE_SWIPE);
        }

        public override void OnDisconnect(Controller controller)
        {
            //Note: not dispatched when running in a debugger.
            SafeWriteLine("Disconnected");
        }

        public override void OnExit(Controller controller)
        {
            SafeWriteLine("Exited");
        }

        float _BeforeZAxis = 0;
        int counter = 0;
        bool _check = false;

        public override void OnFrame(Controller controller)
        {
            // Get the most recent frame and report some basic information
            Frame frame = controller.Frame();

            foreach (Hand hand in frame.Hands)
            {

                // Get the hand's normal vector and direction
                Vector normal = hand.PalmNormal;
                Vector direction = hand.Direction;

                // Get fingers

                Finger index = null;
                Finger middle = null;

                foreach (Finger finger in hand.Fingers)
                {
                    if (finger.Type == Finger.FingerType.TYPE_INDEX)
                    {
                        //SafeWriteLine(finger.TipPosition.ToString());
                        //SafeWriteLine(finger.StabilizedTipPosition.ToString());
                        index = finger;
                    }
                    if (finger.Type == Finger.FingerType.TYPE_MIDDLE)
                    {
                        middle = finger;
                    }
                }
                Vector dist = null;
                if (index != null && middle != null)
                {
                    dist = index.TipPosition - middle.TipPosition;
                    //SafeWriteLine(dist.MagnitudeSquared.ToString());
                }
                if(dist.MagnitudeSquared < 1000 && _check == false)
                {
                    
                }

                if (dist != null)
                {
                    if (dist.MagnitudeSquared < 1000)
                    {
                        //SafeWriteLine((counter++).ToString());
                        SafeWriteLine((_BeforeZAxis - index.TipPosition.z).ToString());
                        Mouse.mouse_event(Mouse.Wheel, 0, 0, (int)(index.TipPosition.z - _BeforeZAxis), 0);
                    }
                }

                if (index != null)
                {
                    _BeforeZAxis = index.TipPosition.z;
                }



            }

            if (!frame.Hands.IsEmpty || !frame.Gestures().IsEmpty)
            {
                //feWriteLine("");
            }
        }
    }

    class Program
    {
        public static void Main()
        {
            // Create a sample listener and controller
            SampleListener listener = new SampleListener();
            Controller controller = new Controller();
            controller.SetPolicy(Controller.PolicyFlag.POLICY_BACKGROUND_FRAMES);

            // Have the sample listener receive events from the controller
            controller.AddListener(listener);

            // Keep this process running until Enter is pressed
            Console.WriteLine("Press Enter to quit...");
            Console.ReadLine();

            // Remove the sample listener when done
            controller.RemoveListener(listener);
            controller.Dispose();
        }
    }

    public class Mouse
    {
        [System.Runtime.InteropServices.DllImport("user32.dll")]

        public static extern void mouse_event(int dwFlags, int dx, int dy, int dwData, int dwExtraInfo);
        public const int LeftDown = 0x02;
        public const int LeftUp = 0x04;
        public const int RightDown = 0x08;
        public const int RightUp = 0x10;
        public const int MiddleDown = 0x0020;
        public const int MiddleUp = 0x0040;
        public const int Move = 0x0001;
        public const int Absolute = 0x8000;
        public const int Wheel = 0x0800;
        public const int HWWheel = 0x01000;

    }
}