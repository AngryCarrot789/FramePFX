﻿//  
// Copyright (c) Xavier CLEMENCE (xavier.clemence@gmail.com) and REghZy/AngryCarrot789. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information. 
// Ruler Wpf Version 3.1
// 

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using FramePFX.Editors.Controls.Rulers.Rulers;

namespace FramePFX.Editors.Controls.Rulers {
    public abstract class RulerBase : FrameworkElement {
        public static readonly DependencyProperty MaxValueProperty = DependencyProperty.Register(nameof(MaxValue), typeof(double), typeof(RulerBase), new FrameworkPropertyMetadata(double.NaN, FrameworkPropertyMetadataOptions.AffectsRender));
        public static readonly DependencyProperty RulerPositionProperty = DependencyProperty.Register(nameof(RulerPosition), typeof(RulerPosition), typeof(RulerBase), new FrameworkPropertyMetadata(RulerPosition.Top, (d, e) => ((RulerBase) d).UpdateRulerPosition((RulerPosition) e.NewValue)));
        public static readonly DependencyProperty MajorStepValuesProperty = DependencyProperty.Register(nameof(MajorStepValues), typeof(IEnumerable<int>), typeof(RulerBase), new FrameworkPropertyMetadata(new int[] {1, 2, 5}, FrameworkPropertyMetadataOptions.AffectsRender));
        public static readonly DependencyProperty MinPixelSizeProperty = DependencyProperty.Register(nameof(MinPixelSize), typeof(int), typeof(RulerBase), new FrameworkPropertyMetadata(4, FrameworkPropertyMetadataOptions.AffectsRender));
        public static readonly DependencyProperty ValueStepTransformProperty = DependencyProperty.Register(nameof(ValueStepTransform), typeof(Func<double, double>), typeof(RulerBase), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));
        public static readonly DependencyProperty StepColorProperty = DependencyProperty.Register(nameof(StepColor), typeof(Brush), typeof(RulerBase), new FrameworkPropertyMetadata(Brushes.DimGray, FrameworkPropertyMetadataOptions.AffectsRender, (d, e) => ((RulerBase) d).majorLineStepColourPen = null));
        public static readonly DependencyProperty MinorStepRatioProperty = DependencyProperty.Register(nameof(MinorStepRatio), typeof(double), typeof(RulerBase), new FrameworkPropertyMetadata(0.33, FrameworkPropertyMetadataOptions.AffectsRender));
        public static readonly DependencyProperty DisplayZeroLineProperty = DependencyProperty.Register(nameof(DisplayZeroLine), typeof(bool), typeof(RulerBase), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender));
        public static readonly DependencyProperty StepPropertiesProperty = DependencyProperty.Register(nameof(StepProperties), typeof(RulerStepProperties), typeof(RulerBase), new FrameworkPropertyMetadata(null));
        public static readonly DependencyProperty SlaveStepPropertiesProperty = DependencyProperty.Register(nameof(SlaveStepProperties), typeof(RulerStepProperties), typeof(RulerBase), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));
        public static readonly DependencyProperty TextFormatProperty = DependencyProperty.Register(nameof(TextFormat), typeof(string), typeof(RulerBase), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));
        public static readonly DependencyProperty MajorLineSizeProperty = DependencyProperty.Register(nameof(MajorLineSize), typeof(double?), typeof(RulerBase), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));
        public static readonly DependencyProperty TopRulerLineAlignmentProperty = DependencyProperty.Register(nameof(TopRulerLineAlignment), typeof(VerticalAlignment), typeof(RulerBase), new FrameworkPropertyMetadata(VerticalAlignment.Bottom, FrameworkPropertyMetadataOptions.AffectsRender));
        public static readonly DependencyProperty LeftRulerLineAlignmentProperty = DependencyProperty.Register(nameof(LeftRulerLineAlignment), typeof(HorizontalAlignment), typeof(RulerBase), new FrameworkPropertyMetadata(HorizontalAlignment.Right, FrameworkPropertyMetadataOptions.AffectsRender));
        public static readonly DependencyProperty MajorLineThicknessProperty = DependencyProperty.Register("MajorLineThickness", typeof(double), typeof(RulerBase), new FrameworkPropertyMetadata(1d, FrameworkPropertyMetadataOptions.AffectsRender));
        public static readonly DependencyProperty MinorLineThicknessProperty = DependencyProperty.Register("MinorLineThickness", typeof(double), typeof(RulerBase), new FrameworkPropertyMetadata(0.5d, FrameworkPropertyMetadataOptions.AffectsRender));
        public static readonly DependencyProperty BackgroundProperty = Panel.BackgroundProperty.AddOwner(typeof(RulerBase), new FrameworkPropertyMetadata(Panel.BackgroundProperty.DefaultMetadata.DefaultValue, FrameworkPropertyMetadataOptions.None));
        public static readonly DependencyProperty FontFamilyProperty = TextElement.FontFamilyProperty.AddOwner(typeof(RulerBase), new FrameworkPropertyMetadata(SystemFonts.MessageFontFamily, (d, e) => ((RulerBase) d).CachedTypeFace = null));
        public static readonly DependencyProperty ForegroundProperty = TextElement.ForegroundProperty.AddOwner(typeof(RulerBase), new FrameworkPropertyMetadata(SystemColors.ControlTextBrush));

        private Pen majorLineStepColourPen;
        private Pen minorLineStepColourPen;

        public double MaxValue {
            get => (double) this.GetValue(MaxValueProperty);
            set => this.SetValue(MaxValueProperty, value);
        }

        public RulerPosition RulerPosition {
            get => (RulerPosition) this.GetValue(RulerPositionProperty);
            set => this.SetValue(RulerPositionProperty, value);
        }

        public IEnumerable<int> MajorStepValues {
            get => (IEnumerable<int>) this.GetValue(MajorStepValuesProperty);
            set => this.SetValue(MajorStepValuesProperty, value);
        }

        public int MinPixelSize {
            get => (int) this.GetValue(MinPixelSizeProperty);
            set => this.SetValue(MinPixelSizeProperty, value);
        }

        public Func<double, double> ValueStepTransform {
            get => (Func<double, double>) this.GetValue(ValueStepTransformProperty);
            set => this.SetValue(ValueStepTransformProperty, value);
        }

        public Brush StepColor {
            get => (Brush) this.GetValue(StepColorProperty);
            set => this.SetValue(StepColorProperty, value);
        }

        public Pen MajorStepColourPen => this.majorLineStepColourPen ?? (this.StepColor is Brush brush ? this.majorLineStepColourPen = new Pen(brush, this.MajorLineThickness) : null);
        public Pen MinorStepColourPen => this.minorLineStepColourPen ?? (this.StepColor is Brush brush ? this.minorLineStepColourPen = new Pen(brush, this.MinorLineThickness) : null);

        public double MinorStepRatio {
            get => (double) this.GetValue(MinorStepRatioProperty);
            set => this.SetValue(MinorStepRatioProperty, value);
        }

        public bool DisplayZeroLine {
            get => (bool) this.GetValue(DisplayZeroLineProperty);
            set => this.SetValue(DisplayZeroLineProperty, value);
        }

        public RulerStepProperties StepProperties {
            get => (RulerStepProperties) this.GetValue(StepPropertiesProperty);
            set => this.SetValue(StepPropertiesProperty, value);
        }

        public RulerStepProperties SlaveStepProperties {
            get => (RulerStepProperties) this.GetValue(SlaveStepPropertiesProperty);
            set => this.SetValue(SlaveStepPropertiesProperty, value);
        }

        public string TextFormat {
            get => (string) this.GetValue(TextFormatProperty);
            set => this.SetValue(TextFormatProperty, value);
        }

        public double? MajorLineSize {
            get => (double?) this.GetValue(MajorLineSizeProperty);
            set => this.SetValue(MajorLineSizeProperty, value);
        }

        public VerticalAlignment TopRulerLineAlignment {
            get => (VerticalAlignment) this.GetValue(TopRulerLineAlignmentProperty);
            set => this.SetValue(TopRulerLineAlignmentProperty, value);
        }

        public HorizontalAlignment LeftRulerLineAlignment {
            get => (HorizontalAlignment) this.GetValue(LeftRulerLineAlignmentProperty);
            set => this.SetValue(LeftRulerLineAlignmentProperty, value);
        }

        public double MinorLineThickness {
            get => (double) this.GetValue(MinorLineThicknessProperty);
            set => this.SetValue(MinorLineThicknessProperty, value);
        }

        public double MajorLineThickness {
            get => (double) this.GetValue(MajorLineThicknessProperty);
            set => this.SetValue(MajorLineThicknessProperty, value);
        }

        [Bindable(true)]
        [Category("Appearance")]
        public Brush Background {
            get => (Brush) this.GetValue(BackgroundProperty);
            set => this.SetValue(BackgroundProperty, value);
        }

        [Bindable(true)]
        [Category("Appearance")]
        [Localizability(LocalizationCategory.Font)]
        public FontFamily FontFamily {
            get => (FontFamily) this.GetValue(FontFamilyProperty);
            set => this.SetValue(FontFamilyProperty, value);
        }

        [Bindable(true)]
        [Category("Appearance")]
        public Brush Foreground {
            get => (Brush) this.GetValue(ForegroundProperty);
            set => this.SetValue(ForegroundProperty, value);
        }

        public Typeface CachedTypeFace { get; set; }

        protected RulerBase() {
        }

        protected abstract void UpdateRulerPosition(RulerPosition position);
    }
}