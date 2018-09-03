//The Mark method 
public string Mark(Image<Gray, byte> sourceImage, Image<Gray, byte> modelImage, int x = 100, int y = 300)
        {

            Image<Gray, byte> sourceImage1 = sourceImage.Copy();
            Image<Gray, byte> modelImage1 = modelImage.Copy();

//PyrDown the image to decrease the excuting time

            sourceImage1 = sourceImage1.PyrDown().PyrDown().PyrDown();
            modelImage1 = modelImage1.PyrDown().PyrDown().PyrDown();
            bool isGoodMatch = false;

//Left is the min rotate angle Right is the max rotate angle
            double left = (double)this.numericUpDown1.Value;
            double right = (double)numericUpDown2.Value;
            double acc = 0.1;

//using recursive method to find the best angle which fits the marker best

            double angle = MyApproch(sourceImage1, modelImage1, left, right, acc);
            double socre = Convert.ToDouble(textBox1.Text);
            var rotatedObsered = sourceImage1.Rotate(angle, new Gray(0), true);
            Mat result = new Mat();
            CvInvoke.MatchTemplate(rotatedObsered, modelImage1, result, Emgu.CV.CvEnum.TemplateMatchingType.CcoeffNormed);
            double minValue = 0;
            double maxValue = 0;
            Point bestLoc = new Point();
            Point minLoc = new Point();
            Point maxLoc = new Point();
            CvInvoke.MinMaxLoc(result, ref minValue, ref maxValue, ref minLoc, ref maxLoc);
            bestLoc = maxLoc;

            if (maxValue >= socre)
            {
                isGoodMatch = true;
            }

            if (isGoodMatch)
            {
                maxLoc.X = maxLoc.X * 8;
                maxLoc.Y = maxLoc.Y * 8;
                Point rectOringinal = new Point();
                Point point2 = new Point(maxLoc.X + modelImage.Width, maxLoc.Y);
                Point point3 = new Point(maxLoc.X, maxLoc.Y + modelImage.Height);
                Point point4 = new Point(maxLoc.X + modelImage.Width, maxLoc.Y + modelImage.Height);
                Point rotateCenter = new Point(sourceImage.Width / 2, sourceImage.Height / 2);
                rectOringinal.X = (int)
                    (
                        (maxLoc.X - rotateCenter.X)
                        * Math.Cos((-angle / 180) * Math.PI)
                        - (maxLoc.Y - rotateCenter.Y)
                        * Math.Sin((-angle / 180) * Math.PI) + rotateCenter.X
                    );

//Rotate the Bounding rect
                rectOringinal.Y = (int)((maxLoc.X - rotateCenter.X) * Math.Sin((-angle / 180) * Math.PI) + (maxLoc.Y - rotateCenter.Y) * Math.Cos((-angle / 180) * Math.PI) + rotateCenter.Y);
                point2.X = (int)((point2.X - rotateCenter.X) * Math.Cos((-angle / 180) * Math.PI) - (point2.Y - rotateCenter.Y) * Math.Sin((-angle / 180) * Math.PI) + rotateCenter.X);
                point2.Y = (int)((point2.X - rotateCenter.X) * Math.Sin((-angle / 180) * Math.PI) + (point2.Y - rotateCenter.Y) * Math.Cos((-angle / 180) * Math.PI) + rotateCenter.Y);
                point3.X = (int)((point3.X - rotateCenter.X) * Math.Cos((-angle / 180) * Math.PI) - (point3.Y - rotateCenter.Y) * Math.Sin((-angle / 180) * Math.PI) + rotateCenter.X);
                point3.Y = (int)((point3.X - rotateCenter.X) * Math.Sin((-angle / 180) * Math.PI) + (point3.Y - rotateCenter.Y) * Math.Cos((-angle / 180) * Math.PI) + rotateCenter.Y);
                point4.X = (int)((point4.X - rotateCenter.X) * Math.Cos((-angle / 180) * Math.PI) - (point4.Y - rotateCenter.Y) * Math.Sin((-angle / 180) * Math.PI) + rotateCenter.X);
                point4.Y = (int)((point4.X - rotateCenter.X) * Math.Sin((-angle / 180) * Math.PI) + (point4.Y - rotateCenter.Y) * Math.Cos((-angle / 180) * Math.PI) + rotateCenter.Y);
                PointF accPoint = new PointF();
                accPoint.X =
                       Convert.ToSingle((maxLoc.X + modelImage.Width / 2 - rotateCenter.X)
                       * Math.Cos((-angle / 180) * Math.PI)
                       - (maxLoc.Y + modelImage.Height / 2 - rotateCenter.Y)
                       * Math.Sin((-angle / 180) * Math.PI) + rotateCenter.X
                   );
                accPoint.Y = Convert.ToSingle(((maxLoc.X + modelImage.Width / 2 - rotateCenter.X) * Math.Sin((-angle / 180) * Math.PI) + (maxLoc.Y + modelImage.Height / 2 - rotateCenter.Y) * Math.Cos((-angle / 180) * Math.PI) + rotateCenter.Y));
                maxLoc.X = (int)
                    (
                        (maxLoc.X + modelImage.Width / 2 - rotateCenter.X)
                        * Math.Cos((-angle / 180) * Math.PI)
                        - (maxLoc.Y + modelImage.Height / 2 - rotateCenter.Y)
                        * Math.Sin((-angle / 180) * Math.PI) + rotateCenter.X
                    );

                maxLoc.Y = (int)((maxLoc.X + modelImage.Width / 2 - rotateCenter.X) * Math.Sin((-angle / 180) * Math.PI) + (maxLoc.Y + modelImage.Height / 2 - rotateCenter.Y) * Math.Cos((-angle / 180) * Math.PI) + rotateCenter.Y);

                Rectangle rect = new Rectangle(rectOringinal, modelImage.Size);
                sourceImage.ROI = rect;


                sourceImage.ROI = Rectangle.Empty;

//Draw the lines of the bounding rect
                CvInvoke.Line(imageBox.Image, rectOringinal, point2, new MCvScalar(0, 255, 0, 100), 3);
                CvInvoke.Line(imageBox.Image, point2, point4, new MCvScalar(0, 255, 0, 100), 3);
                CvInvoke.Line(imageBox.Image, point4, point3, new MCvScalar(0, 255, 0, 100), 3);
                CvInvoke.Line(imageBox.Image, point3, rectOringinal, new MCvScalar(0, 255, 0, 100), 3);
                // CvInvoke.Rectangle(imageBox1.Image, rect, new MCvScalar(0, 255, 0, 100), 10);
                CvInvoke.PutText(imageBox.Image, "X:" + (maxLoc.X) + " Y:" + (maxLoc.Y) + " Angle:" + (-angle), new Point(x, y), Emgu.CV.CvEnum.FontFace.HersheyPlain, 5, new MCvScalar(255, 0, 0, 100), 5);
                // imageBox.Image = imageBox.Image;
                PointF[] pointF = new PointF[1];
                pointF[0] = accPoint;
                string a, b, c;

//Tranform pixel coordinate to Robert coordinate
                pointF = CvInvoke.PerspectiveTransform(pointF, homograph);
//Format the position information in ABB Robert format
                if (pointF[0].X >= 0)
                {
                    a = string.Format("{0:+00000.000}", pointF[0].X);
                }
                else
                {
                    a = string.Format("{0:00000.000}", pointF[0].X);
                }
                if (pointF[0].Y >= 0)
                {
                    b = string.Format("{0:+00000.000}", pointF[0].Y);
                }
                else
                {
                    b = string.Format("{0:00000.000}", pointF[0].Y);
                }
                if ((-angle) >= 0)
                {
                    c = string.Format("{0:+00000.000}", (-angle));
                }
                else
                {
                    c = string.Format("{0:00000.000}", (-angle));
                }
                imageBox.Image = imageBox.Image;
                showLog("1," + a + "," + b + "," + c);
                return "1," + a + "," + b + "," + c;

            }
//Haven't found any marker
            else
            {
                CvInvoke.PutText(imageBox.Image, "No Found", new Point(100, 500), Emgu.CV.CvEnum.FontFace.HersheyComplex, 10, new MCvScalar(0, 0, 255, 100), 5);
                imageBox.Image = imageBox.Image;
                return "+00000.000,+00000.000,+00000.000,+00000.000";
            }


        }

