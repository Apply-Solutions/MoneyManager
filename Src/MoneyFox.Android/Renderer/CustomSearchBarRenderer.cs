using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Runtime;
using Android.Widget;
using Java.Lang.Reflect;
using Microsoft.AppCenter.Crashes;
using MoneyFox.Droid.Renderer;
using NLog;
using System;
using System.Linq;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using Color = Xamarin.Forms.Color;
using Object = Java.Lang.Object;

#nullable enable
[assembly: ExportRenderer(typeof(SearchBar), typeof(CustomSearchBarRenderer))]
namespace MoneyFox.Droid.Renderer
{
    public class CustomSearchBarRenderer : SearchBarRenderer
    {
        private readonly Logger logger = LogManager.GetCurrentClassLogger();

        public CustomSearchBarRenderer(Context context) : base(context)
        {
        }

        protected override void OnElementChanged(ElementChangedEventArgs<SearchBar> e)
        {
            base.OnElementChanged(e);
            try
            {
                if(Control != null)
                {

                    SearchView searchView = Control;
                    searchView.Iconified = false;
                    searchView.SetIconifiedByDefault(false);

                    SetBackgroundColor(searchView);

                    EditText editText = Control.GetChildrenOfType<EditText>().First();

                    editText.SetHighlightColor(Color.Accent.ToAndroid());

                    if(Build.VERSION.SdkInt >= BuildVersionCodes.Q)
                    {
                        TrySetCursorPointerColorNew(editText);
                    }
                    else
                    {
                        TrySetCursorPointerColor(editText);
                    }

                    UpdateSearchButtonColor();
                    UpdateCancelButtonColor();
                }
            }
            catch(Exception ex)
            {
                Crashes.TrackError(ex);
            }
        }

        private void SetBackgroundColor(SearchView searchView)
        {

            if(App.Current.UserAppTheme == OSAppTheme.Dark)
            {
                App.Current.Resources.TryGetValue("BackgroundColorSearchBarDark", out object darkTintColor);
                searchView.SetBackgroundColor(((Color)darkTintColor).ToAndroid());
            }
            else
            {
                App.Current.Resources.TryGetValue("BackgroundColorSearchBarLight", out object lightTintColor);
                searchView.SetBackgroundColor(((Color)lightTintColor).ToAndroid());
            }
        }

        private void TrySetCursorPointerColorNew(TextView textView)
        {
            try
            {
                textView.SetTextCursorDrawable(Resource.Drawable.CustomCursor);

                Drawable? textSelectHandleDrawable = textView.TextSelectHandle;

                textSelectHandleDrawable?.SetColorFilter(new BlendModeColorFilter(Color.Accent.ToAndroid(), BlendMode.SrcIn!));
                textView.TextSelectHandle = textSelectHandleDrawable;

                Drawable? textSelectHandleLeftDrawable = textView.TextSelectHandleLeft;
                textSelectHandleLeftDrawable?.SetColorFilter(new BlendModeColorFilter(Color.Accent.ToAndroid(), BlendMode.SrcIn!));
                textView.TextSelectHandle = textSelectHandleLeftDrawable;

                Drawable? textSelectHandleRightDrawable = textView.TextSelectHandleRight;
                textSelectHandleRightDrawable?.SetColorFilter(new BlendModeColorFilter(Color.Accent.ToAndroid(), BlendMode.SrcIn!));
                textView.TextSelectHandle = textSelectHandleRightDrawable;
            }
            catch(Exception ex)
            {
                logger.Error(ex, "Issue in rendering custom search bar.");
            }
        }

        private void TrySetCursorPointerColor(TextView textView)
        {
            try
            {
                var textViewTemplate = new TextView(textView.Context);

                Field field = textViewTemplate.Class.GetDeclaredField("mEditor");
                field.Accessible = true;
                Object? editor = field.Get(textView);

                if(editor == null)
                {
                    return;
                }

                string[] fieldsNames = { "mTextSelectHandleLeftRes", "mTextSelectHandleRightRes", "mTextSelectHandleRes" };
                string[] drawableNames = { "mSelectHandleLeft", "mSelectHandleRight", "mSelectHandleCenter" };

                for(int index = 0; index < fieldsNames.Length && index < drawableNames.Length; index++)
                {
                    string fieldName = fieldsNames[index];
                    string drawableName = drawableNames[index];

                    field = textViewTemplate.Class.GetDeclaredField(fieldName);
                    field.Accessible = true;
                    int handle = field.GetInt(textView);

                    Drawable? handleDrawable = Resources?.GetDrawable(handle, null);

                    if(handleDrawable == null)
                    {
                        return;
                    }

                    if(Build.VERSION.SdkInt >= BuildVersionCodes.Q)
                    {
                        handleDrawable.SetColorFilter(new BlendModeColorFilter(Color.Accent.ToAndroid(), BlendMode.SrcIn!));
                    }
                    else
                    {
#pragma warning disable CS0618 // Type or member is obsolete
                        handleDrawable.SetColorFilter(Color.Accent.ToAndroid(), PorterDuff.Mode.SrcIn!);
#pragma warning restore CS0618 // Type or member is obsolete
                    }

                    field = editor.Class.GetDeclaredField(drawableName);
                    field.Accessible = true;
                    field.Set(editor, handleDrawable);
                }

                IntPtr intPtrtextViewClass = JNIEnv.FindClass(typeof(TextView));
                IntPtr mCursorDrawableResProperty = JNIEnv.GetFieldID(intPtrtextViewClass, "mCursorDrawableRes", "I");

                JNIEnv.SetField(textView.Handle, mCursorDrawableResProperty, Resource.Drawable.CustomCursor);
            }
            catch(Exception ex)
            {
                logger.Error(ex, "Issue on setting the cursor color for custom search bar.");
            }
        }

        private void UpdateSearchButtonColor()
        {
            int? searchViewCloseButtonId = Control.Resources?.GetIdentifier("android:id/search_mag_icon", null, null);
            if(searchViewCloseButtonId.HasValue && searchViewCloseButtonId != 0)
            {
                SetColorGray(FindViewById<ImageView>(searchViewCloseButtonId.Value));
            }
        }

        private void UpdateCancelButtonColor()
        {
            int? searchViewCloseButtonId = Control.Resources?.GetIdentifier("android:id/search_close_btn", null, null);
            if(searchViewCloseButtonId.HasValue && searchViewCloseButtonId != 0)
            {
                SetColorGray(FindViewById<ImageView>(searchViewCloseButtonId.Value));
            }
        }

        private static void SetColorGray(ImageView? image)
        {
            if(Build.VERSION.SdkInt >= BuildVersionCodes.Q)
            {
                image?.Drawable?.SetColorFilter(new BlendModeColorFilter(Android.Graphics.Color.Gray, BlendMode.SrcIn!));
            }
            else
            {
#pragma warning disable CS0618 // Type or member is obsolete
                image?.Drawable?.SetColorFilter(Android.Graphics.Color.Gray, PorterDuff.Mode.SrcIn!);
#pragma warning restore CS0618 // Type or member is obsolete
            }
        }
    }
}
