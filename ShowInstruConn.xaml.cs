using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.IO.Ports;

namespace sportsCare
{
    /// <summary>
    /// ShowInstruConn.xaml 的交互逻辑
    /// </summary>
    public partial class ShowInstruConn : Window
    {
        public ShowInstruConn()
        {
            InitializeComponent();
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            string[] portList = System.IO.Ports.SerialPort.GetPortNames();
            for (int i = 0; i < portList.Length; ++i)
            {
                string name = portList[i];
                comboCom.Items.Add(name);
                comboCom2.Items.Add(name);
            }
            refreshConnStatus1();
        }

        private void refreshConnStatus1()
        {
            btnDisconn.IsEnabled = MainWindow.comPort.IsOpen;
            btnScan.IsEnabled = !MainWindow.comPort.IsOpen;
        }

        private void refreshConnStatus2()
        {
            btnDisconn2.IsEnabled = MainWindow.comPort.IsOpen;
            btnScan2.IsEnabled = !MainWindow.comPort.IsOpen;
        }

        private void btnScan_Click(object sender, RoutedEventArgs e)
        {
            if (comboCom.Items.Count == 0)
            {
                MessageBox.Show("未找到设备。请确认检测系统硬件已经连接到电脑且电源已打开", "未找到设备", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            else
            {
                try
                {
                    MainWindow.comPort.PortName = comboCom.SelectedItem.ToString();
                    MainWindow.comPort.BaudRate = 9600;
                    MainWindow.comPort.DataBits = 8;
                    MainWindow.comPort.Parity = Parity.None;
                    MainWindow.comPort.StopBits = StopBits.One;
                    MainWindow.comPort.ReceivedBytesThreshold = 4;
                    MainWindow.comPort.Open();
                    MainWindow.comPort.WriteTimeout = 3000;
                    MessageBox.Show("设备连接成功", "连接成功", MessageBoxButton.OK, MessageBoxImage.Information);
                    Close();
                }
                catch
                {
                    MessageBox.Show("设备连接失败。请确认检测系统硬件已经连接到电脑且电源已打开", "设备连接失败", MessageBoxButton.OK, MessageBoxImage.Error);
                    Close();
                }
            }
            refreshConnStatus1();
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void btnDisconn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MessageBox.Show("连接已断开", "断开成功", MessageBoxButton.OK, MessageBoxImage.Information);
                MainWindow.comPort.Close();
            }
            catch
            {
                MessageBox.Show("设备未连接，不用关闭", "设备未连接", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            refreshConnStatus1();
        }

        private void btnScan2_Click(object sender, RoutedEventArgs e)
        {
            if (comboCom2.Items.Count == 0)
            {
                MessageBox.Show("未找到设备。请确认检测系统硬件已经连接到电脑且电源已打开", "未找到设备", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            else
            {
                try
                {
                    MainWindow.comPort2.PortName = comboCom2.SelectedItem.ToString();
                    MainWindow.comPort2.BaudRate = 9600;
                    MainWindow.comPort2.DataBits = 8;
                    MainWindow.comPort2.Parity = Parity.None;
                    MainWindow.comPort2.StopBits = StopBits.One;
                    MainWindow.comPort2.ReceivedBytesThreshold = 4;
                    MainWindow.comPort2.Open();
                    MainWindow.comPort2.WriteTimeout = 3000;
                    MessageBox.Show("设备连接成功", "连接成功", MessageBoxButton.OK, MessageBoxImage.Information);
                    Close();
                }
                catch
                {
                    MessageBox.Show("设备连接失败。请确认检测系统硬件已经连接到电脑且电源已打开", "设备连接失败", MessageBoxButton.OK, MessageBoxImage.Error);
                    Close();
                }
            }
            refreshConnStatus2();
        }

        private void btnDisconn2_Click(object sender, RoutedEventArgs e)
        {

            try
            {
                MessageBox.Show("连接已断开", "断开成功", MessageBoxButton.OK, MessageBoxImage.Information);
                MainWindow.comPort2.Close();
            }
            catch
            {
                MessageBox.Show("设备未连接，不用关闭", "设备未连接", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            refreshConnStatus2();
        }
    }
}
