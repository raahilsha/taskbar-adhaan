using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Runtime.InteropServices;
using CSDeskBand;
using CSDeskBand.ContextMenu;
using System.Windows.Threading;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;

namespace TimeFinder
{
    [ComVisible(true)]
    //[Guid("89BF6B36-A0B0-4C95-A666-87A55C226986")]
    [Guid("4B8730FD-F800-4B4C-B6FA-F40CEEBE5F41")]
    [CSDeskBandRegistration(Name = "Time Finder", ShowDeskBand = true)]
    public class TimeFinder : CSDeskBandWpf
    {
        UserControl1 mainControl = new UserControl1();
        protected override UIElement UIElement => mainControl;

        String[] prayerTimes = { "00:00", "00:00", "00:00", "00:00", "00:00" };
        String[] prayerTimesTomorrow = { "00:00", "00:00", "00:00", "00:00", "00:00" };
        String[] prayerNames = { "Fajr", "Dhuhr", "Asr", "Maghrib", "Isha" };

        public TimeFinder()
        {
            Options.IsFixed = false;

            TaskbarInfo.TaskbarSizeChanged += (sender, args) =>
            {
                mainControl.TaskbarWidth = args.Size.Width;
                mainControl.TaskbarHeight = args.Size.Height;
            };

            mainControl.DisplayText = "Error";

            loadConfiguration();
            refreshDisplay_tick(null, null);

            DispatcherTimer dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Tick += new EventHandler(refreshInfo_tick);
            dispatcherTimer.Interval = new TimeSpan(0, 30, 0);
            dispatcherTimer.Start();

            DispatcherTimer dispatcherTimer2 = new DispatcherTimer();
            dispatcherTimer2.Tick += new EventHandler(refreshInfo2_tick);
            dispatcherTimer2.Interval = new TimeSpan(0, 30, 0);
            dispatcherTimer2.Start();

            DispatcherTimer dispatcherTimer3 = new DispatcherTimer();
            dispatcherTimer3.Tick += new EventHandler(refreshDisplay_tick);
            dispatcherTimer3.Interval = new TimeSpan(0, 0, 5);
            dispatcherTimer3.Start();

            // For some reason, this needs to be manually called with a delay to get the Deskband
            // to match the taskbar height
            var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(2) };
            timer.Start();
            timer.Tick += (sender, args) =>
            {
                TaskbarInfo.UpdateInfo();
                mainControl.TaskbarHeight = TaskbarInfo.Size.Height;
            };
        }

        private void refreshDisplay_tick(object sender, EventArgs e)
        {
            if (mainControl.configReloaded)
            {
                loadConfiguration();
            }

            if (prayerTimes[4].Equals("00:00"))
            {
                mainControl.DisplayText = "Error";
                return;
            }

            DateTime now = DateTime.Now;
            int currentMilHours = Int32.Parse(now.ToString("HH"));
            int currentMilMinutes = Int32.Parse(now.ToString("mm"));
            string finalDisplay = "";

            for (int i = 0; i < 5; ++i)
            {
                int prayerMilHours = Int32.Parse(prayerTimes[i].Substring(0, 2));
                int prayerMilMinutes = Int32.Parse(prayerTimes[i].Substring(3, 2));

                if (prayerMilHours > currentMilHours || (prayerMilHours == currentMilHours && prayerMilMinutes > currentMilMinutes))
                {
                    bool isAM = prayerMilHours < 12;
                    prayerMilHours = prayerMilHours % 12;
                    if (prayerMilHours == 0)
                        prayerMilHours = 12;
                    finalDisplay = prayerNames[i] + " at " + prayerMilHours.ToString() + ":" + prayerMilMinutes.ToString("00") + " " + (isAM ? "AM" : "PM");
                    break;
                }
            }

            if (finalDisplay.Equals(""))
            {
                int prayerMilHours = Int32.Parse(prayerTimesTomorrow[0].Substring(0, 2));
                int prayerMilMinutes = Int32.Parse(prayerTimesTomorrow[0].Substring(3, 2));

                bool isAM = prayerMilHours < 12;
                prayerMilHours = prayerMilHours % 12;
                if (prayerMilHours == 0)
                    prayerMilHours = 12;
                finalDisplay = prayerNames[0] + " at " + prayerMilHours.ToString() + ":" + prayerMilMinutes.ToString("00") + " " + (isAM ? "AM" : "PM");
            }

            mainControl.DisplayText = finalDisplay;
        }

