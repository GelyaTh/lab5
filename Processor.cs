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
    class Processor
    {
        private Image<Bgr, byte> srcImg;
        private Tesseract ocr;
        private Mat frame;

        public Processor(Tesseract ocr)
        {
            this.ocr = ocr;
        }

        public Image<Bgr, byte> SrcImg { get => srcImg; set => srcImg = value; }
        public Mat Frame { get => frame; set => frame = value; }

        private List<Rectangle> findInterestAreas()
        {
            List<Rectangle> rois = new List<Rectangle>();
            var gray = srcImg.Convert<Gray, byte>();
            gray._ThresholdBinaryInv(new Gray(100), new Gray(255));
            //расширение
            var dilatedImage = gray.Dilate(4);
            // нахождение контуров
            var contours = new VectorOfVectorOfPoint();
            CvInvoke.FindContours(dilatedImage, contours, null, RetrType.List, ChainApproxMethod.ChainApproxSimple);
            // обход контуров
            for (int i = 0; i < contours.Size; i++)
            {
                // отброс заведомо маленьких контуров
                if (CvInvoke.ContourArea(contours[i], false) > 200)
                {
                    // нахождение ограничивающего прямоугольника
                    Rectangle rect = CvInvoke.BoundingRectangle(contours[i]);
                    rois.Add(rect);
                }
            }
            return rois;
        }

        public Image<Bgr, byte> drawInterestAreas()
        {
            // создание копии исходного изображения
            var copy = srcImg.Copy();
            foreach(Rectangle rect in findInterestAreas())
            {
                // отрисовка прямоугольника
                copy.Draw(rect, new Bgr(Color.Blue), 1);
            }
            return copy;
        }

        public Image<Bgr, byte> getInterestArea(int number)
        {
            List<Rectangle> rois = findInterestAreas();
            srcImg.ROI = rois[getActualROINumber(number, rois.Count)];
            // получение "куска" изображения, содержащего текст
            var roiCopy = srcImg.Copy();
            // сброс области интереса 
            srcImg.ROI = Rectangle.Empty;
            return roiCopy;
        }

        public String getTextFromROI(Image<Bgr, byte> roiImg)
        {
            ocr.SetImage(roiImg); //фрагмент изображения, содержащий текст
            ocr.Recognize(); //распознание текста 
            Tesseract.Character[] words = ocr.GetCharacters(); //получение найденных символов

            StringBuilder strBuilder = new StringBuilder();
            for (int i = 0; i < words.Length; i++)
            {
                strBuilder.Append(words[i].Text);
            }
            return strBuilder.ToString();
        }

        public String getTextFromVideoFrame()
        {
            List<Rectangle> rois = findInterestAreas();
            List<String> frameText = new List<String>();
            foreach(Rectangle roi in rois)
            {
                srcImg.ROI = roi;
                // получение "куска" изображения, содержащего текст
                var roiCopy = srcImg.Copy();
                frameText.Add(getTextFromROI(roiCopy));
                // сброс области интереса 
                srcImg.ROI = Rectangle.Empty;
            }
            StringBuilder stringBuilder = new StringBuilder();
            foreach (String roiText in frameText)
                stringBuilder.Append(roiText);
            return stringBuilder.ToString();
        }

        private int getActualROINumber(int number, int roisListSize)
        {
            return roisListSize * (number/roisListSize+1) - 1 - number;
        }

        private List<Rectangle> findFaces()
        {
            List<Rectangle> faces = new List<Rectangle>();
            using (CascadeClassifier face = new CascadeClassifier("C:\\haarcascades\\haarcascade_frontalface_default.xml"))
            {
                using (Mat ugray = new Mat())
                {
                    CvInvoke.CvtColor(srcImg, ugray, Emgu.CV.CvEnum.ColorConversion.Bgr2Gray);
                    Rectangle[] facesDetected = face.DetectMultiScale(ugray, 1.1, 10, new Size(20, 20));
                    faces.AddRange(facesDetected);
                }
            }
            return faces;
        }

        public Image<Bgra, byte> drawMaskOnFaces()
        {
            Image<Bgra, byte> resImg = srcImg.Copy().Convert<Bgra, byte>();
            foreach (Rectangle rect in findFaces()) //для каждого лица
            {
                resImg.ROI = rect; //для области содержащей лицо
                Image<Bgra, byte> small = frame.ToImage<Bgra, byte>().Resize(rect.Width, rect.Height, Inter.Nearest); //создание                                                                                                            
                //копирование изображения small на изображение res с использованием маски копирования mask
                CvInvoke.cvCopy(small, resImg, small.Split()[3]);
                resImg.ROI = Rectangle.Empty;
            }
            return resImg;
        }

    }
}
