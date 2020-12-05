using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using Emgu.CV.OCR;

namespace Lab5_AOCI
{
    public partial class Form1 : Form
    {
        private Image<Bgr, byte> sourceImage;
        Processor processor;
        Tesseract _ocr;
        VideoCapture capture;
        CascadeClassifier face;
        Mat image = new Mat();
        Image<Bgr, byte> input;
        Mat frame;
        int frameCounter = 0;
        String lastText;

        bool playingVideoText;
        bool playingVideoMask;
        public Form1()
        {
            InitializeComponent();
            processor = new Processor(new Tesseract("C:\\tessdata", "eng", OcrEngineMode.TesseractLstmCombined));
            playingVideoText = false;
            playingVideoText = false;
        }

        private void button1_Click(object sender, EventArgs e)
        {           
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Файлы изображений (*.jpg,  *.jpeg,  *.jpe,  *.jfif,  *.png)  |  *.jpg;  *.jpeg;  *.jpe;  *.jfif; *.png";
            var result = openFileDialog.ShowDialog(); // открытие диалога выбора файла

            if (result == DialogResult.OK) // открытие выбранного файла
            {
                string fileName = openFileDialog.FileName;
                processor.SrcImg = new Image<Bgr, byte>(fileName).Resize(550, 350, Inter.Linear);
                imageBox1.Image = processor.SrcImg;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            imageBox2.Image = processor.drawInterestAreas();
            Image<Bgr, byte> roi = processor.getInterestArea((int)numericUpDown1.Value);
            imageBox3.Image = roi;
            listBox1.Items.Add(processor.getTextFromROI(roi));
            listBox1.SelectedIndex = listBox1.Items.Count - 1;
            listBox1.SelectedIndex = -1;
        }


       private void button3_Click_1(object sender, EventArgs e)
        {
            imageBox3.Image = null;
            imageBox1.Image = null;

            if (playingVideoText)
            {
                timer1.Enabled = false;
                frameCounter = 0;
                playingVideoText = false;
                return;
            }
            if (playingVideoMask)
            {
                timer2.Enabled = false;
                frameCounter = 0;
                playingVideoMask = false;
                return;
            }
            OpenFileDialog openFileDialog = new OpenFileDialog();
            //openFileDialog.Filter = "Файлы видео (*.webm,  *.mp4)  |  *.webm;  *.mp4";
            var result = openFileDialog.ShowDialog(); // открытие диалога выбора файла

            if (result == DialogResult.OK) // открытие выбранного файла
            {
                string fileName = openFileDialog.FileName;
                capture = new VideoCapture(fileName);
                timer1.Enabled = true;
                playingVideoText = true;           
            }
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            Image<Bgr, byte> roi = processor.getInterestArea((int)numericUpDown1.Value);
            imageBox3.Image = roi; 
            listBox1.Items.Add(processor.getTextFromROI(roi));
            listBox1.SelectedIndex = listBox1.Items.Count - 1;
            listBox1.SelectedIndex = -1;
        }

        private void timer1_Tick_1(object sender, EventArgs e)
        {
            var frame = capture.QueryFrame();
            sourceImage = frame.ToImage<Bgr, byte>().Resize(550, 350, Inter.Linear);
            processor.SrcImg = sourceImage;

            if (playingVideoText)
            {
                imageBox2.Image = processor.drawInterestAreas();
                String newText = processor.getTextFromVideoFrame();
                if (!newText.Equals(lastText))
                {
                    listBox1.Items.Add(processor.getTextFromVideoFrame());
                    listBox1.SelectedIndex = listBox1.Items.Count - 1;
                    listBox1.SelectedIndex = -1;
                    lastText = newText;
                }
            }
                //imageBox2.Image = GetImageWithCellShading(trackBar3.Value);
            //imageBox2.Image = GetImageWithCanny();
            frameCounter++;

            if (frameCounter >= capture.GetCaptureProperty(CapProp.FrameCount))
            {
                timer1.Enabled = false;
                frameCounter = 0;
                playingVideoText = false;
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            imageBox3.Image = null;
            imageBox1.Image = null;

            if (playingVideoText)
            {
                timer1.Enabled = false;
                frameCounter = 0;
                playingVideoText = false;
                return;
            }
            if (playingVideoMask)
            {
                timer2.Enabled = false;
                frameCounter = 0;
                playingVideoMask = false;
                return;
            }
            OpenFileDialog openFileDialog = new OpenFileDialog();
            //openFileDialog.Filter = "Файлы изображений (*.jpg,  *.jpeg,  *.jpe,  *.jfif,  *.png)  |  *.jpg;  *.jpeg;  *.jpe;  *.jfif; *.png";
            var result = openFileDialog.ShowDialog(); // открытие диалога выбора файла

            if (result == DialogResult.OK) // открытие выбранного файла
            {
                string fileName = openFileDialog.FileName;
                processor.Frame = CvInvoke.Imread(fileName, ImreadModes.Unchanged);
            }
            openFileDialog = new OpenFileDialog();
            //openFileDialog.Filter = "Файлы видео (*.webm,  *.mp4)  |  *.webm;  *.mp4";
            result = openFileDialog.ShowDialog(); // открытие диалога выбора файла

            if (result == DialogResult.OK) // открытие выбранного файла
            {
                string fileName = openFileDialog.FileName;
                capture = new VideoCapture(fileName);
                timer2.Enabled = true;
                playingVideoMask = true;
            }
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            var frame = capture.QueryFrame();
            sourceImage = frame.ToImage<Bgr, byte>().Resize(550, 350, Inter.Linear);
            processor.SrcImg = sourceImage;

            if (playingVideoMask)
            {
                imageBox2.Image = processor.drawMaskOnFaces().Resize(550, 350, Inter.Linear);
            }
            frameCounter++;

            if (frameCounter >= capture.GetCaptureProperty(CapProp.FrameCount))
            {
                timer2.Enabled = false;
                frameCounter = 0;
                playingVideoMask = false;
            }
        }



        /* private void ProcessFrame(object sender, EventArgs e)
         {
             if (capture != null && capture.Ptr != IntPtr.Zero)
             {
                 capture.Retrieve(image);

                 input = image.ToImage<Bgr, byte>();

                 List<Rectangle> faces = new List<Rectangle>();

                 //using (CascadeClassifier face = new CascadeClassifier("C:\\haarcascades\\haarcascade_frontalface_default.xml"))
                 //{
                 //    using (Mat ugray = new Mat())
                 //    {
                 //        CvInvoke.CvtColor(sourceImage, ugray, Emgu.CV.CvEnum.ColorConversion.Bgr2Gray);
                 //        Rectangle[] facesDetected = face.DetectMultiScale(ugray, 1.1, 10, new Size(20, 20));
                 //        faces.AddRange(facesDetected);
                 //    }
                 //}

                 foreach (Rectangle rect in faces) input.Draw(rect, new Bgr(Color.Yellow), 2);

                 Mat ugray = new Mat();
                 CvInvoke.CvtColor(image, ugray, ColorConversion.Bgr2Gray);
                 Rectangle[] facedDetected = face.DetectMultiScale(ugray, 1.1, 10, new Size(20, 20));
                 faces.AddRange(facedDetected);

                 Image<Bgra, byte> res = input.Convert<Bgra, byte>();

                 if (faces.Count > 0)
                 {
                     foreach (Rectangle rect in faces)
                     {
                         res.ROI = rect;

                         Image<Bgr, byte> small = frame.ToImage<Bgra, byte>().Resize(rect.Width, rect.Height, Inter.Linear);

                         CvInvoke.cvCopy(small, res, small.Split()[3]);
                     }                        
                 }

                 faces.Clear();
             }

             imageBox1.Image = input;
         }*/
    }
}
