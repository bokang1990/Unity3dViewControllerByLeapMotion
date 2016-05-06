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

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern void keybd_event(uint vk, uint scan, uint flags, uint extraInfo);

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

        public const int KeyALT = 0x012;
        public const int KeyDown = 0x00;
        public const int KeyUp = 0x02;

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

        float _attachIMSensitivity = 700f;  //index-middle attach checking sensitivity
        float _attachRPSensitivity = 900f;  //ring-pinky attach checking sensitivity
        float _stepSensitivity = 5f;     //one move step sensitivity
        float _wheelMoveDist = 100f;      //wheel move distance
        float _xMoveDist = 10f;           //x axis move distance
        float _yMoveDist = 10f;           //y axis move distance
        int _counter = 0;
        POINT mousePT;                    //mouse pointer position
        int _mode = 0;                   //move mode. 0,1
        bool _mode0Check = false;
        bool _mode1Check = false;

        public bool OnOff = true; 

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


            if(!OnOff)
            {
                return;
            }

            // Get the most recent frame and report some basic information
            Frame frame = controller.Frame();
            
            foreach (Hand hand in frame.Hands)
            {

                //SafeWriteLine(hand.PalmVelocity.MagnitudeSquared.ToString());
                if (hand.PalmVelocity.MagnitudeSquared > (500 * 500))
                {
                    //SafeWriteLine("So Fast!"+ _counter++);
                    return;
                }

                // Get the hand's normal vector and direction
                Vector normal = hand.PalmNormal;
                Vector direction = hand.Direction;
                
                Finger indexFinger = null;
                Finger middleFinger = null;
                Finger ringFinger = null;
                Finger pinkyFinger = null;

                //index,middle finger setting
                foreach (Finger finger in hand.Fingers)
                {
                    if (finger.Type == Finger.FingerType.TYPE_INDEX)
                    {
                        indexFinger = finger;
                    }
                    else if (finger.Type == Finger.FingerType.TYPE_MIDDLE)
                    {
                        middleFinger = finger;
                    }
                    else if (finger.Type == Finger.FingerType.TYPE_RING)
                    {
                        ringFinger = finger;
                    }
                    else if (finger.Type == Finger.FingerType.TYPE_PINKY)
                    {
                        pinkyFinger = finger;
                    }

                }

                Vector RPdist = ringFinger.TipPosition - pinkyFinger.TipPosition;

                //index-middle finger tips distance for checking attach 
                Vector IMdist = indexFinger.TipPosition - middleFinger.TipPosition; //index middle distance
                
                //if attach/seperate index-middle finger, checking and setting
                if (IMdist.MagnitudeSquared < _attachIMSensitivity && _attachFingerCheck == false && !_mode1Check)  
                {
                    _mode = 0;
                    Mouse.mouse_event(Mouse.MiddleDown, 0, 0, 0, 0);

                    _attachFingerCheck = true;
                    _beforeVector = indexFinger.TipPosition;
                    Mouse.GetCursorPos(out mousePT);    //save cursor positon
                    _mode0Check = true;
                }   
                else if (IMdist.MagnitudeSquared >= _attachIMSensitivity && _attachFingerCheck == true && !_mode1Check)
                {
                    Mouse.mouse_event(Mouse.MiddleUp, 0, 0, 0, 0);

                    _attachFingerCheck = false;
                    Mouse.SetCursorPos(mousePT.x, mousePT.y);   //load cursor positon
                    _mode0Check = false;
                }

                if(RPdist.MagnitudeSquared < _attachRPSensitivity && _attachFingerCheck == false && !_mode0Check)
                {
                    _mode = 1;
                    Mouse.keybd_event(0x012, 0, 0x00, 0);       //left alt key down
                    Mouse.mouse_event(Mouse.LeftDown, 0, 0, 0, 0);

                    _attachFingerCheck = true;
                    _beforeVector = indexFinger.TipPosition;
                    Mouse.GetCursorPos(out mousePT);    //save cursor positon
                    _mode1Check = true;
                }
                else if(RPdist.MagnitudeSquared >= _attachRPSensitivity && _attachFingerCheck == true && !_mode0Check)
                {
                    Mouse.keybd_event(0x012, 0, 0x02, 0);       //left alt key up
                    Mouse.mouse_event(Mouse.LeftUp, 0, 0, 0, 0);

                    _attachFingerCheck = false;
                    Mouse.SetCursorPos(mousePT.x, mousePT.y);   //load cursor positon
                    _mode1Check = false;
                }


                //wheel move
                if (_attachFingerCheck == true && Math.Abs(indexFinger.TipPosition.z - _beforeVector.z) >= _stepSensitivity && _mode == 0)
                {

                    int realMoveWheel = (int)((indexFinger.TipPosition.z - _beforeVector.z) * _wheelMoveDist / _stepSensitivity);
                    Mouse.mouse_event(Mouse.Wheel, 0, 0, realMoveWheel, 0);
                    _beforeVector.z = indexFinger.TipPosition.z;
                }

                //x move
                if (_attachFingerCheck == true && Math.Abs(indexFinger.TipPosition.x - _beforeVector.x) >= _stepSensitivity)
                {
                    int realMovePosX = (int)((indexFinger.TipPosition.x - _beforeVector.x) * _xMoveDist / _stepSensitivity);
                    Mouse.mouse_event(Mouse.Move, realMovePosX, 0, 0, 0);
                    _beforeVector.x = indexFinger.TipPosition.x;
                }

                //y move
                if (_attachFingerCheck == true && Math.Abs(indexFinger.TipPosition.y - _beforeVector.y) >= _stepSensitivity)
                {
                    int realMovePosY = -(int)((indexFinger.TipPosition.y - _beforeVector.y) * _yMoveDist / _stepSensitivity);
                    Mouse.mouse_event(Mouse.Move, 0, realMovePosY, 0, 0);
                    _beforeVector.y = indexFinger.TipPosition.y;
                }

                break;  //for one hand
            }

            //suddenly disappear, setting init
            if (_attachFingerCheck == true && frame.Hands.IsEmpty)
            {

                if (_mode0Check)
                {
                    Mouse.mouse_event(Mouse.MiddleUp, 0, 0, 0, 0);
                    _mode0Check = false;
                }

                if (_mode1Check)
                {
                    Mouse.keybd_event(0x012, 0, 0x02, 0);       //left alt key up
                    Mouse.mouse_event(Mouse.LeftUp, 0, 0, 0, 0);
                    _mode1Check = false;
                }

                _attachFingerCheck = false;
                Mouse.SetCursorPos(mousePT.x, mousePT.y);   //load cursor positon
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
            Console.WriteLine("===>>  QUIT : SPACE BAR   ///   TURN ON/OFF : c");
            while (true)
            {
                ConsoleKey c = Console.ReadKey().Key;
                if (c == ConsoleKey.C)
                {
                    listener.OnOff = !listener.OnOff;
                    if(listener.OnOff == false)
                    {
                        Console.WriteLine("TURN OFF");
                    }
                    else
                    {
                        Console.WriteLine("TURN ON");
                    }
                }
                if (c == ConsoleKey.Spacebar)
                    break;
            }

            // Remove the sample listener when done
            controller.RemoveListener(listener);
            controller.Dispose();
        }
    }

}