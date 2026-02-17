using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Monocle;
namespace Celeste.Mod.DBBHelper {
    /// <summary>
    /// DBB数学库，提供常见插值，颜色转换等功能
    /// </summary>
    public class DBBMath
    {
        public static readonly float MachineEpsilonFloat = GetMachineEpsilonFloat();
        /// <summary>
        /// <para>基于输入参数time返回归一化线性插值结果，返回结果始终在[start,end]区间</para>
        /// <para>即使time小于0.0或者time大于1.0，函数也会强制限制返回结果在给定区间内</para>
        /// </summary>
        /// <returns>归一化线性插值结果(double)</returns>
        public static double Linear_Lerp(double time, double start, double end)
        {
            double result = start * (1.0 - time) + end * time;
            double min = Math.Min(start, end);
            double max = Math.Max(start, end);
            if (result <= min)
            {
                return min;
            }
            if (result >= max)
            {
                return max;
            }
            return result;
        }
        /// <summary>
        /// <para>基于输入参数time返回归一化线性插值结果，返回结果始终在start和end所连线段之间</para>
        /// <para>即使time小于0.0或者time大于1.0，函数也会强制限制返回结果在给定区间内</para>
        /// </summary>
        /// <returns>归一化线性插值点的坐标(Vector2)</returns>
        public static Vector2 Linear_Lerp(double time, Vector2 start, Vector2 end)
        {
            double x = Linear_Lerp(time, start.X, end.X);
            double y = Linear_Lerp(time, start.Y, end.Y);
            return new Vector2((float)x, (float)y);
        }
        /// <summary>
        /// <para>基于输入参数time返回归一化线性插值结果，返回结果始终在start和end所连线段之间</para>
        /// <para>即使time小于0.0或者time大于1.0，函数也会强制限制返回结果在给定区间内</para>
        /// </summary>
        /// <returns>归一化线性插值点的坐标(Vector3)</returns>
        public static Vector3 Linear_Lerp(double time, Vector3 start, Vector3 end)
        {
            double x = Linear_Lerp(time, start.X, end.X);
            double y = Linear_Lerp(time, start.Y, end.Y);
            double z = Linear_Lerp(time, start.Z, end.Z);
            return new Vector3((float)x, (float)y, (float)z);
        }
        /// <summary>
        /// <para>基于输入参数time返回归一化线性插值结果，返回结果始终在start和end所连线段之间</para>
        /// <para>即使time小于0.0或者time大于1.0，函数也会强制限制返回结果在给定区间内</para>
        /// </summary>
        /// <returns>归一化线性插值点的坐标(Vector4)</returns>
        public static Vector4 Linear_Lerp(double time, Vector4 start, Vector4 end)
        {
            double x = Linear_Lerp(time, start.X, end.X);
            double y = Linear_Lerp(time, start.Y, end.Y);
            double z = Linear_Lerp(time, start.Z, end.Z);
            double w = Linear_Lerp(time, start.W, end.W);
            return new Vector4((float)x, (float)y, (float)z, (float)w);
        }
        /// <summary>
        /// <para>基于输入参数time返回归一化三次贝塞尔插值点的坐标</para>
        /// <para>time的值一定位于[0.0,1.0]之间，插值点的X坐标一定位于[0.0,1.0]之间，Y坐标可以为任意值</para>
        /// <para>该函数使用了四个点(0,0)、point1、point2、(1,1)来控制贝塞尔曲线，并使用time作为插值因子来确定应获取曲线上的哪个点</para>
        /// </summary>
        /// <returns>归一化贝塞尔插值点的坐标(Vector2)</returns>
        public static Vector2 Beizer_Lerp(double time, Vector2 point1, Vector2 point2)
        {
            double per = Math.Clamp(time, 0.0, 1.0);
            double p1x = Math.Clamp(point1.X, 0.0, 1.0);
            double p2x = Math.Clamp(point2.X, 0.0, 1.0);
            double x = 3.0 * p1x * per * Math.Pow(1.0 - per, 2) + 3.0 * p2x * Math.Pow(per, 2) * (1.0 - per) + Math.Pow(per, 3);
            double y = 3.0 * point1.Y * per * Math.Pow(1.0 - per, 2) + 3.0 * point1.Y * Math.Pow(per, 2) * (1.0 - per) + Math.Pow(per, 3);
            return new Vector2((float)x, (float)y);
        }
        /// <summary>
        /// <para>基于输入参数time返回归一化三次贝塞尔函数值progress，其中(time,progress)是贝塞尔曲线上的横纵坐标</para>
        /// <para>该函数使用了四个点(0,0)、point1、point2、(1,1)来控制贝塞尔曲线的生成，time的值一定位于[0.0,1.0]之间</para>
        /// <para>函数使用牛顿迭代法来求数值解，iter控制迭代次数，默认为6</para>
        /// </summary>
        /// <returns>归一化三次贝塞尔曲线函数值(double)</returns>
        /// 这个函数目前还没有验证正确性，目测是有问题，不要使用
        public static double Beizer_ExplicitLerp(double time, Vector2 point1, Vector2 point2, int iteration = 6)
        {
            double x = Math.Clamp(time, 0.0, 1.0);
            double p1x = Math.Clamp(point1.X, 0.0, 1.0);
            double p2x = Math.Clamp(point2.X, 0.0, 1.0);
            double t_n = 0.5;
            //牛顿迭代，默认迭代次数为6
            for (int iter = 0; iter < iteration; iter++)
            {
                //分母
                double denominator = (9.0 * (p1x - p2x) + 3.0) * Math.Pow(t_n, 2) + 6.0 * (p2x - 2.0 * p1x) * t_n + 3 * p1x;
                //分子
                double molecule = 3.0 * p1x * t_n * Math.Pow(1.0 - t_n, 2) + 3.0 * p2x * Math.Pow(t_n, 2) * (1.0 - t_n) + Math.Pow(t_n, 3) - x;
                //牛顿法失效的时候使用二分法
                if (denominator == 0.0)
                {
                    if (molecule < 0.0) { t_n = 0.5 * t_n; }
                    else { t_n = (1.0 + t_n) / 2.0; }
                }
                else
                {
                    t_n = t_n - molecule / denominator;
                }

            }
            return 3.0 * point1.Y * t_n * Math.Pow(1.0 - t_n, 2) + 3.0 * point1.Y * Math.Pow(t_n, 2) * (1.0 - t_n) + Math.Pow(t_n, 3);
        }
        /// <summary>
        /// <para>基于输入参数t返回easeInSin映射后的结果</para>
        /// <para>输入参数t应在0.0到1.0之间，当输入超出该区间范围时会被强制截断到给定区间内</para>
        /// </summary>
        /// <returns>easeInSin映射后的结果(double)</returns>
        public static double easeInSin(double t)
        {
            double time = Math.Clamp(t, 0.0, 1.0);
            return 1.0 - Math.Cos((time * Math.PI) / 2.0f);
        }
        /// <summary>
        /// <para>基于输入参数t返回easeOutSin映射后的结果</para>
        /// <para>输入参数t应在0.0到1.0之间，当输入超出该区间范围时会被强制截断到给定区间内</para>
        /// </summary>
        /// <returns>easeOutSin映射后的结果(double)</returns>
        public static double easeOutSin(double t)
        {
            double time = Math.Clamp(t, 0.0, 1.0);
            return Math.Sin((time * Math.PI) / 2.0f);
        }
        /// <summary>
        /// <para>基于输入参数t返回easeInOutSin映射后的结果</para>
        /// <para>输入参数t应在0.0到1.0之间，当输入超出该区间范围时会被强制截断到给定区间内</para>
        /// </summary>
        /// <returns>easeInOutSin映射后的结果(double)</returns>
        public static double easeInOutSin(double t)
        {
            double time = Math.Clamp(t, 0.0, 1.0);
            return -(Math.Cos(Math.PI * time) - 1.0f) / 2.0f;
        }
        /// <summary>
        /// <para>基于输入参数t返回easeInCubic映射后的结果</para>
        /// <para>输入参数t应在0.0到1.0之间，当输入超出该区间范围时会被强制截断到给定区间内</para>
        /// </summary>
        /// <returns>easeInCubic映射后的结果(double)</returns>
        public static double easeInCubic(double t)
        {
            double time = Math.Clamp(t, 0.0, 1.0);
            return Math.Pow(time, 3);
        }
        /// <summary>
        /// <para>基于输入参数t返回easeOutCubic映射后的结果</para>
        /// <para>输入参数t应在0.0到1.0之间，当输入超出该区间范围时会被强制截断到给定区间内</para>
        /// </summary>
        /// <returns>easeOutCubic映射后的结果(double)</returns>
        public static double easeOutCubic(double t)
        {
            double time = Math.Clamp(t, 0.0, 1.0);
            return 1 - Math.Pow(1.0 - time, 3);
        }
        /// <summary>
        /// <para>基于输入参数t返回easeOutCubic映射后的结果</para>
        /// <para>输入参数t应在0.0到1.0之间，当输入超出该区间范围时会被强制截断到给定区间内</para>
        /// </summary>
        /// <returns>easeOutCubic映射后的结果(double)</returns>
        public static double easeInOutCubic(double t)
        {
            double time = Math.Clamp(t, 0.0, 1.0);
            return time < 0.5 ? 4.0 * Math.Pow(time, 3) : 1.0 - Math.Pow(-2.0 * time + 2.0, 3) / 2.0;
        }
        /// <summary>
        /// <para>基于输入参数t返回easeInQuard映射后的结果</para>
        /// <para>输入参数t应在0.0到1.0之间，当输入超出该区间范围时会被强制截断到给定区间内</para>
        /// </summary>
        /// <returns>easeInQuard映射后的结果(double)</returns>
        public static double easeInQuard(double t)
        {
            double time = Math.Clamp(t, 0.0, 1.0);
            return Math.Pow(time, 4);
        }
        /// <summary>
        /// <para>基于输入参数t返回easeOutQuard映射后的结果</para>
        /// <para>输入参数t应在0.0到1.0之间，当输入超出该区间范围时会被强制截断到给定区间内</para>
        /// </summary>
        /// <returns>easeOutQuard映射后的结果(double)</returns>
        public static double easeOutQuard(double t)
        {
            double time = Math.Clamp(t, 0.0, 1.0);
            return 1.0 - Math.Pow(1.0 - time, 4);
        }
        /// <summary>
        /// <para>基于输入参数t返回easeInOutQuard映射后的结果</para>
        /// <para>输入参数t应在0.0到1.0之间，当输入超出该区间范围时会被强制截断到给定区间内</para>
        /// </summary>
        /// <returns>easeInOutQuard映射后的结果(double)</returns>
        public static double easeInOutQuard(double t)
        {
            double time = Math.Clamp(t, 0.0, 1.0);
            return time < 0.5 ? 8.0f * Math.Pow(time, 4) : 1.0 - Math.Pow(-2.0 * time + 2.0, 4) / 2.0;
        }
        /// <summary>
        /// <para>基于输入参数t返回easeInBack映射后的结果</para>
        /// <para>输入参数t应在0.0到1.0之间，当输入超出该区间范围时会被强制截断到给定区间内</para>
        /// </summary>
        /// <returns>easeInBack映射后的结果(double)</returns>
        public static double easeInBack(double t)
        {
            double time = Math.Clamp(t, 0.0, 1.0);
            return 2.70158 * Math.Pow(time, 3) - 1.70158 * Math.Pow(time, 2);
        }
        /// <summary>
        /// <para>基于输入参数t返回easeOutBack映射后的结果</para>
        /// <para>输入参数t应在0.0到1.0之间，当输入超出该区间范围时会被强制截断到给定区间内</para>
        /// </summary>
        /// <returns>easeOutBack映射后的结果(double)</returns>
        public static double easeOutBack(double t)
        {
            double time = Math.Clamp(t, 0.0, 1.0);
            return 1.0 + 2.70158 * Math.Pow(time - 1.0, 3) + 1.70158 * Math.Pow(time - 1.0, 2);
        }
        /// <summary>
        /// <para>基于输入参数t返回easeInOutBack映射后的结果</para>
        /// <para>输入参数t应在0.0到1.0之间，当输入超出该区间范围时会被强制截断到给定区间内</para>
        /// </summary>
        /// <returns>easeInOutBack映射后的结果(double)</returns>
        public static double easeInOutBack(double t)
        {
            double time = Math.Clamp(t, 0.0, 1.0);
            return time < 0.5 ? Math.Pow(time * 2.0, 2) * (7.18982 * time - 2.59491) / 2.0
                        : (Math.Pow(time * 2.0 - 2.0, 2) * (3.59491 * (time * 2.0 - 2.0) + 3.59491) + 2.0) / 2.0;
        }
        /// <summary>
        /// 向量归一化
        /// </summary>
        /// <returns>归一化向量(Vector2)</returns>
        public static Vector2 Normalize(Vector2 vec)
        {
            return Calc.SafeNormalize(vec);
        }
        /// <summary>
        /// 对一个点进行旋转，向量将以原点为origin的局部坐标系进行旋转
        /// <para>origin为旋转原点，point为待旋转点</para>
        /// <para>theta为旋转角度，默认逆时针为旋转的正方向</para>
        /// <para>当angle_mode为true时为角度制，否则为弧度制</para>
        /// </summary>
        /// <returns>旋转后的点(Vector2)</returns>
        public static Vector2 Rotate(Vector2 origin, Vector2 point, float theta, bool angle_mode = true)
        {
            float rad_theta = theta;
            if (angle_mode == true)
            {
                rad_theta = theta / 180.0f * (float)Math.PI;
            }
            Vector2 temp_point = point - origin;
            float x = (float)(Math.Cos(rad_theta) * temp_point.X - Math.Sin(rad_theta) * temp_point.Y);
            float y = (float)(Math.Sin(rad_theta) * temp_point.X + Math.Cos(rad_theta) * temp_point.Y);
            Vector2 new_point = new Vector2(x, y) + origin;
            return new_point;
        }
        /// <summary>
        /// 返回两个向量的叉积的结果
        /// <para>严格而言，二维向量没有叉积，实际上称作两个向量的外积的行列式比较好</para>
        /// <para>不过好像大部分人都喜欢直接叫这个东西叉积，所以我就这么写了</para>
        /// </summary>
        /// <returns>叉积的结果(float)</returns>
        public static float Cross(Vector2 vec1, Vector2 vec2)
        {
            return vec1.X * vec2.Y - vec2.X * vec1.Y;
        }
        /// <summary>
        /// <para>运动映射，将时间值t映射为指定的比例值，style(string)用于控制不同的映射方式</para>
        /// <para>映射值将基于style的值，通过不同的插值函数得到</para>
        /// <para>时间值t一定在[0.0,1.0]区间内，任何小于0.0的t值都会被0.0替代，任何大于1.0的t值都会被1.0替代</para>
        /// <para>映射方式有:</para>
        /// <para>线性缓动：Linear</para>
        /// <para>正弦缓动：easeInSin、easeOutSin、easeInOutSin</para>
        /// <para>三次缓动：easeInCubic、easeOutCubic、easeInOutCubic</para>
        /// <para>四次缓动：easeInQuard、easeOutQuard、easeInOutQuard</para>
        /// <para>回弹缓动：easeInBack、easeOutBack、easeInOutBack</para>
        /// </summary>
        /// <returns>映射后的比例值(float)</returns>
        public static float MotionMapping(float t, string style)
        {
            double time = Math.Clamp(t, 0.0, 1.0);
            double mapping_value = 0.0f;
            switch (style)
            {
                case "Linear": mapping_value = DBBMath.Linear_Lerp(time, 0.0, 1.0); break;
                case "easeInSin": mapping_value = DBBMath.easeInSin(time); break;
                case "easeOutSin": mapping_value = DBBMath.easeOutSin(time); break;
                case "easeInOutSin": mapping_value = DBBMath.easeInOutSin(time); break;
                case "easeInCubic": mapping_value = DBBMath.easeInCubic(time); break;
                case "easeOutCubic": mapping_value = DBBMath.easeOutCubic(time); break;
                case "easeInOutCubic": mapping_value = DBBMath.easeInOutCubic(time); break;
                case "easeInQuard": mapping_value = DBBMath.easeInQuard(time); break;
                case "easeOutQuard": mapping_value = DBBMath.easeOutQuard(time); break;
                case "easeInOutQuard": mapping_value = DBBMath.easeInOutQuard(time); break;
                case "easeInBack": mapping_value = DBBMath.easeInBack(time); break;
                case "easeOutBack": mapping_value = DBBMath.easeOutBack(time); break;
                case "easeInOutBack": mapping_value = DBBMath.easeInOutBack(time); break;
                default: break;
            }
            return (float)mapping_value;
        }
        /// <summary>
        /// 将十六进制颜色字符串(例如"FFFFFFFF")转化成归一化的Vector4的形式，如果字符串非法则返回(1,1,1,1)
        /// </summary>
        public static Vector4 ConvertColor(string color)
        {
            Color tmp_color = Calc.HexToColor(color);
            return new Vector4(tmp_color.R / 255.0f, tmp_color.G / 255.0f, tmp_color.B / 255.0f, tmp_color.A / 255.0f);
        }
        /// <summary>
        /// <para>将归一化的Vector4向量转化成十六进制颜色字符串(例如"FFFFFFFF")的形式</para>
        /// <para>需要注意，字符串采用大端法，例如，对于向量(0.5 ,0.6 ,0.7 ,0.8 )有：</para>
        /// <para>R：result[0]=7 result[1]=F</para>
        /// <para>G：result[2]=9 result[3]=9</para>
        /// <para>B：result[4]=B result[5]=2</para>
        /// <para>A：result[6]=C result[7]=C</para>
        /// <para>normalized_color: 归一化的Vector4向量，其所有分量的范围均应在0到1之间，超出该范围的值将被截断到指定区间</para>
        /// <para>alpha_reversed: 是否保留A通道</para>
        /// </summary>
        /// <returns>十六进制颜色字符串，字符串采取大端法</returns>
        public static string ConvertColor(Vector4 normalized_color, bool alpha_reversed)
        {
            string r = ((int)(255.0f * Math.Clamp(normalized_color.X, 0.0f, 1.0f))).ToString("X2");
            string g = ((int)(255.0f * Math.Clamp(normalized_color.Y, 0.0f, 1.0f))).ToString("X2");
            string b = ((int)(255.0f * Math.Clamp(normalized_color.Z, 0.0f, 1.0f))).ToString("X2");
            string a = ((int)(255.0f * Math.Clamp(normalized_color.W, 0.0f, 1.0f))).ToString("X2");
            string result = r + g + b;
            if (alpha_reversed)
            {
                result += a;
            }
            return result;
        }
        /// <summary>
        /// <para>将HSL值转化为RGB颜色</para>
        /// <para>hue：色相值，取值应为0.0到1.0，当输入超出该区间范围时会被强制截断到给定区间内</para>
        /// <para>saturation：饱和度，取值应为0.0到1.0，当输入超出该区间范围时会被强制截断到给定区间内</para>
        /// <para>value：明度，取值应为0.0到1.0，当输入超出该区间范围时会被强制截断到给定区间内</para>
        /// </summary>
        /// <returns>转化后的颜色(Color)</returns>
        public static Color HSVtoRGB(float hue, float saturation, float value)
        {
            // 确保输入在合法范围内
            float clampedHue = Math.Clamp(hue, 0.0f, 1.0f);
            float clampedSaturation = Math.Clamp(saturation, 0.0f, 1.0f);
            float clampedValue = Math.Clamp(value, 0.0f, 1.0f);

            float c = clampedValue * clampedSaturation;
            float x = c * (1 - Math.Abs((clampedHue * 6) % 2 - 1));
            float m = clampedValue - c;
            float r, g, b;
            if (clampedHue < 1.0f / 6.0f){r = c;g = x;b = 0;}
            else if (clampedHue < 2.0f / 6.0f){r = x;g = c;b = 0;}
            else if (clampedHue < 3.0f / 6.0f){r = 0;g = c;b = x;}
            else if (clampedHue < 4.0f / 6.0f){r = 0;g = x;b = c;}
            else if (clampedHue < 5.0f / 6.0f){r = x;g = 0;b = c;}
            else{r = c;g = 0;b = x;}
            return new Color(r + m, g + m, b + m, 1.0f);
        }
        /// <summary>
        /// <para>将HSL值转化为RGB颜色</para>
        /// <para>hue：色相值，取值应为0.0到1.0，当输入超出该区间范围时会被强制截断到给定区间内</para>
        /// <para>saturation：饱和度，取值应为0.0到1.0，当输入超出该区间范围时会被强制截断到给定区间内</para>
        /// <para>value：明度，取值应为0.0到1.0，当输入超出该区间范围时会被强制截断到给定区间内</para>
        /// </summary>
        /// <returns>转化后的归一化颜色向量，不含A通道(Vector3)</returns>
        public static Vector3 HSVtoRGB_Normalized(float hue, float saturation, float value)
        {
            // 确保输入在合法范围内
            float clampedHue = Math.Clamp(hue, 0.0f, 1.0f);
            float clampedSaturation = Math.Clamp(saturation, 0.0f, 1.0f);
            float clampedValue = Math.Clamp(value, 0.0f, 1.0f);

            float c = clampedValue * clampedSaturation;
            float x = c * (1 - Math.Abs((clampedHue * 6) % 2 - 1));
            float m = clampedValue - c;
            float r, g, b;
            if (clampedHue < 1.0f / 6.0f){r = c;g = x;b = 0;}
            else if (clampedHue < 2.0f / 6.0f){r = x;g = c;b = 0;}
            else if (clampedHue < 3.0f / 6.0f){r = 0;g = c;b = x;}
            else if (clampedHue < 4.0f / 6.0f){r = 0;g = x;b = c;}
            else if (clampedHue < 5.0f / 6.0f){r = x;g = 0;b = c;}
            else{r = c;g = 0;b = x;}
            return new Vector3(r + m, g + m, b + m);
        }

