using LightBuzz.Vitruvius;
using LightBuzz.Vitruvius.WPF;
using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
using System.Runtime.InteropServices;

namespace VitruviusTest {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        public enum Mode {
            Color,
            Depth
        }

        public enum Status {
            InActive,
            Draw,
            Zoom
        }

        Mode _mode = Mode.Color;
        Status _status = Status.InActive;

        GestureController _gestureController;

        const float ZThreshold = 0.1f;
        const float DistanceThreshold = 0.02f;

        float initHandLeftX;
        float initHandLeftY;
        float initHandRightX;
        float initHandRightY;

        int windowWidth = 1366;
        int windowHeight = 768;
        int Flag = 0;
        float speed = 2.0f;

        public MainWindow() {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            KinectSensor sensor = SensorExtensions.Default();

            if (sensor != null) {
                sensor.EnableAllStreams();
                sensor.ColorFrameReady += Sensor_ColorFrameReady;
                //sensor.DepthFrameReady += Sensor_DepthFrameReady;
                sensor.SkeletonFrameReady += Sensor_SkeletonFrameReady;

                _gestureController = new GestureController(GestureType.All);
                _gestureController.GestureRecognized += GestureController_GestureRecognized;

                sensor.Start();
            }
        }

        void Sensor_ColorFrameReady(object sender, ColorImageFrameReadyEventArgs e) {
            using (var frame = e.OpenColorImageFrame()) {
                if (frame != null) {
                    if (_mode == Mode.Color) {
                        camera.Source = frame.ToBitmap();
                    }
                }
            }
        }

        void Sensor_DepthFrameReady(object sender, DepthImageFrameReadyEventArgs e) {
            using (var frame = e.OpenDepthImageFrame()) {
                if (frame != null) {
                    if (_mode == Mode.Depth) {
                        camera.Source = frame.ToBitmap();
                    }
                }
            }
        }

