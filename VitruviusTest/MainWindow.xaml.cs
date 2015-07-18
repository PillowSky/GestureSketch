using LightBuzz.Vitruvius;
using LightBuzz.Vitruvius.WPF;
using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
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

        public enum Status {
            InActive,
            Line,
            Circle,
            Rectangle,
        }

        static readonly string TAG = "LightBuzz.Vitruvius";

        Status leftStatus = Status.InActive;
        Status rightStatus = Status.InActive;

        //GestureController _gestureController;
        SolidColorBrush circleColor = new SolidColorBrush(Colors.Wheat);
        SolidColorBrush lineColor = new SolidColorBrush(Colors.Violet);
        SolidColorBrush rectangleColor = new SolidColorBrush(Colors.Tomato);

        const float YThreshold = 0.2f;
        const float ZThreshold = 0.1f;
        const float DistanceThreshold = 0.1f;
        
        int windowWidth = 1366;
        int windowHeight = 768;
        int canvasWidth = 1366;
        int canvasHeight = 768;

        Timer timer;
        bool shouldClean = false;
        //Debug debug;

        public MainWindow() {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            KinectSensor sensor = SensorExtensions.Default();

            if (sensor != null) {
                //sensor.ColorStream.Enable();
                sensor.SkeletonStream.Enable();
                //sensor.EnableAllStreams();
                //sensor.ColorFrameReady += Sensor_ColorFrameReady;
                //sensor.DepthFrameReady += Sensor_DepthFrameReady;
                sensor.SkeletonFrameReady += Sensor_SkeletonFrameReady;

                //_gestureController = new GestureController(GestureType.All);
                //_gestureController.GestureRecognized += GestureController_GestureRecognized;

                sensor.Start();
            }
            //debug = new Debug();
            //debug.Show();

            timer = new Timer();
            timer.Interval = 60000;
            timer.Elapsed += onTimerElapsed;
            timer.Start();

            drawTip();
        }

        void Sensor_ColorFrameReady(object sender, ColorImageFrameReadyEventArgs e) {
            using (var frame = e.OpenColorImageFrame()) {
                if (frame != null) {
                    camera.Source = frame.ToBitmap();
                }
            }
        }

        void Sensor_SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e) {
            using (var frame = e.OpenSkeletonFrame()) {
                if (frame != null) {
                    if (shouldClean) {
                        canvas.Children.Clear();
                        leftStatus = Status.InActive;
                        rightStatus = Status.InActive;
                        drawTip();
                        shouldClean = false;
                    } else {
                        canvas.ClearSkeletons();
                    }

                    var skeletons = frame.Skeletons().Where(s => s.TrackingState == SkeletonTrackingState.Tracked).OrderBy(s => s.Joints[JointType.Head].Position.X);
                    foreach (var skeleton in skeletons) {
                        if (skeleton != null) {
                            // Update skeleton gestures.
                            //_gestureController.Update(skeleton);

                            // Draw skeleton.
                            canvas.DrawSkeleton(skeleton);

                            // Display user height.
                            //tblHeights.Text += string.Format("\nUser {0}: {1}cm", skeleton.TrackingId, skeleton.Height());
                        }
                    }
                    // Extract
                    DrawSketch(skeletons);
                }
            }
        }

        void GestureController_GestureRecognized(object sender, GestureEventArgs e) {
            /*
            // Display the gesture type.
            tblGestures.Text = e.Name;

            // Do something according to the type of the gesture.

            if (e.Name.Equals("SwipeLeft")) {
                if (_status == Status.InActive) {
                    SetCursorPos(windowWidth / 2, windowHeight / 2);
                    mouse_event(MOUSEEVENTF_LEFTDOWN + MOUSEEVENTF_ABSOLUTE, windowWidth / 2, windowHeight / 2, 0, 0);
                } else {
                    if (_status == Status.Translate) {
                        mouse_event(MOUSEEVENTF_MIDDLEUP, 0, 0, 0, 0);
                        SetCursorPos(windowWidth / 2, windowHeight / 2);
                        mouse_event(MOUSEEVENTF_LEFTDOWN + MOUSEEVENTF_ABSOLUTE, windowWidth / 2, windowHeight / 2, 0, 0);
                    }
                }

                _status = Status.Rotate;
                //tblHeights.Text = "Rotate";
            }

            if (e.Name.Equals("SwipeRight")) {
                if (_status == Status.InActive) {
                    SetCursorPos(windowWidth / 2, windowHeight / 2);
                    mouse_event(MOUSEEVENTF_MIDDLEDOWN + MOUSEEVENTF_ABSOLUTE, windowWidth / 2, windowHeight / 2, 0, 0);
                } else {
                    if (_status == Status.Rotate) {
                        mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
                        SetCursorPos(windowWidth / 2, windowHeight / 2);
                        mouse_event(MOUSEEVENTF_MIDDLEDOWN + MOUSEEVENTF_ABSOLUTE, windowWidth / 2, windowHeight / 2, 0, 0);
                    }
                }

                _status = Status.Translate;
                //tblHeights.Text = "Translate";
            }

            if (e.Name.Equals("JoinedHands") || e.Name.Equals("ZoomIn") || e.Name.Equals("ZoomOut")) {
                switch (_status) {
                    case Status.Rotate:
                        mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
                        break;
                    case Status.Translate:
                        mouse_event(MOUSEEVENTF_MIDDLEUP, 0, 0, 0, 0);
                        break;
                }
                _status = Status.InActive;
            }
            */
        }

        private void ExtractEvent(Skeleton skeleton) {
            /*
            if (skeleton.Joints[JointType.HandRight].Position.Y > skeleton.Joints[JointType.HipRight].Position.Y) {
                if (_status == Status.InActive) {
                    SetCursorPos(windowWidth / 2, windowHeight / 2);
                    mouse_event(MOUSEEVENTF_LEFTDOWN + MOUSEEVENTF_ABSOLUTE, windowWidth / 2, windowHeight / 2, 0, 0);
                    _status = Status.Rotate;
                }
            }

            if (skeleton.Joints[JointType.HandRight].Position.Y < skeleton.Joints[JointType.HipRight].Position.Y) {
                if (_status == Status.Rotate) {
                    mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
                    _status = Status.InActive;
                }
            }

            if (skeleton.Joints[JointType.HandLeft].Position.Y > skeleton.Joints[JointType.HipLeft].Position.Y) {
                if (_status == Status.InActive) {
                    SetCursorPos(windowWidth / 2, windowHeight / 2);
                    mouse_event(MOUSEEVENTF_MIDDLEDOWN + MOUSEEVENTF_ABSOLUTE, windowWidth / 2, windowHeight / 2, 0, 0);
                    _status = Status.Translate;
                }
            }

            if (skeleton.Joints[JointType.HandLeft].Position.Y < skeleton.Joints[JointType.HipLeft].Position.Y) {
                if (_status == Status.Translate) {
                    mouse_event(MOUSEEVENTF_MIDDLEUP, 0, 0, 0, 0);
                    _status = Status.InActive;
                }
            }

            switch (_status) {
                case Status.Rotate:
                    int xpos = (int)(skeleton.Joints[JointType.HandRight].Position.X * windowWidth);
                    int ypos = (int)((1 - skeleton.Joints[JointType.HandRight].Position.Y) * windowHeight);
                    SetCursorPos(xpos, ypos);
                    break;
                case Status.Translate:
                    int xpos2 = (int)(skeleton.Joints[JointType.HandLeft].Position.X * windowWidth);
                    int ypos2 = (int)((1 - skeleton.Joints[JointType.HandLeft].Position.Y) * windowHeight);
                    SetCursorPos(xpos2, ypos2);
                    break;
            }*/
        }

        private void DrawSketch(IOrderedEnumerable<Skeleton> skeletons) {
            Skeleton left;
            Skeleton right;

            //debug.count.Text = string.Format("{0}", skeletons.Count());
            switch (skeletons.Count()) {
                case 0:
                    return;
                case 1:
                    left = skeletons.ElementAt(0);
                    if (leftStatus == Status.Circle) {
                        DrawCircleLeft(left);
                    }
                    if (leftStatus == Status.InActive) {
                        if (left.Joints[JointType.HandLeft].Position.X > left.Joints[JointType.HandRight].Position.X) {
                            leftStatus = Status.Circle;
                            //debug.status.Text = "Circle";
                        }
                    }
                    rightStatus = Status.InActive;
                    break;
                case 2:
                    left = skeletons.ElementAt(0);
                    right = skeletons.ElementAt(1);

                    //debug.left.Text = string.Format("({0}, {1})", left.Joints[JointType.FootRight].Position.X, left.Joints[JointType.FootRight].Position.Y);
                    //debug.right.Text = string.Format("({0}, {1})", right.Joints[JointType.FootLeft].Position.X, right.Joints[JointType.FootLeft].Position.Y);
                    //debug.rectangle.Text = string.Format("{0}", (Math.Pow(left.Joints[JointType.FootRight].Position.X - right.Joints[JointType.FootLeft].Position.X, 2) + Math.Pow(left.Joints[JointType.FootRight].Position.X - right.Joints[JointType.FootLeft].Position.Y, 2)));
                    if (leftStatus == Status.Line && rightStatus == Status.Line) {
                        DrawLine(left, right);
                    } else {
                        if (leftStatus == Status.Rectangle && rightStatus == Status.Rectangle) {
                            DrawRectangle(left, right);
                        } else {
                            if (leftStatus == Status.Circle) {
                                DrawCircleLeft(left);
                            }
                            if (rightStatus == Status.Circle) {
                                DrawCircleRight(right);
                            }
                            if (leftStatus == Status.InActive) {
                                if (left.Joints[JointType.HandLeft].Position.X > left.Joints[JointType.HandRight].Position.X) {
                                    leftStatus = Status.Circle;
                                    //debug.status.Text = "LeftCircle";
                                }
                            }
                            if (rightStatus == Status.InActive) {
                                if (right.Joints[JointType.HandLeft].Position.X > right.Joints[JointType.HandRight].Position.X) {
                                    rightStatus = Status.Circle;
                                    //debug.status.Text = "RightCircle";
                                }
                            }
                            if (leftStatus == Status.InActive && rightStatus == Status.InActive) {
                                if (left.Joints[JointType.HandRight].Position.X > right.Joints[JointType.HandLeft].Position.X) {
                                    leftStatus = Status.Line;
                                    rightStatus = Status.Line;
                                    //debug.status.Text = "Line";
                                } else {
                                    if (left.Joints[JointType.FootRight].Position.X > right.Joints[JointType.FootLeft].Position.X) {
                                        leftStatus = Status.Rectangle;
                                        rightStatus = Status.Rectangle;
                                        //debug.status.Text = "Rectangle";
                                    }
                                }
                            }
                        }
                    }
                    break;
            }
        }

        private void DrawCircleLeft(Skeleton left) {
            int leftX = (int)((left.Joints[JointType.HandLeft].Position.X + 1) * canvasWidth / 2);
            int leftY = (int)((1 - left.Joints[JointType.HandLeft].Position.Y) * canvasHeight / 2);
            int rightX = (int)((left.Joints[JointType.HandRight].Position.X + 1) * canvasWidth / 2);
            int rightY = (int)((1 - left.Joints[JointType.HandRight].Position.Y) * canvasHeight / 2);

            int centerX = (leftX + rightX) / 2;
            int centerY = (leftY + rightY) / 2;
            int radius = (int) Math.Sqrt(Math.Pow(leftX - centerX, 2) + Math.Pow(leftY - centerY, 2));

            Ellipse circle = new Ellipse {
                Width = radius * 2,
                Height = radius * 2,
                Stroke = circleColor,
                StrokeThickness = 4
            };

            if (Math.Abs(left.Joints[JointType.FootLeft].Position.Y - left.Joints[JointType.FootRight].Position.Y) > YThreshold) {
                leftStatus = Status.InActive;
            } else {
                circle.Tag = TAG;
            }

            Canvas.SetLeft(circle, centerX - radius);
            Canvas.SetTop(circle, centerY - radius);
            canvas.Children.Add(circle);
        }

        private void DrawCircleRight(Skeleton right) {
            int leftX = (int)((right.Joints[JointType.HandLeft].Position.X + 1) * canvasWidth / 2);
            int leftY = (int)((1 - right.Joints[JointType.HandLeft].Position.Y) * canvasHeight / 2);
            int rightX = (int)((right.Joints[JointType.HandRight].Position.X + 1) * canvasWidth / 2);
            int rightY = (int)((1 - right.Joints[JointType.HandRight].Position.Y) * canvasHeight / 2);

            int centerX = (leftX + rightX) / 2;
            int centerY = (leftY + rightY) / 2;
            int radius = (int) Math.Sqrt(Math.Pow(leftX - centerX, 2) + Math.Pow(leftY - centerY, 2));

            Ellipse circle = new Ellipse {
                Width = radius * 2,
                Height = radius * 2,
                Stroke = circleColor,
                StrokeThickness = 4
            };

            if (Math.Abs(right.Joints[JointType.FootLeft].Position.Y - right.Joints[JointType.FootRight].Position.Y) > YThreshold) {
                rightStatus = Status.InActive;
            } else {
                circle.Tag = TAG;
            }

            Canvas.SetLeft(circle, centerX - radius);
            Canvas.SetTop(circle, centerY - radius);
            canvas.Children.Add(circle);
        }

        private void DrawLine(Skeleton left, Skeleton right) {
            int leftX = (int)((left.Joints[JointType.HandRight].Position.X + 1) * canvasWidth / 2);
            int leftY = (int)((1 - left.Joints[JointType.HandRight].Position.Y) * canvasHeight / 2);
            int rightX = (int)((right.Joints[JointType.HandLeft].Position.X + 1) * canvasWidth / 2);
            int rightY = (int)((1 - right.Joints[JointType.HandLeft].Position.Y) * canvasHeight / 2);

            Line line = new Line {
                X1 = leftX,
                Y1 = leftY,
                X2 = rightX,
                Y2 = rightY,
                Stroke = lineColor,
                StrokeThickness = 4,
            };

            if ((Math.Abs(left.Joints[JointType.FootLeft].Position.Y - left.Joints[JointType.FootRight].Position.Y) > YThreshold) && (Math.Abs(right.Joints[JointType.FootLeft].Position.Y - right.Joints[JointType.FootRight].Position.Y) > YThreshold)) {
                leftStatus = rightStatus = Status.InActive;
            } else {
                line.Tag = TAG;
            }

            canvas.Children.Add(line);
        }

        private void DrawRectangle(Skeleton left, Skeleton right) {
            int leftX = (int)((left.Joints[JointType.HandRight].Position.X + 1) * canvasWidth / 2);
            int leftY = (int)((1 - left.Joints[JointType.HandRight].Position.Y) * canvasHeight / 2);
            int rightX = (int)((right.Joints[JointType.HandLeft].Position.X + 1) * canvasWidth / 2);
            int rightY = (int)((1 - right.Joints[JointType.HandLeft].Position.Y) * canvasHeight / 2);

            Rectangle rect = new Rectangle {
                Width = Math.Abs(rightX - leftX),
                Height = Math.Abs(rightY - leftY),
                Stroke = rectangleColor,
                StrokeThickness = 4,
            };

            if ((Math.Abs(left.Joints[JointType.FootLeft].Position.Y - left.Joints[JointType.FootRight].Position.Y) > YThreshold) && (Math.Abs(right.Joints[JointType.FootLeft].Position.Y - right.Joints[JointType.FootRight].Position.Y) > YThreshold)) {
                leftStatus = rightStatus = Status.InActive;
            } else {
                rect.Tag = TAG;
            }

            Canvas.SetLeft(rect, Math.Min(leftX, rightX));
            Canvas.SetTop(rect, Math.Min(leftY, rightY));
            canvas.Children.Add(rect);
        }
        private void Color_Click(object sender, RoutedEventArgs e) {
            //_mode = Mode.Color;
        }

        private void Depth_Click(object sender, RoutedEventArgs e) {
            //_mode = Mode.Depth;
        }

        private void OnKeyDownHandler(object sender, KeyEventArgs e) {
            if (e.Key == Key.C) {
                canvas.Children.Clear();
                leftStatus = Status.InActive;
                rightStatus = Status.InActive;
                drawTip();
            }
        }


        void onTimerElapsed(object sender, ElapsedEventArgs e) {
            shouldClean = true;
        }

        void drawTip() {
            Line line = new Line {
                X1 = 100,
                Y1 = 100,
                X2 = 200,
                Y2 = 100,
                Stroke = lineColor,
                StrokeThickness = 4,
            };
            canvas.Children.Add(line);

            Ellipse circle = new Ellipse {
                Width = 100,
                Height = 100,
                Stroke = circleColor,
                StrokeThickness = 4
            };

            Canvas.SetLeft(circle, 100);
            Canvas.SetTop(circle, 200);
            canvas.Children.Add(circle);

            Rectangle rect = new Rectangle {
                Width = 100,
                Height = 100,
                Stroke = rectangleColor,
                StrokeThickness = 4,
            };

            Canvas.SetLeft(rect, 100);
            Canvas.SetTop(rect, 400);
            canvas.Children.Add(rect);
        }

        [DllImport("user32.dll")]
        public static extern bool SetCursorPos(int x, int y);

        [DllImport("user32.dll")]
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

    }

}
