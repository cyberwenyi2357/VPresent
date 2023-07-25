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
using System.Media;
using Microsoft.Kinect;
using NUI3D;
using Microsoft.Kinect.VisualGestureBuilder;
using Microsoft.Speech.AudioFormat;
using Microsoft.Speech.Recognition;



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
        private BodyFrameReader bodyFrameReader;
        private KinectSensor sensor;
        private Body[] bodies;
        private DrawingGroup drawingGroup;
        private DrawingImage drawingImg;
        private double drawingImgWidth = 1920, drawingImgHeight = 1080;
        private Boolean isTouchedRight = false;
        private Boolean[] isBoth = new Boolean[4];
        private Boolean isGood = false;
        private Boolean isPointing = false;
        private Boolean isClosed = false;
        
        private Boolean clear = false;
        private System.Windows.Media.Brush touming = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0, 255,255,255));
        SolidColorBrush blue = new SolidColorBrush(System.Windows.Media.Color.FromArgb(60, 0, 0, 255));
        SoundPlayer soundPlayer = new SoundPlayer("cleared.wav");
        private Boolean mapToColorSpace = true;
        private Pose[] poseLib;
        private int targetPoseIndex = 0;
        private Rect[] r = new Rect[4];
        private Rect[] c = new Rect[4];
        private Rect[] shadow = new Rect[4];
        private RecognizerInfo kinectRecognizerInfo;
        private SpeechRecognitionEngine recognizer;
        private Choices PPTChoices = new Choices();
        private String[] PPTNames = { "classification", "model fitting", "prediction", "clustering", "dimension reduction", "cross validation", "bias variance trade-off","model complexity score" };
        private Boolean[] isrecognized = new Boolean[4];
        private Boolean[] isTouchedLeft = new Boolean[4];
        private int count = 0;
        List<Point> list = new List<Point>();
        public MainWindow()
        {
            InitializeComponent();
        }

        private System.Windows.Point p1 = new System.Windows.Point(30 + 330 + 50, 320 - 250);
        private System.Windows.Point start = new System.Windows.Point(30 + 330 + 50, 320 - 250);
        private Boolean[] isTouchedLeft1 = new Boolean[4];
        private Boolean[] isBoth1 = new Boolean[4];
        private void BuildCommands() // call it in Window_Loaded()
        {
            PPTChoices.Add(PPTNames);
        }
        //The skeleton tracking part refers to lecture 06, the added condition is my own contribution
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
        
        //private Rect r = new Rect(98, 54, 220, 153);
        private Random rand = new Random();
        private Rect r1 = new Rect(60, 60, 20, 20);
        private void MovingImageReset()
        { 

        }
        private void DisplayRecognizedImage(DrawingContext dc)
        {
            if(isrecognized[0]){
                dc.DrawImage(classification.Source, c[0]);
            }
            if (isrecognized[1])
            {
                dc.DrawImage(modelFitting.Source, c[1]);
            }
            if (isrecognized[2])
            {
                dc.DrawImage(prediction.Source, c[2]);
            }
            if (isrecognized[3])
            {
                dc.DrawImage(crossValidation.Source, c[3]);
            }
        }
        //private void Point(DrawingContext dc){
        //    dc.DrawImage(pointer.Source, r1);
        //}
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

        
private void BuildGrammar(){
            GrammarBuilder grammarBuilder = new GrammarBuilder();
            grammarBuilder.Culture = kinectRecognizerInfo.Culture;
            grammarBuilder.Append(PPTChoices);
            Grammar grammar = new Grammar(grammarBuilder);
            recognizer.LoadGrammar(grammar);
        }
                 
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            System.Console.WriteLine("window loaded");
            sensor = KinectSensor.GetDefault(); // get the default Kinect sensor 
            sensor.Open();
            BodyFrameReaderInit();
            vgbFrameSource = new VisualGestureBuilderFrameSource(sensor, 0);

            // Create a gesture database using the pre-trained ones with VGB
            // @ to allow \ in the name 
            vgbDb = new VisualGestureBuilderDatabase(@".\Gestures\MyGestures.gbd");

            vgbFrameSource.AddGestures(vgbDb.AvailableGestures);

            vgbFrameReader = vgbFrameSource.OpenReader();
            vgbFrameReader.FrameArrived += VgbFrameReader_FrameArrived;

            bodyFrameReader = sensor.BodyFrameSource.OpenReader();
            bodyFrameReader.FrameArrived += BodyFrameReader_FrameArrived;
            DrawingGroupInit();
            

            NUI3D.ColorFrameManager colorFrameManager = new NUI3D.ColorFrameManager();
            colorFrameManager.Init(sensor, colorImg);
            
            AudioBeamFrameReader audioBeamFrameReader = sensor.AudioSource.OpenReader();
            audioBeamFrameReader.FrameArrived += AudioBeamFrameReader_FrameArrived;
            kinectRecognizerInfo = FindKinectRecognizerInfo();
            if (kinectRecognizerInfo != null)
            {
                recognizer = new SpeechRecognitionEngine(kinectRecognizerInfo);
            }
            BuildCommands();
            BuildGrammar();
            IReadOnlyList<AudioBeam> audioBeamList = sensor.AudioSource.AudioBeams;
            System.IO.Stream audioStream = audioBeamList[0].OpenInputStream();

            KinectAudioStream kinectAudioStream = new KinectAudioStream(audioStream);
            // let the convertStream know speech is going active
            kinectAudioStream.SpeechActive = true;

            this.recognizer.SetInputToAudioStream(
                kinectAudioStream, new SpeechAudioFormatInfo(EncodingFormat.Pcm, 16000, 16, 1, 32000, 2, null));
            recognizer.RecognizeAsync(RecognizeMode.Multiple);
            for (int i = 0; i < 4; i++)
            {
                isrecognized[i] = false;
                isTouchedLeft[i] = false;
            }
            recognizer.SpeechRecognized += Recognizer_SpeechRecognized;

            for (int i = 0; i < 4; i++)
            {
                r[i] = new Rect(98, 54 + 170 * i, 170, 143);
                shadow[i] = new Rect(98, 54 + 170 * i, 170, 143);
                c[i] = new Rect(rand.Next(250,650), 104 + 100 * rand.Next(0,7), 170, 143);
            }
            
        }
        private void VgbFrameReader_FrameArrived(object sender, VisualGestureBuilderFrameArrivedEventArgs e)
        {
            using (VisualGestureBuilderFrame vgbFrame = e.FrameReference.AcquireFrame())
            {
                if (vgbFrame == null)
                {
                    //recognitionResult.Text = "No gesture recognized";
                    return;
                }

                IReadOnlyDictionary<Gesture, DiscreteGestureResult> results =
                    vgbFrame.DiscreteGestureResults;
                if (results != null)
                {
                    Brush brush = Brushes.Black; // class exercise 
                    // Check if any of the gestures is recognized 
                    bool recognized = false;
                    foreach (Gesture gesture in results.Keys)
                    {
                        DiscreteGestureResult result = results[gesture];

                        if (result.Detected)
                        {
                            //using (DrawingContext dc = drawingGroup.Open())
                            //{
                            recognitionResult.Text =  "clear gesture recognized" ;
                            
                            recognized = true;
                            // class exercise 
                            if (gesture.Name.Equals("Clear_Right"))
                            {
                                  if(result.Confidence>0.75){
                                    clear = true;

                                    for (int i = 0; i < 4; i++)
                                    {
                                        //语音识别到的
                                        c[i].X = -198;
                                    }
                                    for (int i = 0; i < 4; i++)
                                    {
                                        if (c[i].X <= -198){
                                            isrecognized[i] = false;
                                            c[i] = new Rect(rand.Next(250, 650), 104 + 100 * rand.Next(0, 10), 170, 143);
                                            recognizedCommand.Text = "";
                                        }
                                    }

                                        soundPlayer.Play();
                                }
                                //clear = false;
                           }
                            
                        }
                    }
                    if (!recognized) recognitionResult.Text = "Waive your hand to clear👋";

                    recognitionResult.Foreground = brush; // class exercise 
                }
            }
        }
        private Boolean leftHit = false;
        private Boolean rightHit = false;
        
        private void Recognizer_SpeechRecognized(
            object sender, SpeechRecognizedEventArgs e)
        {
            recognizedCommand.Text =  e.Result.Text;
            switch(e.Result.Text){
                case "classification":
                    isrecognized[0] = true;
                    //Thickness margin = new Thickness();
                    //margin.Left = c[0].Left-300;
                    //margin.Top = c[0].Top-100;
                    //recognizedCommand.Margin = margin;

                    recognizedCommand.Margin = new Thickness(c[0].Left - 370, c[0].Top - 140, 70,70);
                    count += 1;
                    break;
                case "model fitting":
                    isrecognized[1] = true;
                    recognizedCommand.Margin = new Thickness(c[1].Left - 370, c[1].Top -140, 70, 70);
                    count += 1;
                    break;
                case "prediction":
                    isrecognized[2] = true;
                    recognizedCommand.Margin = new Thickness(c[2].Left - 370, c[2].Top - 140, 70, 70);
                    count += 1;
                    break;
                case "cross validation":
                    isrecognized[3] = true;
                    recognizedCommand.Margin = new Thickness(c[3].Left - 370, c[3].Top - 140, 70, 70);
                    count += 1;
                    break;
            }
            
        }
            private void AudioBeamFrameReader_FrameArrived(object sender, AudioBeamFrameArrivedEventArgs e)
        {
            using (AudioBeamFrameList frameList = e.FrameReference.AcquireBeamFrames())
            {
                if (frameList == null) return;

                // Only one audio beam is supported. Get the sub frame list for this beam
                IReadOnlyList<AudioBeamSubFrame> subFrameList = frameList[0].SubFrames;

                // Loop over all sub frames, extract audio buffer and beam information
                foreach (AudioBeamSubFrame subFrame in subFrameList)
                {
                    // beamAngleConfidenceTxt.Text = "Count " + subFrameList.Count;
                    beamAngleTxt.Text = "Beam Angle: " + subFrame.BeamAngle * 180 / Math.PI;
                    beamAngleConfidenceTxt.Text = "Confidence: " + subFrame.BeamAngleConfidence;

                }
            }
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
                Body body = GetClosestBody(bodyFrame);
                if (body != null)
                    vgbFrameSource.TrackingId = body.TrackingId;
                int numOfUsers = 0;

                using (DrawingContext dc = drawingGroup.Open())
                {
                    // draw a transparent background to set the render size
                    dc.DrawRectangle(System.Windows.Media.Brushes.Transparent, null,
                                  new Rect(0.0, 0.0, drawingImgWidth, drawingImgHeight));
                    
                    var segments = new[]
                    {
          new LineSegment(new System.Windows.Point(80+330, 70+300-250), true),
          new LineSegment(new System.Windows.Point(310,370-250), true)
       };
                    var figure = new PathFigure(start, segments, true);
                    var geo = new PathGeometry(new[] { figure });
                    var drawingPen = new System.Windows.Media.Pen(System.Windows.Media.Brushes.Black, 3);
                    dc.DrawLine(drawingPen, p1, new System.Windows.Point(80+330, 70+300-250));
                    dc.DrawLine(drawingPen, new System.Windows.Point(80+330, 70+300-250), new System.Windows.Point(310,370-250));
                    dc.DrawLine(drawingPen, new System.Windows.Point(310, 370-250), p1);
                    for (int i = 0; i < bodies.Length; i++)
                        if (bodies[i].IsTracked)
                        {
                            numOfUsers++;
                            CameraSpacePoint headPosi = bodies[i].Joints[JointType.Head].Position;
                            s = String.Format("Head: X: {0:0.0} Y: {1:0.0} Z: {2:0.0}",
                                                        headPosi.X, headPosi.Y, headPosi.Z);
                            DrawSkeleton(bodies[i], dc);
                            SkeletonBasedInteractivity(bodies[i], dc);
                            PositionBasedMatching(bodies[i]);
                        }
                    DisplayRecognizedImage(dc);
                    if (leftHit == true)
                    {
                        p1= new System.Windows.Point(30 + 330, 170);
                        start = p1;
                        dc.DrawGeometry(System.Windows.Media.Brushes.Red, null, geo);
                        for (int i = 0; i < 4; i++)
                        {
                            dc.DrawImage(classification.Source, shadow[0]);
                            dc.DrawImage(modelFitting.Source, shadow[1]);
                            dc.DrawImage(prediction.Source, shadow[2]);
                            dc.DrawImage(crossValidation.Source, shadow[3]);
                        }
                    }else if(leftHit==false){
                        p1= new System.Windows.Point(30 + 330, 320 - 250);
                        start = p1;
                        dc.DrawGeometry(System.Windows.Media.Brushes.Red, null, geo);
                    }
                    
                    for (int i = 0; i < bodies.Length; i++){
                       
                        VisualizeHandState(bodies[i], dc, JointType.HandLeft, bodies[i].HandLeftState);
                    }
                }
                statusTxt.Text = "#User: " + numOfUsers + "; " + s;
            }
        }
        private void PositionBasedMatching(Body body)
        {
            if (body.Joints[JointType.HandLeft].TrackingState != TrackingState.NotTracked)
            {
                System.Windows.Point leftHand = MapCameraPointToScreenSpace(body, JointType.HandLeft);
                HitTest(new System.Windows.Point(410-50,95), leftHand, 30);
                Hide(new System.Windows.Point(410-50, 145), leftHand, 30);

            }
        }
        private void HitTest(System.Windows.Point pt1, System.Windows.Point pt2, float threshold)
        {
            if ((pt1.X - pt2.X) * (pt1.X - pt2.X) +
                (pt1.Y - pt2.Y) * (pt1.Y - pt2.Y) < threshold * threshold)
            {
                if (leftHit == false)
                {
                    leftHit = true;
                }

            }
        }
        private void Hide(System.Windows.Point pt1, System.Windows.Point pt2, float threshold)
        {
            if ((pt1.X - pt2.X) * (pt1.X - pt2.X) +
                (pt1.Y - pt2.Y) * (pt1.Y - pt2.Y) < threshold * threshold)
            {
                if (leftHit == true)
                {
                    leftHit = false;
                }

            }
        }
        
        private Body GetClosestBody(BodyFrame bodyFrame)
        {
            Body[] bodies = new Body[6];
            bodyFrame.GetAndRefreshBodyData(bodies);
            Body closestBody = null;
            foreach (Body b in bodies)
            {
                if (b.IsTracked)
                {
                    if (closestBody == null) closestBody = b;
                    else
                    {
                        Joint newHeadJoint = b.Joints[JointType.Head];
                        Joint oldHeadJoint = closestBody.Joints[JointType.Head];
                        if (newHeadJoint.TrackingState == TrackingState.Tracked &&
                        newHeadJoint.Position.Z < oldHeadJoint.Position.Z)
                        {
                            closestBody = b;
                        }
                    }
                }
            }
            return closestBody;
        }
            private ColorSpacePoint MapCameraPointToColorSpace(Body body, JointType jointType)
        {
            return sensor.CoordinateMapper.MapCameraPointToColorSpace(
                body.Joints[jointType].Position);
        }

        private void DrawSkeleton(Body body, DrawingContext dc)
        {
 
            for (int i = 0; i < bones.Length; i += 2)
            // for (int i = 0; i < bones_crazy.Length; i += 2)
            {
                DrawBone(body, dc, bones[i], bones[i + 1]);
            }
            // Exercise 3
            foreach (JointType jt in body.Joints.Keys) {
                ColorSpacePoint pt = MapCameraPointToColorSpace(body, jt);
                dc.DrawEllipse(touming, null, new System.Windows.Point(pt.X, pt.Y), 10, 10);
            }
            RenderClippedEdges(body, dc);
            //VisualizeHandState(body, dc, JointType.HandLeft, body.HandLeftState);
  
        }

        private void DrawBone(Body body, DrawingContext dc,
                JointType j0, JointType j1)
        {
            ColorSpacePoint pt0 = MapCameraPointToColorSpace(body, j0);
            ColorSpacePoint pt1 = MapCameraPointToColorSpace(body, j1);

            dc.DrawLine(new System.Windows.Media.Pen(touming, 5),
                            new System.Windows.Point(pt0.X, pt0.Y), new System.Windows.Point(pt1.X, pt1.Y));
        }
        public System.Windows.Point MapCameraPointToScreenSpace(Body body, JointType jointType)
        {
            System.Windows.Point screenPt = new System.Windows.Point(0, 0);
            if (mapToColorSpace) // to color space 
            {
                ColorSpacePoint pt = sensor.CoordinateMapper.MapCameraPointToColorSpace(
                body.Joints[jointType].Position);
                screenPt.X = pt.X;
                screenPt.Y = pt.Y;
            }
            else // to depth space
            {
                DepthSpacePoint pt = sensor.CoordinateMapper.MapCameraPointToDepthSpace(
                    body.Joints[jointType].Position);
                screenPt.X = pt.X;
                screenPt.Y = pt.Y;
            }
            return screenPt;
        }

        private void RenderClippedEdges(Body body, DrawingContext drawingContext)
        {
            double clipBoundsThickness = 10;

            if (body.ClippedEdges.HasFlag(FrameEdges.Bottom))
            {
                drawingContext.DrawRectangle(
                    System.Windows.Media.Brushes.Red, null,
                    new Rect(0, drawingImgHeight - clipBoundsThickness, drawingImgWidth, clipBoundsThickness));
            }

            if (body.ClippedEdges.HasFlag(FrameEdges.Top))
            {
                drawingContext.DrawRectangle(
                    System.Windows.Media.Brushes.Red, null,
                    new Rect(0, 0, drawingImgWidth, clipBoundsThickness));
            }

            if (body.ClippedEdges.HasFlag(FrameEdges.Left))
            {
                drawingContext.DrawRectangle(
                    System.Windows.Media.Brushes.Red, null,
                    new Rect(0, 0, clipBoundsThickness, drawingImgHeight));
            }

            if (body.ClippedEdges.HasFlag(FrameEdges.Right))
            {
                drawingContext.DrawRectangle(
                    System.Windows.Media.Brushes.Red, null,
                    new Rect(drawingImgWidth - clipBoundsThickness, 0, clipBoundsThickness, drawingImgHeight));
            }
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
//this part is mainly my won contribution
            
                for (int i=0;i<4;i++){
                if ((c[i].Contains(leftHand.X, leftHand.Y) || c[i].Contains(leftTip.X, leftTip.Y)))
                {
                    //rectangle1.Fill = new SolidColorBrush(Color.FromRgb(0, 111, 0));
                    for (int j = 0; j < 4; j++)
                    {
                        isTouchedLeft[j] = false;
                        isTouchedLeft1[j] = false;
                    }
                        isTouchedLeft[i] = true;
                }

                if ((isrecognized[i] == true) && isTouchedLeft[i] == true)// hit test & hand gestures 
                {
                    dc.DrawRectangle(System.Windows.Media.Brushes.Yellow, null, new Rect(c[i].X - 7, c[i].Y - 5, c[i].Width * 1.05, c[i].Height * 1.08));
                    if (body.HandLeftState == HandState.Closed)
                    {
                        c[i].X = leftHand.X - c[i].Width / 2; // move
                        c[i].Y = leftHand.Y - c[i].Height / 2;
                        dc.DrawRectangle(System.Windows.Media.Brushes.Red, null, new Rect(c[i].X - 7, c[i].Y - 5, c[i].Width * 1.05, c[i].Height * 1.08));
                    }
                }
                
                if ((c[i].Contains(rightHand.X, rightHand.Y) || c[i].Contains(rightThumb.X, rightThumb.Y) || c[i].Contains(rightTip.X, rightTip.Y)) && (c[i].Contains(leftHand.X, leftHand.Y) || c[i].Contains(leftThumb.X, leftThumb.Y) || c[i].Contains(leftTip.X, leftTip.Y)))
                {
                    isBoth[i] = true;
                }else if ((Math.Abs(c[i].X - rightHand.X) > 80 && Math.Abs(c[i].Y - rightHand.Y) > 80) && (Math.Abs(c[i].Y - leftHand.Y) > 80 && Math.Abs(c[i].Y - leftHand.Y) > 80))
                {
                    isBoth[i] = false;
                }
                if (isBoth[i] == true)
                {
                    if (Math.Abs(rightHand.X - leftHand.X) > 150)
                    {
                        c[i].Width = Math.Abs(rightHand.X - leftHand.X);
                        c[i].Height = c[i].Width / 1.4;
                        dc.DrawRectangle(System.Windows.Media.Brushes.Green, null, new Rect(c[i].X - 7, c[i].Y - 5, c[i].Width * 1.05, c[i].Height * 1.05));
                    }
                }
                if ((r[i].Contains(leftHand.X, leftHand.Y) || r[i].Contains(leftTip.X, leftTip.Y)))
                {
                    //rectangle1.Fill = new SolidColorBrush(Color.FromRgb(0, 111, 0));
                    
                    for (int j = 0; j < 4; j++)
                    {
                        isTouchedLeft1[j] = false;
                        isTouchedLeft[j] = false;
                    }
                    isTouchedLeft1[i] = true;
                }
                if ((r[i].Contains(rightHand.X, rightHand.Y) || r[i].Contains(rightThumb.X, rightThumb.Y) || r[i].Contains(rightTip.X, rightTip.Y)) && (r[i].Contains(leftHand.X, leftHand.Y) || r[i].Contains(leftThumb.X, leftThumb.Y) || r[i].Contains(leftTip.X, leftTip.Y)))
                {
                    isBoth1[i] = true;
                }
                else if ((Math.Abs(r[i].X - rightHand.X) > 80 && Math.Abs(r[i].Y - rightHand.Y) > 80) && (Math.Abs(r[i].Y - leftHand.Y) > 80 && Math.Abs(r[i].Y - leftHand.Y) > 80))
                {
                    isBoth1[i] = false;
                }
                
                if ((leftHit )&& isTouchedLeft1[i])// hit test & hand gestures 
                {
                    dc.DrawRectangle(System.Windows.Media.Brushes.Yellow, null, new Rect(r[i].X - 7, r[i].Y - 5, r[i].Width * 1.05, r[i].Height * 1.08));
                    if (body.HandLeftState == HandState.Closed)
                    {
                        r[i].X = leftHand.X - r[i].Width / 2; // move
                        r[i].Y = leftHand.Y - r[i].Height / 2;
                        dc.DrawRectangle(System.Windows.Media.Brushes.Red, null, new Rect(r[i].X - 7, r[i].Y - 5, r[i].Width * 1.05, r[i].Height * 1.08));
       
                    }
                    if (isBoth1[i] == true)
                    {
                        if (Math.Abs(rightHand.X - leftHand.X) > 150)
                        {
                            r[i].Width = Math.Abs(rightHand.X - leftHand.X);
                            r[i].Height = r[i].Width / 1.4;
                            dc.DrawRectangle(System.Windows.Media.Brushes.Green, null, new Rect(r[i].X - 7, r[i].Y - 5, r[i].Width * 1.05, r[i].Height * 1.05));
                        }
                    }
                }
                if(r[i].X > 108){
                    if (isTouchedLeft1[i]){
                        dc.DrawRectangle(System.Windows.Media.Brushes.Yellow, null, new Rect(r[i].X - 7, r[i].Y - 5, r[i].Width * 1.05, r[i].Height * 1.08));
                        if (body.HandLeftState == HandState.Closed)
                        {
                            r[i].X = leftHand.X - r[i].Width / 2; // move
                            r[i].Y = leftHand.Y - r[i].Height / 2;
                            dc.DrawRectangle(System.Windows.Media.Brushes.Red, null, new Rect(r[i].X - 7, r[i].Y - 5, r[i].Width * 1.05, r[i].Height * 1.08));
                        }
                    }
                    if (isBoth1[i] == true)
                    {
                        if (Math.Abs(rightHand.X - leftHand.X) > 150)
                        {
                            r[i].Width = Math.Abs(rightHand.X - leftHand.X);
                            r[i].Height = r[i].Width / 1.4;
                            dc.DrawRectangle(System.Windows.Media.Brushes.Green, null, new Rect(r[i].X - 7, r[i].Y - 5, r[i].Width * 1.05, r[i].Height * 1.05));
                        }
                    }
                    if (i == 0)
                    {
                        dc.DrawImage(classification.Source, r[0]);
                    }
                    else if (i == 1)
                    {
                        dc.DrawImage(modelFitting.Source, r[1]);
                    }
                    else if (i == 2)
                    {
                        dc.DrawImage(prediction.Source, r[2]);
                    }
                    else if (i == 3)
                    {
                        dc.DrawImage(crossValidation.Source, r[3]);
                    }
                }
            } 
        }
        private void VisualizeHandState(Body body, DrawingContext dc, JointType jointType, HandState handState)
        {
            SolidColorBrush red = new SolidColorBrush(System.Windows.Media.Color.FromArgb(100, 255, 0, 0));
            SolidColorBrush green = new SolidColorBrush(System.Windows.Media.Color.FromArgb(100, 0, 255, 0));
            System.Drawing.Pen skyBluePen = new System.Drawing.Pen(System.Drawing.Brushes.DeepSkyBlue, 5);

            // Set the pen's width.
            double radius = 20;
            ColorSpacePoint hand_pt = MapCameraPointToColorSpace(body, jointType);
            switch (handState)
            {
                case HandState.Closed:
                    isClosed = false;
                    break;
                case HandState.Open:
                    isPointing = false;
                    break;
                case HandState.Lasso:
                    isPointing = true;
                    break;
            }
            if (isPointing == true)
            {
                float pPointerX = hand_pt.X;
                float pPointerY = hand_pt.Y;
                pPointerX = hand_pt.X;
                pPointerY = hand_pt.Y;
                dc.DrawEllipse(blue, null, new Point(hand_pt.X, hand_pt.Y), radius, radius);
            }

        }

        private RecognizerInfo FindKinectRecognizerInfo()
        {
            var recognizers =
                SpeechRecognitionEngine.InstalledRecognizers();

            foreach (RecognizerInfo recInfo in recognizers)
            {
                // look at each recognizer info value 
                // to find the one that works for Kinect
                if (recInfo.AdditionalInfo.ContainsKey("Kinect"))
                {
                    string details = recInfo.AdditionalInfo["Kinect"];
                    if (details == "True"
            && recInfo.Culture.Name == "en-US")
                    {
                        // If we get here we have found 
                        // the info we want to use
                        return recInfo;
                    }
                }
            }
            return null;
        }
    }
}
