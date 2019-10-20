﻿using Xamarin.Forms;

namespace MoneyFox.Presentation.Controls
{
    public class CardView : Frame
    {
        public CardView()
        {
            Padding = 0;

            if (Device.RuntimePlatform == Device.UWP)
            {
                HasShadow = false;
                BorderColor = Color.Transparent;
                BackgroundColor = Color.Transparent;
                CornerRadius = 0;
            }
        }

        public static readonly BindableProperty ShadowRadiusProperty = BindableProperty.Create(
            nameof(ShadowRadius),
            typeof(int),
            typeof(CardView),
            10);

        public int ShadowRadius
        {
            get => (int) GetValue(ShadowRadiusProperty);
            set => SetValue(ShadowRadiusProperty, value);
        }
    }
}