        /// <summary>
        /// <para>将归一化的RGB值转化为HSV值</para>
        /// <para>r：红色值，取值应为0.0到1.0，当输入超出该区间范围时会被强制截断到给定区间内</para>
        /// <para>g：绿色值，取值应为0.0到1.0，当输入超出该区间范围时会被强制截断到给定区间内</para>
        /// <para>b：蓝色值，取值应为0.0到1.0，当输入超出该区间范围时会被强制截断到给定区间内</para>
        /// </summary>
        /// <returns>转化后的归一化HSV值(Vector3)</returns>
        public static Vector3 RGBtoHSV(float r, float g, float b)
        {
            // 确保输入在合法范围内
            float clampedR = Math.Clamp(r, 0.0f, 1.0f);
            float clampedG = Math.Clamp(g, 0.0f, 1.0f);
            float clampedB = Math.Clamp(b, 0.0f, 1.0f);

            // 计算最大值、最小值和差值
            float max = Math.Max(clampedR, Math.Max(clampedG, clampedB));
            float min = Math.Min(clampedR, Math.Min(clampedG, clampedB));
            float delta = max - min;

            // 初始化 HSV 值
            float h = 0f;
            float s = 0f;
            float v = max; // HSV 的 V 直接取最大值

            // 如果不是灰度色（delta != 0）
            if (delta > 0.0001f) // 使用小的容差值避免浮点误差
            {
                // 计算饱和度（HSV 的 S = delta / max）
                s = delta / max;

                // 计算色相（和 HSL 相同）
                if (Math.Abs(max - clampedR) < 0.0001f)
                {
                    h = ((clampedG - clampedB) / delta) % 6f;
                }
                else if (Math.Abs(max - clampedG) < 0.0001f)
                {
                    h = 2f + (clampedB - clampedR) / delta;
                }
                else
                {
                    h = 4f + (clampedR - clampedG) / delta;
                }

                // 将色相转换为 0-1 范围
                h = (h * 60f) % 360f;
                if (h < 0) { h += 360f; }
                h /= 360f; // 归一化到 0-1
            }

            return new Vector3(h, s, v);
        }

