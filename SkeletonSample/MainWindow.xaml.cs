using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using Coding4Fun.Kinect.Wpf;
using Microsoft.Kinect;

namespace SkeletonSample
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        KinectSensor kinect;
        int actionCount = 0;
        float preHeadPos    = 0;
        float preRHandPos   = 0;
        float preLHandPos   = 0;
        float headPos       = 0;
        float lHandPos      = 0;
        float rHandPos      = 0;
        int skeltonStatus    = -1; //down -1 up 1 else 0
        int preSkeltonStatus = -1; //down -1 up 1 else 0



        public MainWindow()
        {
            InitializeComponent();

            try
            {
                // 利用可能なKinectを探す
                foreach (var k in KinectSensor.KinectSensors)
                {
                    if (k.Status == KinectStatus.Connected)
                    {
                        kinect = k;
                        break;
                    }
                }
                if (kinect == null)
                {
                    throw new Exception("利用可能なKinectがありません");
                }

                // すべてのフレーム更新通知をもらう
                kinect.AllFramesReady += new EventHandler<AllFramesReadyEventArgs>(kinect_AllFramesReady);

                // Color,Depth,Skeletonを有効にする
                kinect.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);
                //kinect.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30);
                kinect.SkeletonStream.Enable();

                // Kinectの動作を開始する
                kinect.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                Close();
            }
        }

        // すべてのデータの更新通知を受け取る
        void kinect_AllFramesReady(object sender, AllFramesReadyEventArgs e)
        {
            // Disposableなのでusingでくくる
            using (ColorImageFrame colorFrame = e.OpenColorImageFrame())
            {
                if (colorFrame != null)
                {
                    imageRgbCamera.Source = colorFrame.ToBitmapSource();
                }
            }

            // Disposableなのでusingでくくる
            using (DepthImageFrame depthFrame = e.OpenDepthImageFrame())
            {
                if (depthFrame != null)
                {
                    imageDepthCamera.Source = depthFrame.ToBitmapSource();
                }
            }

            // Disposableなのでusingでくくる
            using (SkeletonFrame skeletonFrame = e.OpenSkeletonFrame())
            {
                if (skeletonFrame != null)
                {
                    // 骨格位置の表示
                    ShowSkeleton(skeletonFrame);
                }
            }
        }

        private void ShowSkeleton(SkeletonFrame skeletonFrame)
        {
            // キャンバスをクリアする
            canvasSkeleton.Children.Clear();

            // スケルトンデータを取得する
            Skeleton[] skeletonData = new Skeleton[skeletonFrame.SkeletonArrayLength];
            skeletonFrame.CopySkeletonDataTo(skeletonData);

            System.Console.Out.Write("length: "+skeletonData.Length);
            // プレーヤーごとのスケルトンを描画する
            if (skeletonData.Length > 1)
            {
                var skeleton = skeletonData[1];
                if(skeleton.Joints[JointType.Head].TrackingState != JointTrackingState.NotTracked) {
                    positionLabel.Content = "headY: " + skeleton.Joints[JointType.Head].Position.Y;
                    preHeadPos = headPos;
                    headPos = skeleton.Joints[JointType.Head].Position.Y;
                }

                if (skeleton.Joints[JointType.HandRight].TrackingState != JointTrackingState.NotTracked)
                {
                    positionLabel2.Content = "rHandY: " + skeleton.Joints[JointType.HandRight].Position.Y;
                    preRHandPos = rHandPos;
                    rHandPos = skeleton.Joints[JointType.HandRight].Position.Y;
                }

                if (skeleton.Joints[JointType.HandLeft].TrackingState != JointTrackingState.NotTracked)
                {
                    positionLabel3.Content = "lHandY: " + skeleton.Joints[JointType.HandLeft].Position.Y;
                    preLHandPos = lHandPos;
                    lHandPos = skeleton.Joints[JointType.HandLeft].Position.Y;
                }

                checkSkeltonStatus();
                if (this.checkAction())
                {
                    this.actionCount += 1;
                    actionCountLabel.Content = this.actionCount + "　回";

                }

                //positionLabel.Content = skeleton.Joints[0].Position.X.ToString();
                // 追跡されているプレイヤー
                if (skeleton.TrackingState == SkeletonTrackingState.Tracked)
                {   
                    
                    // 骨格を描画する
                    foreach (Joint joint in skeleton.Joints)
                    {
                        // 追跡されている骨格
                        if (joint.TrackingState != JointTrackingState.NotTracked)
                        {
                            // 骨格の座標をカラー座標に変換する
                            ColorImagePoint point = kinect.MapSkeletonPointToColor(joint.Position, kinect.ColorStream.Format);

                            // 円を書く
                            canvasSkeleton.Children.Add(new Ellipse()
                            {
                                Margin = new Thickness(point.X, point.Y, 0, 0),
                                Fill = new SolidColorBrush(Colors.Black),
                                Width = 20,
                                Height = 20,
                            });
                        }
                    }
                }
            }
        }

        private bool checkAction()
        {
            return this.skeltonStatus == 1 && this.preSkeltonStatus == -1;
        }

        private void checkSkeltonStatus()
        {   
            //Current Skeleton Status
            if (this.lHandPos > 0.1 && this.rHandPos > 0.1 && this.headPos > 0.1)
            {
                this.skeltonStatus = 1;
            }
            else if (this.lHandPos < -0.1 && this.rHandPos < -0.1 && this.headPos < 0.0)
            {   
                this.skeltonStatus = -1;
            }
            //Previous Skeleton Status
            if (this.preLHandPos > 0.1 && this.preRHandPos > 0.1 && this.preHeadPos > 0.1)
            {
                this.preSkeltonStatus = 1;
            }
            else if (this.preLHandPos < -0.1 && this.preRHandPos < -0.1 && this.preHeadPos < 0.0)
            {
                this.preSkeltonStatus = -1;
            }
        }
    }
}