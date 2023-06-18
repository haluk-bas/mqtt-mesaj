using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Formatter;

namespace MQTT_Mesaj
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            TestConnection();

            Closing += MainWindow_Closing;
            Loaded += MainWindow_Loaded;

            mesaj.Focus();
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (System.IO.File.Exists("./log.txt"))
                txt1.Text = System.IO.File.ReadAllText("./log.txt");

            if (System.IO.File.Exists("./data.txt"))
                isim.Text = System.IO.File.ReadAllText("./data.txt");

        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            System.IO.File.WriteAllText("./log.txt", txt1.Text);
            System.IO.File.WriteAllText("./data.txt", isim.Text);

        }

        public async void TestConnection()
        {
            var MqttFactory = new MqttFactory();
            var mqttClient = new MqttFactory().CreateMqttClient();

            var options = new MqttClientOptionsBuilder()
                    .WithProtocolVersion(MqttProtocolVersion.V500)
                    .WithTcpServer("127.0.0.1", 1883)
                    .WithCleanSession()
                    .Build();

            try
            {
                var baglanti = await mqttClient.ConnectAsync(options, CancellationToken.None);
            }
            catch (Exception)
            {
                return;
            }

            var subuscripOption = new MqttClientSubscribeOptionsBuilder()
                .WithTopicFilter("test")
                .Build();

            await mqttClient.SubscribeAsync(subuscripOption);
            mqttClient.ApplicationMessageReceivedAsync += MqttClient_ApplicationMessageReceivedAsync;
        }

        public static async Task Publish_Application_Message(string deger)
        {
            MainWindow a = new MainWindow();
            var mqttFactory = new MqttFactory();

            using (var mqttClient = mqttFactory.CreateMqttClient())
            {
                var mqttClientOptions = new MqttClientOptionsBuilder()
                    .WithTcpServer("127.0.0.1")
                    .Build();
                try
                {
                    await mqttClient.ConnectAsync(mqttClientOptions, CancellationToken.None);
                }
                catch (Exception)
                {
                    MessageBox.Show("Mesaj Gönderilemedi!", "MQTT Broker Bağlantı Hatası", MessageBoxButton.OK, MessageBoxImage.Error);
                    a.TestConnection();
                    return;
                }

                var applicationMessage = new MqttApplicationMessageBuilder()
                    .WithTopic("test")
                    .WithPayload(deger)
                    .Build();
                try
                {
                    await mqttClient.PublishAsync(applicationMessage, CancellationToken.None);
                }
                catch (Exception)
                {
                    return;
                }
            }
        }

        private async Task MqttClient_ApplicationMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs arg)
        {
            await Dispatcher.InvokeAsync(() =>
            {
                string mesg = Encoding.UTF8.GetString(arg.ApplicationMessage.Payload);
                txt1.Text += mesg + Environment.NewLine;
            });
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            if (mesaj.Text.Length > 0 && isim.Text.Length > 0)
            {
                await Publish_Application_Message("| " + isim.Text + " |  " + mesaj.Text + "  (" + DateTime.Now.ToString("dd/MM/yy HH:mm") + ")");
                mesaj.Text = "";
            }
            else
            {
                MessageBox.Show("İsim veya mesaj boş olamaz!", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            txt1.Text = "";
        }
    }
}



