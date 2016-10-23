using DelSole.ClientAuth;
using System;
using System.Collections.Generic;
using System.Linq;
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

namespace SampleApp_CS
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        AuthService AuthService;

        public MainWindow()
        {
            InitializeComponent();
            this.AuthService = new AuthService("https://localhost:28888/"); // Replace with your Web API based address
        }

        private async void button_Click(object sender, RoutedEventArgs e)
        {
            var token = await AuthService.LoginAsync("admin", "admin");

            if(this.AuthService.IsAuthenticated)
            {
                MessageBox.Show($"{token.AccessToken} issued at {token.IssuedAt?.ToLongDateString()}, expires on {token.ExpiresAt.ToString()}");
            }
        }
    }
}