        /// <summary>
        /// <para>获取向量( Max(a.X,b.X) , Max(a.Y,b.Y) )</para>
        /// </summary>
        /// <returns>向量(Vector2)</returns>
        public static Vector2 Max(Vector2 a, Vector2 b)
        {
            return new Vector2(Math.Max(a.X, b.X), Math.Max(a.Y, b.Y));
        }

        /// <summary>
        /// <para>对value的各分量依据min和max进行截断，并返回各分量拼接成的向量</para>
        /// <para>对于value的各分量I而言，如果value.I小于min.I则返回min.I，如果value.I大于max.I则返回max.I，如果value.I小于max.I且大于min.I则返回value.I</para>
        /// </summary>
        public static Vector2 Clamp(Vector2 value, Vector2 min, Vector2 max)
        {
            float x = Math.Clamp(value.X, min.X, max.X);
            float y = Math.Clamp(value.Y, min.Y, max.Y);
            return new Vector2(x, y);
        }
        /// <summary>
        /// <para>对value的各分量依据min和max进行截断，并返回各分量拼接成的向量</para>
        /// <para>对于value的各分量I而言，如果value.I小于min.I则返回min.I，如果value.I大于max.I则返回max.I，如果value.I小于max.I且大于min.I则返回value.I</para>
        /// </summary>
        public static Vector3 Clamp(Vector3 value, Vector3 min, Vector3 max)
        {
            float x = Math.Clamp(value.X, min.X, max.X);
            float y = Math.Clamp(value.Y, min.Y, max.Y);
            float z = Math.Clamp(value.Z, min.Z, max.Z);
            return new Vector3(x, y, z);
        }
        /// <summary>
        /// <para>对value的各分量依据min和max进行截断，并返回各分量拼接成的向量</para>
        /// <para>对于value的各分量I而言，如果value.I小于min.I则返回min.I，如果value.I大于max.I则返回max.I，如果value.I小于max.I且大于min.I则返回value.I</para>
        /// </summary>
        public static Vector4 Clamp(Vector4 value, Vector4 min, Vector4 max)
        {
            float x = Math.Clamp(value.X, min.X, max.X);
            float y = Math.Clamp(value.Y, min.Y, max.Y);
            float z = Math.Clamp(value.Z, min.Z, max.Z);
            float w = Math.Clamp(value.W, min.W, max.W);
            return new Vector4(x, y, z, w);
        }

