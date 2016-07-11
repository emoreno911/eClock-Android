using System;
using Android.App;
using Android.Appwidget;
using Android.Content;

namespace eClock
{
    [BroadcastReceiver(Label = "@string/WidgetName")]
    [IntentFilter(new string[] { "android.appwidget.action.APPWIDGET_UPDATE" })]
    [MetaData("android.appwidget.provider", Resource = "@layout/WidgetWrap")]
    public class EclockWidget : AppWidgetProvider
    {
        public override void OnUpdate(Context context, AppWidgetManager appWidgetManager, int[] appWidgetIds)
        {
            // To prevent any ANR timeouts, we perform the update in a service
            context.StartService(new Intent("com.xamarin.EclockIntentService"));
        }
    }
}