        void Sensor_SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e) {
            using (var frame = e.OpenSkeletonFrame()) {
                if (frame != null) {
                    canvas.ClearSkeletons();
                    //tblHeights.Text = string.Empty;

                    var skeleton = frame.Skeletons().Where(s => s.TrackingState == SkeletonTrackingState.Tracked).OrderBy(s => s.Joints[JointType.Head].Position.Z).FirstOrDefault();
                    if (skeleton != null) {
                        // Update skeleton gestures.
                        _gestureController.Update(skeleton);

                        // Draw skeleton.
                        canvas.DrawSkeleton(skeleton);

                        // Display user height.
                        tblHeights.Text = string.Format("\nUser {0}: {1}cm", skeleton.TrackingId, skeleton.Height());

                        // Extract
                        ExtractEvent(skeleton);
                    }
                }
            }
        }

        void GestureController_GestureRecognized(object sender, GestureEventArgs e) {
            // Display the gesture type.
            tblGestures.Text = e.Name;

            // Do something according to the type of the gesture.

            if (e.Name.Equals("SwipeLeft")) {

                mouse_event(MOUSEEVENTF_WHEEL, 0, 0, -100, 0);
                // SetCursorPos(windowWidth / 2, windowHeight / 2);
                // mouse_event(MOUSEEVENTF_LEFTDOWN + MOUSEEVENTF_ABSOLUTE, windowWidth / 2, windowHeight / 2, 0, 0);
                tblHeights.Text = "ScrollLeft";
            }

            if (e.Name.Equals("SwipeRight")) {
                mouse_event(MOUSEEVENTF_WHEEL, 0, 0, 100, 0);
                //SetCursorPos(windowWidth / 2, windowHeight / 2);
                //mouse_event(MOUSEEVENTF_MIDDLEDOWN + MOUSEEVENTF_ABSOLUTE, windowWidth / 2, windowHeight / 2, 0, 0);

            }
           
        }

        private void ExtractEvent(Skeleton skeleton) {
            if (skeleton.Joints[JointType.HandRight].Position.Y > skeleton.Joints[JointType.HipRight].Position.Y) {
               // if (_status == Status.InActive  ) {
                if (skeleton.Joints[JointType.HandLeft].Position.Y > skeleton.Joints[JointType.HandRight].Position.Y) {
                    keybd_event(17, 0, 0, 0);
                    keybd_event(80, 0, 0, 0);
                    keybd_event(80, 0, KEYEVENTF_KEYUP, 0);
                    keybd_event(17, 0, KEYEVENTF_KEYUP, 0);
                    int xpos = (int)((1 + skeleton.Joints[JointType.HandRight].Position.X) * windowWidth / 2);
                    int ypos = (int)((1 - skeleton.Joints[JointType.HandRight].Position.Y) * windowHeight / 2);
                    SetCursorPos(xpos, ypos);
                    mouse_event(MOUSEEVENTF_LEFTDOWN + MOUSEEVENTF_ABSOLUTE, windowWidth / 2, windowHeight / 2, 0, 0);
                    mouse_event(MOUSEEVENTF_LEFTUP + MOUSEEVENTF_ABSOLUTE, windowWidth / 2, windowHeight / 2, 0, 0);
                   _status = Status.Draw;
                }
            }

            if (skeleton.Joints[JointType.HandRight].Position.Y < skeleton.Joints[JointType.HipRight].Position.Y) {
                if (_status == Status.Draw) {
                    keybd_event(17, 0, 0, 0);
                    keybd_event(85, 0, 0, 0);
                    keybd_event(85, 0, KEYEVENTF_KEYUP, 0);
                    keybd_event(17, 0, KEYEVENTF_KEYUP, 0);
                    //mouse_event(MOUSEEVENTF_LEFTDOWN + MOUSEEVENTF_ABSOLUTE, windowWidth / 2, windowHeight / 2, 0, 0);
                    //mouse_event(MOUSEEVENTF_LEFTUP + MOUSEEVENTF_ABSOLUTE, windowWidth / 2, windowHeight / 2, 0, 0);
                   // mouse_event(MOUSEEVENTF_LEFTDOWN + MOUSEEVENTF_ABSOLUTE, windowWidth / 2, windowHeight / 2, 0, 0);
                    _status = Status.InActive;
                   
                }
            }

            /*if (skeleton.Joints[JointType.HandLeft].Position.Y > skeleton.Joints[JointType.HipLeft].Position.Y)
             {
                     if(_status == Status.InActive) {
                         SetCursorPos(windowWidth / 2, windowHeight / 2);
                        // mouse_event(MOUSEEVENTF_MIDDLEDOWN + MOUSEEVENTF_ABSOLUTE, windowWidth / 2, windowHeight / 2, 0, 0);
                         _status = Status.Scroll;
                    }
             }

             if (skeleton.Joints[JointType.HandLeft].Position.Y < skeleton.Joints[JointType.HipLeft].Position.Y)
             {
                 if (_status == Status.Scroll)
                 {
                      mouse_event(MOUSEEVENTF_WHEEL, 0, 0, -100, 0);
                      _status=Status.InActive;
                 }
             }
             */

            //Console.WriteLine(skeleton.Joints[JointType.HandRight].Position.Y);
            //tblHeights.Text = string.Format("{0}", skeleton.Joints[JointType.HandRight].Position.Y);
            /*switch (_status) { 
                case Status.Draw:
                    int xpos = (int)((1 + skeleton.Joints[JointType.HandRight].Position.X) * windowWidth / 2);
                    int ypos = (int)((1 - skeleton.Joints[JointType.HandRight].Position.Y) * windowHeight / 2);
                    SetCursorPos(xpos, ypos);
                    //mouse_event(MOUSEEVENTF_LEFTDOWN + MOUSEEVENTF_ABSOLUTE,xpos,ypos, 0, 0);                  
                    break;
                case Status.Scroll:
                    int xpos2 = (int)(skeleton.Joints[JointType.HandLeft].Position.X * windowWidth);
                    int ypos2 = (int)((1 - skeleton.Joints[JointType.HandLeft].Position.Y) * windowHeight);
                    SetCursorPos(xpos2, ypos2);
                    break;
                 
            }*/
        }

        private void Color_Click(object sender, RoutedEventArgs e) {
            _mode = Mode.Color;
        }

        private void Depth_Click(object sender, RoutedEventArgs e) {
            _mode = Mode.Depth;
        }


        [DllImport("user32.dll")]
        public static extern bool SetCursorPos(int x, int y);

        [DllImport("user32.dll", EntryPoint = "mouse_event")]
        public static extern void mouse_event(int dwFlags, int dx, int dy, int cButton, int dwExtraInfo);

        const int MOUSEEVENTF_MOVE = 0x0001;
        const int MOUSEEVENTF_LEFTDOWN = 0x0002;
        const int MOUSEEVENTF_LEFTUP = 0x0004;
        const int MOUSEEVENTF_RIGHTDOWN = 0x0008;
        const int MOUSEEVENTF_RIGHTUP = 0x0010;
        const int MOUSEEVENTF_MIDDLEDOWN = 0x0020;
        const int MOUSEEVENTF_MIDDLEUP = 0x0040;
        const int MOUSEEVENTF_ABSOLUTE = 0x8000;
        const int MOUSEEVENTF_WHEEL = 0x0800;

        [DllImport("user32.dll")]
        public static extern void keybd_event(Byte bVk, Byte bScan, Int32 dwFlags, Int32 dwExtraInfo);

        const int KEYEVENTF_KEYUP = 0x0002;
    }


}
