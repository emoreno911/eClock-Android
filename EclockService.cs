using System;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Widget;
using Android.Appwidget;
using System.Threading;
using System.Xml.Linq;

namespace eClock
{
    // For this simple service we gonna use the IntentService Class
    // developer.xamarin.com/guides/android/application_fundamentals/services/part_1_-_started_services/#IntentService_class
    [Service]
    [IntentFilter(new String[] { "com.xamarin.EclockIntentService" })]
    public class EclockIntentService : IntentService
    {
        public EclockIntentService() : base("EclockIntentService")
        {
        }

        protected override void OnHandleIntent(Intent intent)
        {
            // Build the widget update
            RemoteViews updateViews = buildUpdate(this);

            // Push update for this widget to the home screen
            ComponentName thisWidget = new ComponentName(this, Java.Lang.Class.FromType(typeof(EclockWidget)).Name);
            AppWidgetManager manager = AppWidgetManager.GetInstance(this);
            manager.UpdateAppWidget(thisWidget, updateViews);
        }

        public RemoteViews buildUpdate(Context context)
        {
            ApiResponse result = TimezoneAPI.GetTimezone();
            long chronobase = 0;
            string timeUpdate, zoneUpdate;

            if (result.status == "OK")
            {
                DateTime dt = TimezoneAPI.UnixTimeStampToDateTime(result.timestamp);
                timeUpdate = string.Format("{0:tt}", dt).ToUpper();
                zoneUpdate = result.zoneName;

                // current time to milliseconds
                long min = (60 * 1000);
                long hour = min * 60;
                // hour in 12h format
                int h12 = Convert.ToInt32(string.Format("{0:hh}", dt));
                chronobase = (h12 * hour) + (dt.Minute * min);
            }
            else
            {
                timeUpdate = result.message;
                zoneUpdate = "Warning: Sync Error";
            }

            // Build an update that holds the updated widget contents
            var updateViews = new RemoteViews(context.PackageName, Resource.Layout.WidgetContent);
            updateViews.SetTextViewText(Resource.Id.wtimezoneText, zoneUpdate);

            if(result.status == "OK")
            {
                updateViews.SetTextViewText(Resource.Id.wtimeText, timeUpdate);
                updateViews.SetChronometer(Resource.Id.wchrono, SystemClock.ElapsedRealtime() - chronobase, null, true);
            }
            
            return updateViews;
        }
    }
}