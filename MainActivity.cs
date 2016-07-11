using System;
using Android.App;
using Android.Widget;
using Android.OS;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Android.Content;

namespace eClock
{
    [Activity(Label = "eClock", MainLauncher = true, Icon = "@drawable/eclock")]
    public class MainActivity : Activity
    {
        private string TimezoneFilePath;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            TimezoneFilePath = TimezoneAPI.GetTimezoneFilePath();
            if (!File.Exists(TimezoneFilePath))
                File.WriteAllText(TimezoneFilePath, "America/Caracas");

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);
            SetupSpinner();

            Button updateButton = FindViewById<Button>(Resource.Id.updateButton);
            updateButton.Click += (o, e) => {
                UpdateTextView();
                StartService(new Intent("com.xamarin.EclockIntentService"));
            };
        }

        private void SetupSpinner()
        {
            // Get timezones list from file
            const string jsonfile = "timezones.json";
            string jsonstr = string.Empty;

            using (var input = Assets.Open(jsonfile))
                using (StreamReader sr = new System.IO.StreamReader(input))
                {
                    jsonstr = sr.ReadToEnd();
                }

            var timezones = Newtonsoft.Json.JsonConvert.DeserializeObject<IEnumerable<Timezone>>(jsonstr);

            // Initalize spinner
            Spinner timezoneSpinner = FindViewById<Spinner>(Resource.Id.timezoneSpinner);

            var timezonesList = timezones.OrderBy(o => o.time_zone).Select(o => o.time_zone).ToList();
            var currentTimezone = File.ReadAllText(TimezoneFilePath);

            var adapter = new ArrayAdapter(
                this, 
                Android.Resource.Layout.SimpleSpinnerItem, 
                timezonesList
            );

            adapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            timezoneSpinner.Adapter = adapter;
            timezoneSpinner.SetSelection(timezonesList.IndexOf(currentTimezone));

            // Autoexecute on app init
            timezoneSpinner.ItemSelected += new EventHandler<AdapterView.ItemSelectedEventArgs>(spinner_ItemSelected);
        }

        private void spinner_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            Spinner spinner = (Spinner)sender;
            string newTimezone = spinner.GetItemAtPosition(e.Position).ToString();

            // Update and save the current timezone
            File.WriteAllText(TimezoneFilePath, newTimezone);

            string toast = string.Format("Current Timezone: {0}", newTimezone);
            Toast.MakeText(this, toast, ToastLength.Long).Show();

            // update ui
            UpdateTextView();
            // update widget through service
            StartService(new Intent("com.xamarin.EclockIntentService"));
        }


        private void UpdateTextView()
        {
            TextView timeTextView = FindViewById<TextView>(Resource.Id.timeTextView);
            Chronometer timeChrono = FindViewById<Chronometer>(Resource.Id.timeChrono);

            ApiResponse result = TimezoneAPI.GetTimezone();
            long chronobase = 0;
            string timeUpdate;

            if (result.status == "OK")
            {
                DateTime dt = TimezoneAPI.UnixTimeStampToDateTime(result.timestamp);
                timeUpdate = string.Format("{0:tt}", dt).ToUpper();

                // current time to milliseconds
                long min = (60 * 1000);
                long hour = min * 60;
                // hour in 12h format
                int h12 = Convert.ToInt32(string.Format("{0:hh}", dt));
                chronobase = (h12 * hour) + (dt.Minute * min);

                timeChrono.Base = SystemClock.ElapsedRealtime() - chronobase;
                timeChrono.Start();
            }
            else
            {
                timeUpdate = "Err";
                timeChrono.Base = SystemClock.ElapsedRealtime();
            }

            timeTextView.Text = timeUpdate;
        }

    }
}

