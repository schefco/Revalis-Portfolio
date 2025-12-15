using System.Net.Http;
using System.Windows;

namespace Revalis
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static readonly HttpClient httpClient = new HttpClient
        {
            BaseAddress = new Uri("http://127.0.0.1:8000/")
        };

        public static ApiService ApiService { get; } = new ApiService(httpClient);

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
        }
    }

}