//The recursive method finding the best rotate angle
 public static double MyApproch(Image<Gray, byte> sourceImage, Image<Gray, byte> tempImage, double left, double right, double acu)
        {
//the acuration is the exit of the recursive method, whereas the acuraration smaller than the value

            if (Math.Abs(right - left) < acu)
            {
                return myScore(sourceImage, tempImage, left) > myScore(sourceImage, tempImage, right) ? left : right;
            }
            double leftvalue = myScore(sourceImage, tempImage, left);
            double rightvalue = myScore(sourceImage, tempImage, right);
            if (leftvalue > rightvalue)
//if the left score a higher score.
            {//如果左边要高则
                right = (left + right) / 2;
                return MyApproch(sourceImage, tempImage, left, right, acu);
            }
//if the left equals to the right value cut to the middle of the left and right
            else if (leftvalue == rightvalue)
            {//如果左右相等
                right = (left + right) / 2;
                return MyApproch(sourceImage, tempImage, left, right, acu);
            }

if the right scores a higher score.
            else
            {//如果右边要高
                left = (right + left) / 2;
                return MyApproch(sourceImage, tempImage, left, right, acu);
            }
        }

//Jude what score at the angle scores
        private static double myScore(Image<Gray, byte> sourceImage, Image<Gray, byte> tempImage, double angle)
        {
            var temp = sourceImage.Rotate(angle, new Gray(0), true);
            Mat result = new Mat();
            CvInvoke.MatchTemplate(temp, tempImage, result, Emgu.CV.CvEnum.TemplateMatchingType.CcoeffNormed, null);

            double min = 0, max = 0;
            Point loc = new Point();
            Point LOC = new Point();
            CvInvoke.MinMaxLoc(result, ref min, ref max, ref loc, ref LOC, null);
            return max;
        }
