using GalaSoft.MvvmLight;
using GPSLocation.Model;
using GalaSoft.MvvmLight.Command;
using System.Device.Location;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using System.Device.Location;
using System.IO;
using System.Text;

namespace GPSLocation.ViewModel
{
    /// <summary>
    /// This class contains properties that the main View can data bind to.
    /// <para>
    /// Use the <strong>mvvminpc</strong> snippet to add bindable properties to this ViewModel.
    /// </para>
    /// <para>
    /// See http://www.galasoft.ch/mvvm/getstarted
    /// </para>
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        private readonly IDataService _dataService;
        private GeoCoordinateWatcher myWatcher;
        /// <summary>
        /// The <see cref="WelcomeTitle" /> property's name.
        /// </summary>
        public const string WelcomeTitlePropertyName = "WelcomeTitle";

        private string _welcomeTitle = string.Empty;

        /// <summary>
        /// Gets the WelcomeTitle property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string WelcomeTitle
        {
            get
            {
                return _welcomeTitle;
            }

            set
            {
                if (_welcomeTitle == value)
                {
                    return;
                }

                _welcomeTitle = value;
                RaisePropertyChanged(WelcomeTitlePropertyName);
            }
        }

        private string _area;
        public string Area
        {
            get
            {
                return _area;
            }

            set
            {
                if (_area == value)
                {
                    return;
                }

                _area = value;
                RaisePropertyChanged("Area");
            }
        }

        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel(IDataService dataService)
        {
            _dataService = dataService;
            _dataService.GetData(
                (item, error) =>
                {
                    if (error != null)
                    {
                        // Report error here
                        return;
                    }

                    WelcomeTitle = "点击开始";
                    Area = "所在位置";
                    initgps = new RelayCommand<string>((x) => Executeinitgps(x));

                });
        }

        public RelayCommand<string> initgps { get; private set; }

        private object Executeinitgps(string x)
        {
            //System.Windows.MessageBox.Show("go");
            myWatcher = new GeoCoordinateWatcher(GeoPositionAccuracy.High);


            myWatcher.StatusChanged += new System.EventHandler<GeoPositionStatusChangedEventArgs>(myWatcher_StatusChanged);
            myWatcher.PositionChanged += new System.EventHandler<GeoPositionChangedEventArgs<GeoCoordinate>>(myWatcher_PositionChanged);
            myWatcher.Start();
            return null;
        }

        void myWatcher_PositionChanged(object sender, GeoPositionChangedEventArgs<GeoCoordinate> e)
        {
            WelcomeTitle = string.Format("当前坐标：{0}/{1}\n", e.Position.Location.Latitude.ToString("0.00000"), e.Position.Location.Longitude.ToString("0.00000"));

            //var url=http://maps.google.com/maps/api/geocode/xml?latlng=40,40&language=zh-CN&sensor=false

            //http://msrmaps.com/TerraService2.asmx WEBSERVICES
            double latitude = e.Position.Location.Latitude;
            double longitude = e.Position.Location.Longitude;

            Uri url = new Uri(string.Format("http://maps.google.com/maps/api/geocode/xml?latlng={0},{1}&language=zh-CN&sensor=false", latitude, longitude), UriKind.Absolute);

            //GoogleAPI方式xml格式
            WebClient xml = new WebClient();

            xml.DownloadStringAsync(url);
            xml.DownloadStringCompleted += new DownloadStringCompletedEventHandler(client_DownloadStringCompleted);


            ///webServices方式
            //myTerraService.TerraServiceSoapClient client = new myTerraService.TerraServiceSoapClient();
            //client.ConvertLonLatPtToNearestPlaceCompleted += new EventHandler<myTerraService.ConvertLonLatPtToNearestPlaceCompletedEventArgs>(client_ConvertLonLatPtToNearestPlaceCompleted);
            //client.ConvertLonLatPtToNearestPlaceAsync(new myTerraService.LonLatPt { Lat = latitude, Lon = longitude });


        }



        void client_ConvertLonLatPtToNearestPlaceCompleted(object sender, myTerraService.ConvertLonLatPtToNearestPlaceCompletedEventArgs e)
        {
            //Area = String.Format("当前位置\n{0}", e.Result);
        }

        void myWatcher_StatusChanged(object sender, GeoPositionStatusChangedEventArgs e)
        {
            switch (e.Status)
            {
                case GeoPositionStatus.Disabled:

                    if (myWatcher.Permission == GeoPositionPermission.Denied)
                    {

                        WelcomeTitle = "you have this application access to location.";
                    }
                    else
                    {
                        WelcomeTitle = "location is not functioning on this device";
                    }
                    break;

                case GeoPositionStatus.Initializing:

                    WelcomeTitle = "initializing......";
                    break;

                case GeoPositionStatus.NoData:

                    WelcomeTitle = "定位失败";

                    break;

                case GeoPositionStatus.Ready:

                    WelcomeTitle = "准备就绪";

                    break;
            }
        }


        void client_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            try
            {
                Area = XElement.Parse(e.Result).Elements("result").First().Element("formatted_address").Value;
            }
            catch
            {
                Area = "在跑就到火星了";
            }
        }

        ////public override void Cleanup()
        ////{
        ////    // Clean up if needed

        ////    base.Cleanup();
        ////}
    }
}