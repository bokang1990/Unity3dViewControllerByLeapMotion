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
    //mouse control class
    public class Mouse
    {
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern void mouse_event(int dwFlags, int dx, int dy, int dwData, int dwExtraInfo);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern Int32 GetCursorPos(out POINT pt);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern bool SetCursorPos(int X, int Y);

        public const int LeftDown = 0x0002;
        public const int LeftUp = 0x0004;
        public const int RightDown = 0x0008;
        public const int RightUp = 0x0010;
        public const int MiddleDown = 0x0020;
        public const int MiddleUp = 0x0040;
        public const int Move = 0x0001;
        public const int Absolute = 0x8000;
        public const int Wheel = 0x0800;
        public const int HWWheel = 0x1000;
        
    }

    public struct POINT
    {
        public Int32 x ;
        public Int32 y ;
    }

    class SampleListener : Listener
    {
        Vector _beforeVector = new Vector(0f,0f,0f);    //befor moving point vector
        bool _attachFingerCheck = false;    //attach checking index,middle finger

        float _attachSensitivity = 700f;  //attach checking sensitivity
        float _stepSensitivity = 5f;     //one move step sensitivity
        float _wheelMoveDist = 100f;      //wheel move distance
        float _xMoveDist = 10f;           //x axis move distance
        float _yMoveDist = 10f;           //y axis move distance
        int _counter = 0;
        POINT mousePT;                    //mouse pointer position
        
        private Object thisLock = new Object();

        private void SafeWriteLine(String line)
        {
            lock (thisLock)
            {
                Console.WriteLine(line);
            }
        }
        
        public override void OnFrame(Controller controller)
        {
            // Get the most recent frame and report some basic information
            Frame frame = controller.Frame();
            
            foreach (Hand hand in frame.Hands)
            {

                //SafeWriteLine(hand.PalmVelocity.MagnitudeSquared.ToString());
                if (hand.PalmVelocity.MagnitudeSquared > (500 * 500))
                {
                    SafeWriteLine("So Fast!"+ _counter++);
                    return;
                }

                // Get the hand's normal vector and direction
                Vector normal = hand.PalmNormal;
                Vector direction = hand.Direction;
                
                Finger index = null;
                Finger middle = null;

                //index,middle finger setting
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
                
                //index-middle finger tips distance for checking attach 
                Vector dist = index.TipPosition - middle.TipPosition;

                //if attach index-middle finger, checking and setting
                if (dist.MagnitudeSquared < _attachSensitivity && _attachFingerCheck == false)  
                {
                    _attachFingerCheck = true;
                    _beforeVector = index.TipPosition;
                    Mouse.mouse_event(Mouse.MiddleDown, 0, 0, 0, 0);
                    Mouse.GetCursorPos(out mousePT);    //save cursor positon
                }

                //if seperate index-middle finger, checking and setting
                if (dist.MagnitudeSquared >= _attachSensitivity && _attachFingerCheck == true)
                {
                    _attachFingerCheck = false;
                    Mouse.mouse_event(Mouse.MiddleUp, 0, 0, 0, 0);
                    Mouse.SetCursorPos(mousePT.x, mousePT.y);   //load cursor positon
                }

                //wheel move
                if (_attachFingerCheck == true && Math.Abs(index.TipPosition.z - _beforeVector.z) >= _stepSensitivity)
                {
                    int realMoveWheel = (int)((index.TipPosition.z - _beforeVector.z) * _wheelMoveDist / _stepSensitivity);
                    Mouse.mouse_event(Mouse.Wheel, 0, 0, realMoveWheel, 0);
                    _beforeVector.z = index.TipPosition.z;
                }

                //x move
                if (_attachFingerCheck == true && Math.Abs(index.TipPosition.x - _beforeVector.x) >= _stepSensitivity)
                {
                    int realMovePosX = (int)((index.TipPosition.x - _beforeVector.x) * _xMoveDist / _stepSensitivity);
                    Mouse.mouse_event(Mouse.Move, realMovePosX, 0, 0, 0);
                    _beforeVector.x = index.TipPosition.x;
                }

                //y move
                if (_attachFingerCheck == true && Math.Abs(index.TipPosition.y - _beforeVector.y) >= _stepSensitivity)
                {
                    int realMovePosY = -(int)((index.TipPosition.y - _beforeVector.y) * _yMoveDist / _stepSensitivity);
                    Mouse.mouse_event(Mouse.Move, 0, realMovePosY, 0, 0);
                    _beforeVector.y = index.TipPosition.y;
                }

                break;  //for one hand
            }

            //suddenly disappear, setting init
            if(_attachFingerCheck == true && frame.Hands.IsEmpty)
            {
                _attachFingerCheck = false;
                Mouse.mouse_event(Mouse.MiddleUp, 0, 0, 0, 0);
                Mouse.GetCursorPos(out mousePT);
            }

            //attach finger checking log
            //if (_attachFingerCheck == true)
            //{
            //    SafeWriteLine((_counter++).ToString());     
            //}
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

}