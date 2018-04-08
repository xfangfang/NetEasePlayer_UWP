using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Data.Json;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Core;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Composition;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Web.Http;

// https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x804 上介绍了“空白页”项模板

namespace NetEasePlayer_UWP
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private String url = "http://www.neu.edu.cn/indexsource/neusong.mp3";
        private String filename = "neusong20154409.mp3";

        Frame root = Window.Current.Content as Frame;

        public MainPage()
        {
            this.InitializeComponent();

            Windows.UI.Core.SystemNavigationManager.GetForCurrentView().BackRequested += BackRequested;
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility =
                root.CanGoBack ? 
                AppViewBackButtonVisibility.Visible : Windows.UI.Core.AppViewBackButtonVisibility.Collapsed;
            root.Navigated += OnNavigated;
        }
        //标题栏返回
        //https://blog.csdn.net/zhongyanfu0/article/details/51883248
        private void OnNavigated(object sender, NavigationEventArgs e)
        {
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility =
                ((Frame)sender).CanGoBack ?
                AppViewBackButtonVisibility.Visible : AppViewBackButtonVisibility.Collapsed;
        }
        private void BackRequested(object sender, BackRequestedEventArgs e)
        {
            Frame rootFrame = Window.Current.Content as Frame;
            if (rootFrame == null)
                return;
            if (rootFrame.CanGoBack && e.Handled == false)
            {
                e.Handled = true;
                rootFrame.GoBack();
            }
        }

        //打开或保存音频
        //https://docs.microsoft.com/zh-cn/windows/uwp/files/quickstart-managing-folders-in-the-music-pictures-and-videos-libraries
        //https://docs.microsoft.com/en-us/windows/uwp/files/quickstart-managing-folders-in-the-music-pictures-and-videos-libraries
        public async Task Gets(Uri uri)
        {

            try
            {
                StorageFile destinationFile = await KnownFolders.MusicLibrary.CreateFileAsync(this.filename);
                try
                {
                    HttpClient httpClient = new HttpClient();
                    var response = await httpClient.GetAsync(uri);
                    var buffer = await response.Content.ReadAsBufferAsync();
                    var sourceStream = await response.Content.ReadAsInputStreamAsync();

                    using (var destinationStream = await destinationFile.OpenAsync(FileAccessMode.ReadWrite))
                    {
                        using (var destinationOutputStream = destinationStream.GetOutputStreamAt(0))
                        {
                            await RandomAccessStream.CopyAndCloseAsync(sourceStream, destinationStream);
                        }
                    }
                    MessageDialog msg = new MessageDialog("校歌下载完毕");
                    await msg.ShowAsync();
                }
                catch (Exception e)
                {
                    Debug.WriteLine("xxxxx{0}", e);
                    MessageDialog msg = new MessageDialog("出了点问题");
                    await msg.ShowAsync();
                }
            }
            catch
            {
                MessageDialog msg = new MessageDialog("文件不能重复下载");
                await msg.ShowAsync();
            }
            finally
            {
                button_open.Visibility = Visibility.Visible;
                mainpage_progress_ring.IsActive = false;
            }
        }   

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            mainpage_progress_ring.IsActive = true;
            Gets(new Uri(this.url));
            button_open.Visibility = Visibility.Collapsed;
        }

        private void Button_Click_Play(object sender, RoutedEventArgs e)
        {
            root.Navigate(typeof(PlayerPage), this.filename);
        }


    }
}
