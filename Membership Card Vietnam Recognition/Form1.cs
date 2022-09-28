using Emgu.CV;
using Emgu.CV.Dnn;
using Emgu.CV.Util;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Emgu.CV.CvEnum;
using System.IO;

namespace Membership_Card_Vietnam_Recognition
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        Net ModelDet = null;
        string PathConfigDet = "yolov4-tiny-custom_det.cfg";
        string PathWeightsDet = "yolov4-tiny-custom_det.weights";
        Net ModelRec = null;
        string PathConfigRec = "yolov4-custom_rec.cfg";
        string PathWeightsRec = "yolov4-custom_rec.weights";
        OpenFileDialog ofd = new OpenFileDialog();
        private void button1_Click(object sender, EventArgs e)
        {
            //c# open file dialog with image filters
            using ( ofd = new OpenFileDialog() { Filter = "Image Files(*.jpg; *.jpeg; *.gif; *.bmp; *.png)|*.jpg; *.jpeg; *.gif; *.bmp; *.png" })
            {
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    //image validation
                    try
                    {
                        Bitmap bmp = new Bitmap(ofd.FileName);//to validate the image
                        if (bmp != null)
                        {//if image is valid
                            pictureBox1.Load(ofd.FileName);//display selected image file
                            pictureBox1.Image.RotateFlip(Rotate(bmp));//display image in proper orientation
                            bmp.Dispose();
                            pictureBox2.Image = null;
                            //System.IO.File.Delete(@"imgCrop.jpg");
                        }
                    }
                    catch (ArgumentException)
                    {
                        MessageBox.Show("The specified image file is invalid.");
                    }                   
                }
            }
        }
        public static RotateFlipType Rotate(Image bmp)
        {
            const int OrientationId = 0x0112;
            PropertyItem pi = bmp.PropertyItems.Select(x => x)
                                        .FirstOrDefault(x => x.Id == OrientationId);
            if (pi == null)
                return RotateFlipType.RotateNoneFlipNone;

            byte o = pi.Value[0];

            //Orientations
            if (o == 2) //TopRight
                return RotateFlipType.RotateNoneFlipX;
            if (o == 3) //BottomRight
                return RotateFlipType.RotateNoneFlipXY;
            if (o == 4) //BottomLeft
                return RotateFlipType.RotateNoneFlipY;
            if (o == 5) //LeftTop
                return RotateFlipType.Rotate90FlipX;
            if (o == 6) //RightTop
                return RotateFlipType.Rotate90FlipNone;
            if (o == 7) //RightBottom
                return RotateFlipType.Rotate90FlipY;
            if (o == 8) //LeftBottom
                return RotateFlipType.Rotate90FlipXY;

            return RotateFlipType.RotateNoneFlipNone; //TopLeft (what the image looks by default) [or] Unknown
        }
        private void button2_Click(object sender, EventArgs e)
        {
            float conf_threshold = 0.5f;
            if (ofd.FileName != String.Empty)
            {
                var img = new Image<Bgr, byte>(ofd.FileName);
                double scale = 0.00392;            
                //Crop ảnh để detect
                var input = DnnInvoke.BlobFromImage(img, scale, new Size(416, 416),new MCvScalar(0,0,0), swapRB: true, crop: false);
                ModelDet.SetInput(input);
                VectorOfMat vectorOfMat = new VectorOfMat();
                ModelDet.Forward(vectorOfMat, ModelDet.UnconnectedOutLayersNames);
                Image<Bgr, byte> imageCrop = img.Clone();
                List<Rectangle> ListRec = new List<Rectangle>();
                List<int> EdgeID = new List<int>();
                List<float> confidences = new List<float>();
                for (int k = 0; k < vectorOfMat.Size; k++)
                {
                    var mat = vectorOfMat[k];
                    var data = ArrayTo2DList(mat.GetData());                    
                    for (int i = 0; i < data.Count; i++)
                    {
                        var row = data[i];
                        var rowsscores = row.Skip(5).ToArray();
                        var classId = rowsscores.ToList().IndexOf(rowsscores.Max());
                        var confidence = rowsscores[classId];
                        //Kiem tra nguong tin cay
                        if (confidence > conf_threshold)
                        {
                            var center_x = (int)(row[0] * img.Width);
                            var center_y = (int)(row[1] * img.Height);

                            var width = (int)(row[2] * img.Width);
                            var height = (int)(row[3] * img.Height);

                            var x = (int)(center_x - (width / 2));
                            var y = (int)(center_y - (height / 2));
                            Rectangle edge = new Rectangle(x, y, width, height);                           
                            imageCrop = img.Clone();
                            imageCrop.ROI = edge;
                            confidences.Add(confidence);
                            EdgeID.Add(classId);
                            ListRec.Add(edge);
                            //CvInvoke.Imshow("imagecrop", imageCrop.Mat);
                            //CvInvoke.WaitKey();
                        }
                    }
                }
                float nms_threshold = 0.4f;
                List<int> indices = new List<int>();
                indices = DnnInvoke.NMSBoxes(ListRec.ToArray(), confidences.ToArray(), conf_threshold, nms_threshold).ToList();
                List<Coordinate> listXY = new List<Coordinate>();
                List<string> labels = new List<string>();
                string[] classes = { "top_left", "top_right", "bottom_right", "bottom_left" };
                Image<Bgr, byte> imageCropDraw = img.Clone();
                foreach (var i in indices)
                {
                    var x = ListRec[i].X;
                    var y = ListRec[i].Y;
                    var w = ListRec[i].Width;
                    var h = ListRec[i].Height;
                    listXY.Add(new Coordinate() { X = x, Y = y }); 
                    labels.Add(classes[EdgeID[i]]);
                    ///Vẽ bouding box + label lên ảnh
                   //CvInvoke.Rectangle(imageCropDraw, new Rectangle(x, y, w, h), new MCvScalar(0, 0, 255), 2);
                   //CvInvoke.PutText(imageCropDraw, classes[EdgeID[i]], new Point(x-5,y-5), FontFace.HersheySimplex, 0.3, new MCvScalar(0,0,255), 3);
                }
                if (!CheckLabels(classes, labels))
                {
                    MessageBox.Show("Không detect được. Thử lại ảnh khác !", "TD JSC");
                }
                else
                {
                    var dic = labels.Zip(listXY, (s, i) => new { s, i })
                          .ToDictionary(item => item.s, item => item.i);
                    // Two-dimensional array.
                    PointF[] source_points = new PointF[4];
                    source_points[0] = new PointF(dic["top_left"].X, dic["top_left"].Y);
                    source_points[1] = new PointF(dic["bottom_left"].X, dic["bottom_left"].Y);
                    source_points[2] = new PointF(dic["bottom_right"].X, dic["bottom_right"].Y);
                    source_points[3] = new PointF(dic["top_right"].X, dic["top_right"].Y);
                    var crop = PerspectiveTransform(imageCropDraw, source_points);
                    crop = ResizeImage(crop, 720, 0);
                    CvInvoke.Imshow("ImgDet", crop.Mat);
                    CvInvoke.WaitKey();
                    //CardInfoReturn obj = new CardInfoReturn();
                    //CardInformation res = obj.Result("123", "NVS", "11-11-2011", "Bac Ninh", "11-11-2011", "11-11-2012", "BN", "11-10-2015", crop, true);
                    //Console.WriteLine(res.FullName);
                    ///Show các thông tin trong thẻ 
                    //pictureBox2.Load("imgCrop.jpg");
                    //pictureBox2.Image.RotateFlip(Rotate(crop.ToBitmap()));//Display selected image file

                }
            }
            else
            {
                MessageBox.Show("Kiểm tra lại đường dẫn ảnh !","TD JSC");
                //Application.Exit();
            }    
        }

        public Image<Bgr, byte> PerspectiveTransform(Image<Bgr, byte> imageCropDraw, PointF[] points)
        {
            // Use L2 norm           
            double width_AD = Math.Sqrt(Math.Pow((points[0].X - points[3].X), 2) + Math.Pow((points[0].Y - points[3].Y), 2));
            double width_BC = Math.Sqrt(Math.Pow((points[1].X - points[2].X), 2) + Math.Pow((points[1].Y - points[2].Y), 2));
            double maxWidth = Math.Max((int)width_AD, (int)width_BC);
            double height_AB = Math.Sqrt(Math.Pow((points[0].X - points[1].X), 2) + Math.Pow((points[0].Y - points[1].Y), 2));
            double height_CD= Math.Sqrt(Math.Pow((points[2].X - points[3].X), 2) + Math.Pow((points[2].Y - points[3].Y), 2));
            double maxHeight = Math.Max((int)height_AB, (int)height_CD);
            PointF[] output_pts = new PointF[4];
            output_pts[0] = new PointF(0,0);
            output_pts[1] = new PointF(0, (float)maxHeight - 1);
            output_pts[2] = new PointF((float)maxWidth - 1, (float)maxHeight - 1);
            output_pts[3] = new PointF((float)maxWidth - 1, 0);
            Mat M = CvInvoke.GetPerspectiveTransform(points, output_pts);           
            CvInvoke.WarpPerspective(imageCropDraw, imageCropDraw, M, new Size((int)maxWidth, (int)maxHeight), interpolationType: Inter.Linear);
            CvInvoke.Imwrite("imgCrop.jpg", imageCropDraw);
            return imageCropDraw;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                ModelDet = DnnInvoke.ReadNetFromDarknet(PathConfigDet, PathWeightsDet);//Load model detection card
                ModelRec = DnnInvoke.ReadNetFromDarknet(PathConfigRec, PathWeightsRec);//Load model recognition card
                //ModelDet.SetPreferableBackend(Emgu.CV.Dnn.Backend.OpenCV);
                //ModelDet.SetPreferableTarget(Target.Cpu);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                Application.Exit();
            }
        }
        public static List<float[]> ArrayTo2DList(Array array)
        {
            //System.Collections.IEnumerator enumerator = array.GetEnumerator();
            int rows = array.GetLength(0);
            int cols = array.GetLength(1);
            List<float[]> list = new List<float[]>();
            List<float> temp = new List<float>();

            for (int i = 0; i < rows; i++)
            {
                temp.Clear();
                for (int j = 0; j < cols; j++)
                {
                    temp.Add(float.Parse(array.GetValue(i, j).ToString()));
                }
                list.Add(temp.ToArray());
            }

            return list;
        }
        public static Image<Bgr, byte> ResizeImage(Image<Bgr, byte> imageOriginal, int width = 0, int height = 0)
        {
            var dim = new Size(0, 0);
            (int w, int h) = (imageOriginal.Width, imageOriginal.Height);
            if (width == 0 && height == 0)
            {
                return imageOriginal;
            }
            if (width == 0)
            {
                double r = height / (float)h;
                dim.Width = (int)(w * r);
                dim.Height = height;
            }
            else
            {
                //double r = width / (float)w;
                double r = width / (float)w;
                dim.Width = width;
                dim.Height = (int)(h * r);
            }
            Image<Bgr, byte> imageReszie = imageOriginal.Resize(dim.Width, dim.Height, Inter.Cubic);
            return imageReszie;
        }
        public bool CheckLabels(string[] classes, List<string> labels)
        {
            bool check = true;
            for (int i=0; i<classes.Length; i++)
            {
                if (!labels.Contains(classes[i]))
                { 
                    check = false;
                    break;
                }    
            }
            return check;
        }        
    }
    public class Coordinate
    {
        /// <summary>
        /// Toạ độ x
        /// </summary>
        public float X { get; set; }
        /// <summary>
        /// Toạ độ y
        /// </summary>
        public float Y { get; set; }
        /// <summary>
        /// Chiều rộng của Rectangle
        /// </summary>
        public float Width { get; set; }
        /// <summary>
        /// Chiều dài của Rectangle
        /// </summary>
        public float Height { get; set; }
    }
    public class CardInformationLP
    {
        /// <summary>
        /// ID 
        /// </summary>
        public string ID { get; set; }
        /// <summary>
        /// Họ và tên
        /// </summary>       
        public string FullName { get; set; }
        /// <summary>
        /// Ngày sinh
        /// </summary>
        public string DateOfBirth { get; set; }
        /// <summary>
        /// Quê quán
        /// </summary>
        public string Home { get; set; }
        /// <summary>
        /// Vào đảng ngày
        /// </summary>
        public string JoinDate { get; set; }
        /// <summary>
        /// Chính thức ngày
        /// </summary>
        public string OfficialDate { get; set; }
        /// <summary>
        /// Nơi cấp thẻ
        /// </summary>
        public string IssuedBy { get; set; }
        /// <summary>
        /// Ngày cấp thẻ
        /// </summary>
        public string IssueDate { get; set; }
        /// <summary>
        /// Ảnh thẻ
        /// </summary>
        public Image<Bgr, byte> Image { get; set;} 
    }
    public class CardInfoReturnLP
    {
        public CardInformationLP Result(string id, string fullName, string dateofBirth, string home, string joinDate, string officialDate, string issuedBy, string issueDate, Image<Bgr, byte> image, bool saveImg)
        {
            CardInformationLP results = new CardInformationLP();
            results.ID = id;
            results.FullName = fullName;
            results.DateOfBirth = dateofBirth;
            results.Home = home;
            results.JoinDate = joinDate;
            results.OfficialDate = officialDate;
            results.IssuedBy = issuedBy;
            results.IssueDate = issueDate;
            if (saveImg)
            {
                results.Image = image;
                string root = Environment.CurrentDirectory;
                string pathSave = root + @"\anhthe";           
                if (Directory.Exists(pathSave))
                {
                    CvInvoke.Imwrite(pathSave + @"\anhthe"+id+".jpg", image);
                }
                else
                {
                    Directory.CreateDirectory(pathSave);
                    CvInvoke.Imwrite(pathSave + @"\anhthe" + id + ".jpg", image);
                }
            }
            else results.Image = null;
            return results;
        }
    }
    
}
