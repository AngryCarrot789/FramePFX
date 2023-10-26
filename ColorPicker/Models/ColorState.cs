using System;

namespace ColorPicker.Models {
    public struct ColorState {
        private double _RGB_R;
        private double _RGB_G;
        private double _RGB_B;

        private double _A;

        private double _HSV_H;
        private double _HSV_S;
        private double _HSV_V;

        private double _HSL_H;
        private double _HSL_S;
        private double _HSL_L;

        public ColorState(double rGB_R, double rGB_G, double rGB_B, double a, double hSV_H, double hSV_S, double hSV_V, double hSL_h, double hSL_s, double hSL_l) {
            this._RGB_R = rGB_R;
            this._RGB_G = rGB_G;
            this._RGB_B = rGB_B;
            this._A = a;
            this._HSV_H = hSV_H;
            this._HSV_S = hSV_S;
            this._HSV_V = hSV_V;
            this._HSL_H = hSL_h;
            this._HSL_S = hSL_s;
            this._HSL_L = hSL_l;
        }

        public void SetARGB(double a, double r, double g, double b) {
            this._A = a;
            this._RGB_R = r;
            this._RGB_G = g;
            this._RGB_B = b;
            this.RecalculateHSVFromRGB();
            this.RecalculateHSLFromRGB();
        }

        public double A {
            get => this._A;
            set {
                this._A = value;
            }
        }

        public double RGB_R {
            get => this._RGB_R;
            set {
                this._RGB_R = value;
                this.RecalculateHSVFromRGB();
                this.RecalculateHSLFromRGB();
            }
        }

        public double RGB_G {
            get => this._RGB_G;
            set {
                this._RGB_G = value;
                this.RecalculateHSVFromRGB();
                this.RecalculateHSLFromRGB();
            }
        }

        public double RGB_B {
            get => this._RGB_B;
            set {
                this._RGB_B = value;
                this.RecalculateHSVFromRGB();
                this.RecalculateHSLFromRGB();
            }
        }

        public double HSV_H {
            get => this._HSV_H;
            set {
                this._HSV_H = value;
                this.RecalculateRGBFromHSV();
                this.RecalculateHSLFromHSV();
            }
        }

        public double HSV_S {
            get => this._HSV_S;
            set {
                this._HSV_S = value;
                this.RecalculateRGBFromHSV();
                this.RecalculateHSLFromHSV();
            }
        }

        public double HSV_V {
            get => this._HSV_V;
            set {
                this._HSV_V = value;
                this.RecalculateRGBFromHSV();
                this.RecalculateHSLFromHSV();
            }
        }

        public double HSL_H {
            get => this._HSL_H;
            set {
                this._HSL_H = value;
                this.RecalculateRGBFromHSL();
                this.RecalculateHSVFromHSL();
            }
        }

        public double HSL_S {
            get => this._HSL_S;
            set {
                this._HSL_S = value;
                this.RecalculateRGBFromHSL();
                this.RecalculateHSVFromHSL();
            }
        }

        public double HSL_L {
            get => this._HSL_L;
            set {
                this._HSL_L = value;
                this.RecalculateRGBFromHSL();
                this.RecalculateHSVFromHSL();
            }
        }

        private void RecalculateHSLFromRGB() {
            Tuple<double, double, double> hsltuple = ColorSpaceHelper.RgbToHsl(this._RGB_R, this._RGB_G, this._RGB_B);
            double h = hsltuple.Item1, s = hsltuple.Item2, l = hsltuple.Item3;
            if (h != -1)
                this._HSL_H = h;
            if (s != -1)
                this._HSL_S = s;
            this._HSL_L = l;
        }

        private void RecalculateHSLFromHSV() {
            Tuple<double, double, double> hsltuple = ColorSpaceHelper.HsvToHsl(this._HSV_H, this._HSV_S, this._HSV_V);
            double h = hsltuple.Item1, s = hsltuple.Item2, l = hsltuple.Item3;
            this._HSL_H = h;
            if (s != -1)
                this._HSL_S = s;
            this._HSL_L = l;
        }

        private void RecalculateHSVFromRGB() {
            Tuple<double, double, double> hsvtuple = ColorSpaceHelper.RgbToHsv(this._RGB_R, this._RGB_G, this._RGB_B);
            double h = hsvtuple.Item1, s = hsvtuple.Item2, v = hsvtuple.Item3;
            if (h != -1)
                this._HSV_H = h;
            if (s != -1)
                this._HSV_S = s;
            this._HSV_V = v;
        }

        private void RecalculateHSVFromHSL() {
            Tuple<double, double, double> hsvtuple = ColorSpaceHelper.HslToHsv(this._HSL_H, this._HSL_S, this._HSL_L);
            double h = hsvtuple.Item1, s = hsvtuple.Item2, v = hsvtuple.Item3;
            this._HSV_H = h;
            if (s != -1)
                this._HSV_S = s;
            this._HSV_V = v;
        }

        private void RecalculateRGBFromHSL() {
            Tuple<double, double, double> rgbtuple = ColorSpaceHelper.HslToRgb(this._HSL_H, this._HSL_S, this._HSL_L);
            this._RGB_R = rgbtuple.Item1;
            this._RGB_G = rgbtuple.Item2;
            this._RGB_B = rgbtuple.Item3;
        }

        private void RecalculateRGBFromHSV() {
            Tuple<double, double, double> rgbtuple = ColorSpaceHelper.HsvToRgb(this._HSV_H, this._HSV_S, this._HSV_V);
            this._RGB_R = rgbtuple.Item1;
            this._RGB_G = rgbtuple.Item2;
            this._RGB_B = rgbtuple.Item3;
        }
    }
}