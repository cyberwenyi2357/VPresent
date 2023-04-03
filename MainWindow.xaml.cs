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
using System.Windows.Controls;
using Microsoft.Kinect;
using NUI3D;
using Microsoft.Kinect.VisualGestureBuilder;
namespace T6_SkeletonTracking
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private VisualGestureBuilderFrameSource vgbFrameSource;
        private VisualGestureBuilderDatabase vgbDb;
        private VisualGestureBuilderFrameReader vgbFrameReader;
        private KinectSensor sensor;
        
        private Body[] bodies;

        private DrawingGroup drawingGroup;
        private DrawingImage drawingImg;
        private double drawingImgWidth = 1920,  drawingImgHeight = 1080;
        private Boolean isTouchedLeft = false;
        private Boolean isTouchedRight = false;
        private Boolean isBoth = false;
        private Boolean isGood = false;
        private Boolean isPointing = false;
        // class exercise 2
        private JointType[] bones = {
                                // Torso
                    JointType.Head, JointType.Neck,
                    JointType.Neck, JointType.SpineShoulder,
                    JointType.SpineShoulder, JointType.SpineMid,
                    JointType.SpineMid, JointType.SpineBase,
                    JointType.SpineShoulder, JointType.ShoulderRight ,
                    JointType.SpineShoulder, JointType.ShoulderLeft,
                    JointType.SpineBase, JointType.HipRight,
                    JointType.SpineBase, JointType.HipLeft,

                    // Right Arm
                    JointType.ShoulderRight, JointType.ElbowRight,
                    JointType.ElbowRight, JointType.WristRight,
                    JointType.WristRight, JointType.HandRight,
                    JointType.HandRight, JointType.HandTipRight,
                    JointType.WristRight, JointType.ThumbRight,

                    // Left Arm
                    JointType.ShoulderLeft, JointType.ElbowLeft,
                    JointType.ElbowLeft, JointType.WristLeft,
                    JointType.WristLeft, JointType.HandLeft,
                    JointType.HandLeft, JointType.HandTipLeft,
                    JointType.WristLeft, JointType.ThumbLeft,

                    // Right Leg
                    JointType.HipRight, JointType.KneeRight,
                    JointType.KneeRight, JointType.AnkleRight,
                    JointType.AnkleRight, JointType.FootRight,
                
                    // Left Leg
                    JointType.HipLeft, JointType.KneeLeft,
                    JointType.KneeLeft, JointType.AnkleLeft,
                    JointType.AnkleLeft, JointType.FootLeft,
        };

        private JointType[] bones_crazy = {
                    // head & spine 
                    JointType.Head, JointType.Neck,
                    JointType.Neck, JointType.SpineShoulder,
                    JointType.SpineMid, JointType.SpineBase,
                    JointType.SpineBase, JointType.HipRight,
                    JointType.SpineBase, JointType.HipLeft, 

                    // right wing 
                    JointType.SpineShoulder, JointType.HandRight,
                    JointType.HandRight, JointType.HipRight,

                    // left wing 
                    JointType.SpineShoulder, JointType.HandLeft,
                    JointType.HandLeft, JointType.HipLeft,

                    // Right Leg
                    JointType.HipRight, JointType.KneeRight,
                    JointType.KneeRight, JointType.AnkleRight,
                    JointType.AnkleRight, JointType.FootRight,
                
                    // Left Leg
                    JointType.HipLeft, JointType.KneeLeft,
                    JointType.KneeLeft, JointType.AnkleLeft,
                    JointType.AnkleLeft, JointType.FootLeft,
        };

        private Rect r = new Rect(98, 54, 210, 150);
        private Random rand = new Random();
        private Rect r1 = new Rect(-50, -50, 30, 30);
        private void MovingImageReset()
        { // to be used later
            //r.Y = -r.Height;
            //r.X = rand.Next((int)r.Width, (int)(drawingImgWidth - r.Width));
            //rectangle1.Fill = new SolidColorBrush(Color.FromRgb(0, 111, 0));
            isTouchedLeft = true;
        }

        private void MovingImage(DrawingContext dc)
        {
            dc.DrawImage(illustration1.Source, r);
            // Exercise 5

            //if (r.Y > drawingImgHeight) MovingImageReset();
        }
        private void Point(DrawingContext dc){
            dc.DrawImage(pointer.Source, r1);
        }

        private void DrawingGroupInit() // called in Window_Loaded 
        {
            drawingGroup = new DrawingGroup();
            drawingImg = new DrawingImage(drawingGroup);
            skeletonImg.Source = drawingImg;

            // prevent drawing outside of our render area
            drawingGroup.ClipGeometry = new RectangleGeometry(
                                        new Rect(0.0, 0.0,
            drawingImgWidth, drawingImgHeight));
        }

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            System.Console.WriteLine("window loaded");

            sensor = KinectSensor.GetDefault(); // get the default Kinect sensor 

            BodyFrameReaderInit();

            sensor.Open();

            DrawingGroupInit();

            // Put the following code at the end of Window_Loaded 
            //DrawingGroup drawingGroup = new DrawingGroup();
            //DrawingImage drawingImg = new DrawingImage(drawingGroup);
            //skeletonImg.Source = drawingImg; // skeletonImg is Image control in .xaml 
            //                                 // drawing outside the clip geometry will be ignored 
            //drawingGroup.ClipGeometry = new RectangleGeometry(
            //                new Rect(0.0, 0.0, 640, 480));

            //using (DrawingContext dc = drawingGroup.Open())
            //{
            //    // draw a transparent background to set the drawImg size
            //    dc.DrawRectangle(Brushes.Transparent, null,
            //            new Rect(0.0, 0.0, 640, 480));
            //    dc.DrawLine(new Pen(Brushes.Red, 5),
            //            new Point(0, 0), new Point(200, 100));
            //}

            // exercise 6
            NUI3D.ColorFrameManager colorFrameManager = new NUI3D.ColorFrameManager();
            colorFrameManager.Init(sensor, colorImg);
        }

        private void BodyFrameReaderInit()
        {
            BodyFrameReader bodyFrameReader = sensor.BodyFrameSource.OpenReader();
            bodyFrameReader.FrameArrived += BodyFrameReader_FrameArrived;

            // BodyCount: maximum number of bodies that can be tracked at one time
            bodies = new Body[sensor.BodyFrameSource.BodyCount];
        }

        private void BodyFrameReader_FrameArrived(object sender, BodyFrameArrivedEventArgs e)
        {
            using (BodyFrame bodyFrame = e.FrameReference.AcquireFrame())
            {
                if (bodyFrame == null) return;

                bodyFrame.GetAndRefreshBodyData(bodies);

                String s = "no skeleton data";

                // class exercise 1
                int numOfUsers = 0;

                using (DrawingContext dc = drawingGroup.Open())
                {
                    // draw a transparent background to set the render size
                    dc.DrawRectangle(Brushes.Transparent, null,
                            new Rect(0.0, 0.0, drawingImgWidth, drawingImgHeight));

                    for (int i = 0; i < bodies.Length; i++)
                        if (bodies[i].IsTracked)
                        {
                            numOfUsers++;

                            CameraSpacePoint headPosi = bodies[i].Joints[JointType.Head].Position;
                            s = String.Format("Head: X: {0:0.0} Y: {1:0.0} Z: {2:0.0}",
                                                        headPosi.X, headPosi.Y, headPosi.Z); 

                            
                            SkeletonBasedInteractivity(bodies[i], dc);
                            DrawSkeleton(bodies[i], dc);
                            Interactivity(bodies[i], dc);
                        }
                    
                    Point(dc);
                    
                }

                statusTxt.Text = "#User: " + numOfUsers + "; " + s;
            }
        }

        private ColorSpacePoint MapCameraPointToColorSpace(Body body, JointType jointType)
        {
            return sensor.CoordinateMapper.MapCameraPointToColorSpace(
                body.Joints[jointType].Position);
        }

        private void DrawSkeleton(Body body, DrawingContext dc)
        {
            // DrawBone(body, dc, JointType.Head, JointType.SpineBase);

            // Exercise 2: draw a complete skeleton 
            for (int i = 0; i < bones.Length; i += 2)
            // for (int i = 0; i < bones_crazy.Length; i += 2)
            {
                DrawBone(body, dc, bones[i], bones[i + 1]);
                // DrawBone(body, dc, bones_crazy[i], bones_crazy[i+1]);
            }

            // Exercise 3
            foreach (JointType jt in body.Joints.Keys) {
                ColorSpacePoint pt = MapCameraPointToColorSpace(body, jt);
                dc.DrawEllipse(Brushes.Blue, null, new Point(pt.X, pt.Y), 10, 10);
            }

            RenderClippedEdges(body, dc);
            MovingImage(dc);
            VisualizeHandState(body, dc, JointType.HandLeft, body.HandLeftState);
            VisualizeHandState(body, dc, JointType.HandRight, body.HandRightState);
            
        }

        private void DrawBone(Body body, DrawingContext dc,
                JointType j0, JointType j1)
        {
            ColorSpacePoint pt0 = MapCameraPointToColorSpace(body, j0);
            ColorSpacePoint pt1 = MapCameraPointToColorSpace(body, j1);

            dc.DrawLine(new Pen(Brushes.Red, 5),
                            new Point(pt0.X, pt0.Y), new Point(pt1.X, pt1.Y));
        }

        private void RenderClippedEdges(Body body, DrawingContext drawingContext)
        {
            double clipBoundsThickness = 10;

            if (body.ClippedEdges.HasFlag(FrameEdges.Bottom))
            {
                drawingContext.DrawRectangle(
                    Brushes.Red, null,
                    new Rect(0, drawingImgHeight - clipBoundsThickness, drawingImgWidth, clipBoundsThickness));
            }

            if (body.ClippedEdges.HasFlag(FrameEdges.Top))
            {
                drawingContext.DrawRectangle(
                    Brushes.Red, null,
                    new Rect(0, 0, drawingImgWidth, clipBoundsThickness));
            }

            if (body.ClippedEdges.HasFlag(FrameEdges.Left))
            {
                drawingContext.DrawRectangle(
                    Brushes.Red, null,
                    new Rect(0, 0, clipBoundsThickness, drawingImgHeight));
            }

            if (body.ClippedEdges.HasFlag(FrameEdges.Right))
            {
                drawingContext.DrawRectangle(
                    Brushes.Red, null,
                    new Rect(drawingImgWidth - clipBoundsThickness, 0, clipBoundsThickness, drawingImgHeight));
            }
        }

        
        public void Interactivity(Body body, DrawingContext dc){
            ColorSpacePoint leftHand =
                            MapCameraPointToColorSpace(body, JointType.HandLeft);
            ColorSpacePoint rightHand =
                MapCameraPointToColorSpace(body, JointType.HandRight);

        }
 
    // Class Exercise 4
    private void SkeletonBasedInteractivity(Body body, DrawingContext dc)
        {
            // exercise 4
            ColorSpacePoint leftHand =
                MapCameraPointToColorSpace(body, JointType.HandLeft);
            ColorSpacePoint rightHand =
                MapCameraPointToColorSpace(body, JointType.HandRight);
            ColorSpacePoint leftTip =
                            MapCameraPointToColorSpace(body, JointType.HandTipLeft);
            ColorSpacePoint rightTip =
                MapCameraPointToColorSpace(body, JointType.HandTipRight);
            ColorSpacePoint leftThumb =
                                       MapCameraPointToColorSpace(body, JointType.ThumbLeft);
            ColorSpacePoint rightThumb =
                MapCameraPointToColorSpace(body, JointType.ThumbRight);
            // hit test

            // exercise 7
            if (r.Contains(leftHand.X, leftHand.Y)) // lasso hand gesture 
            {
                //rectangle1.Fill = new SolidColorBrush(Color.FromRgb(0, 111, 0));
                isTouchedLeft = true;
            }
            if(r.X-leftHand.X>60 || r.Y-leftHand.Y>60){
                isTouchedLeft = false;
            }
            if (isTouchedLeft == true)// hit test & hand gestures 
            {
                
                if(body.HandLeftState == HandState.Closed){
                    r.X = leftHand.X - r.Width / 2; // move
                    r.Y = leftHand.Y - r.Height / 2;
                }
            }
            if (r.Contains(rightHand.X, rightHand.Y)) // lasso hand gesture 
            {
                //rectangle1.Fill = new SolidColorBrush(Color.FromRgb(0, 111, 0));
                isTouchedRight = true;
            }
            if (r.X - rightHand.X > 60 || r.Y - rightHand.Y > 60)
            {
                isTouchedRight = false;
            }
            if (isTouchedRight == true)// hit test & hand gestures 
            {

                if (body.HandRightState == HandState.Closed)
                {
                    r.X = rightHand.X - r.Width / 2; // move
                    r.Y = rightHand.Y - r.Height / 2;
                    dc.DrawRectangle(Brushes.Yellow, null, new Rect(r.X-8, r.Y-6, r.Width*1.05, r.Height*1.1));
                }
            }
            if ((r.Contains(rightHand.X, rightHand.Y) || r.Contains(rightThumb.X, rightThumb.Y) || r.Contains(rightTip.X, rightTip.Y)) && (r.Contains(leftHand.X, leftHand.Y)|| r.Contains(leftThumb.X, leftThumb.Y)  || r.Contains(leftTip.X,leftTip.Y)))
            {
                isBoth = true;
            }
            if ((Math.Abs(r.X - rightHand.X) > 80 && Math.Abs(r.Y - rightHand.Y) > 80) && (Math.Abs(r.Y - leftHand.Y) > 80 && Math.Abs(r.Y - leftHand.Y) > 80))
            {
                isBoth = false;
            }
            if (isBoth == true)
            {
                if(Math.Abs(rightHand.X - leftHand.X)>150){
                    r.Width = Math.Abs(rightHand.X - leftHand.X);
                    r.Height = r.Width / 1.83;
                    dc.DrawRectangle(Brushes.Green, null, new Rect(r.X - 8, r.Y - 6, r.Width * 1.05, r.Height * 1.1));
                }
                
            }
            
        }
        private void VisualizeHandState(Body body, DrawingContext dc, JointType jointType, HandState handState)
        {
            SolidColorBrush red = new SolidColorBrush(Color.FromArgb(100, 255, 0, 0));
            SolidColorBrush green = new SolidColorBrush(Color.FromArgb(100, 0, 255, 0));
            SolidColorBrush yellow = new SolidColorBrush(Color.FromArgb(60, 0, 0, 255));
            double radius = 30;
            ColorSpacePoint hand_pt = MapCameraPointToColorSpace(body, jointType);
            switch (handState)
            {
                case HandState.Closed:
                    isPointing = false;
                    break;
                case HandState.Open:
                    isPointing = false;
                    break;
                case HandState.Lasso:
                    isPointing = true;
                    break;
            }
            if(isPointing==true){
                dc.DrawEllipse(yellow, null, new Point(hand_pt.X, hand_pt.Y), radius, radius);
            }
            
        }
    }
}