        /// <summary>
        /// <para>设有安全保护的SmoothStep插值</para>
        /// <para>将[start,end]里的time根据单调增的三次函数映射为[0,1]里的值，当time超出[start,end]时会被截断</para>
        /// <para>当end与start的距离小于0.0001f时将会返回1.0f</para>
        /// </summary>
        public static float SmoothStep(float start, float end, float time)
        {
            if (Math.Abs(end - start) <= 0.0001f)
            {
                return 1.0f;
            }
            float x = Math.Clamp((time - start) / (end - start), 0.0f, 1.0f);
            return x * x * (3.0f - 2.0f * x);
        }
        /// <summary>
        /// 返回a对n的取模操作，该操作是数学上的取模操作
        /// <para>当n不为0.0f时，返回a mod n，当n为0时，返回0.0f</para>
        /// </summary>
        public static float Mod(float a, float n)
        {
            if (n == 0.0f)
            {
                return 0.0f;
            }
            return (a % n + n) % n;
        }

        /// <summary>
        /// 进行一次Verlet积分迭代，基于当前值和上一次的值，返回下一个值
        /// <para>current_value为当前值，last_value为上一次的值</para>
        /// <para>delta_t为时间步长，acceleration为加速度</para>
        /// </summary>
        public static float Verlet_Integrate(float current_value, float last_value, float delta_t, float acceleration)
        {
            return 2.0f * current_value - last_value + acceleration * delta_t * delta_t;
        }
        /// <summary>
        /// 进行一次Verlet积分迭代，基于当前向量和上一次的向量，返回下一个向量
        /// <para>current_value为当前向量，last_value为上一次的向量</para>
        /// <para>delta_t为时间步长，acceleration为加速度向量</para>
        /// </summary>
        public static Vector2 Verlet_Integrate(Vector2 current_value, Vector2 last_value, float delta_t, Vector2 acceleration)
        {
            return 2.0f * current_value - last_value + acceleration * delta_t * delta_t;
        }

        /// <summary>
        /// 获取机器最小浮点数
        /// </summary>
        private static float GetMachineEpsilonFloat()
        {
            float num = 1f;
            float num2;
            do
            {
                num *= 0.5f;
                num2 = 1f + num;
            }
            while (num2 > 1f);
            return num;
        }
    }
    
}