        private void refreshInfo_tick(object sender, EventArgs e)
        {
            updateInfoFromWeb(0);
        }

        private void refreshInfo2_tick(object sender, EventArgs e)
        {
            updateInfoFromWeb(1);
        }

        private void updateInfoFromWeb(int day)
        {
            long timestamp = DateTimeOffset.Now.ToUnixTimeSeconds();
            timestamp += 60 * 60 * 24 * day;
            string apiUrl = "http://api.aladhan.com/v1/timings/" + timestamp.ToString();
            string apiQuery = "?latitude=" + mainControl.latitude.ToString() + "&longitude=" + mainControl.longitude.ToString() + "&method=" + mainControl.method + "&school=" + mainControl.school;

            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = client.GetAsync(apiQuery).Result;
            if (response.IsSuccessStatusCode)
            {
                var dataObjects = response.Content.ReadAsStringAsync().Result;
                string apiString = "";
                foreach (var d in dataObjects)
                {
                    apiString += d.ToString();
                }
                dynamic apiResponse = JsonConvert.DeserializeObject(apiString);
                if (day == 0)
                {
                    prayerTimes[0] = apiResponse.data.timings.Fajr;
                    prayerTimes[1] = apiResponse.data.timings.Dhuhr;
                    prayerTimes[2] = apiResponse.data.timings.Asr;
                    prayerTimes[3] = apiResponse.data.timings.Maghrib;
                    prayerTimes[4] = apiResponse.data.timings.Isha;
                }
                else
                {
                    prayerTimesTomorrow[0] = apiResponse.data.timings.Fajr;
                    prayerTimesTomorrow[1] = apiResponse.data.timings.Dhuhr;
                    prayerTimesTomorrow[2] = apiResponse.data.timings.Asr;
                    prayerTimesTomorrow[3] = apiResponse.data.timings.Maghrib;
                    prayerTimesTomorrow[4] = apiResponse.data.timings.Isha;
                }
            }
            else
            {
                mainControl.DisplayText = "Error";
                for (int i = 0; i < 5; ++i)
                    prayerTimes[i] = "00:00";
            }

            client.Dispose();
        }

        private void makeConfigFile(string filepath)
        {
            // Defaults are Harvard, ISNA, Shafi
            string[] lines = { "latitude:42.3770", "longitude:-71.1167", "method:2", "school:0" };
            System.IO.File.WriteAllLines(filepath, lines);
        }

        private void loadConfiguration()
        {
            string configfilepath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\NextPrayerTime.conf";
            if (!File.Exists(configfilepath))
            {
                makeConfigFile(configfilepath);
            }

            string[] lines = System.IO.File.ReadAllLines(configfilepath);
            foreach (string line in lines)
            {
                string field = line.Split(':')[0];
                string value = line.Split(':')[1];
                if (field.Equals("latitude"))
                    mainControl.latitude = Single.Parse(value);
                if (field.Equals("longitude"))
                    mainControl.longitude = Single.Parse(value);
                if (field.Equals("method"))
                    mainControl.method = Int32.Parse(value);
                if (field.Equals("school"))
                    mainControl.school = Int32.Parse(value);
            }

            mainControl.configReloaded = false;
            refreshInfo_tick(null, null);
            refreshInfo2_tick(null, null);
        }
    }
}
