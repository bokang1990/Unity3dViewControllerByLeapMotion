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
        Vector _beforeVector = new Vector(0f,0f,0f);
        bool _AttachFingerCheck = false;
        float _sensitive = 10f;
        float _moveDist = 100f;

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

        public override void OnFrame(Controller controller)
        {
            // Get the most recent frame and report some basic information
            Frame frame = controller.Frame();

            foreach (Hand hand in frame.Hands)
            {
                // Get the hand's normal vector and direction
                Vector normal = hand.PalmNormal;
                Vector direction = hand.Direction;
                
                Finger index = null;
                Finger middle = null;

                foreach (Finger finger in hand.Fingers)
                {
                    if (finger.Type == Finger.FingerType.TYPE_INDEX)
                    {
                        index = finger;
                    }
                    if (finger.Type == Finger.FingerType.TYPE_MIDDLE)
                    {
                        middle = finger;
                    }
                }

                Vector dist = null;
                dist = index.TipPosition - middle.TipPosition;

                if (dist.MagnitudeSquared < 700 && _AttachFingerCheck == false)
                {
                    _AttachFingerCheck = true;
                    _beforeVector.z = index.TipPosition.z;
                }

                if (dist.MagnitudeSquared >= 700)
                {
                    _AttachFingerCheck = false;
                }

                if (_AttachFingerCheck == true && Math.Abs(index.TipPosition.z - _beforeVector.z) >= _sensitive)
                {
                    //SafeWriteLine((index.TipPosition.z - _BeforeZAxis).ToString());
                    SafeWriteLine(((index.TipPosition.z - _beforeVector.z) * _moveDist / _sensitive).ToString());
                    Mouse.mouse_event(Mouse.Wheel, 0, 0, (int)((index.TipPosition.z - _beforeVector.z) * _moveDist / _sensitive), 0);
                    _beforeVector.z = index.TipPosition.z;
                }
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