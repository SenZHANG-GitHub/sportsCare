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
using System.Windows.Threading;
using System.Threading;
using System.Media;

using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.IO.Ports;

using Microsoft.Research.DynamicDataDisplay;
using Microsoft.Research.DynamicDataDisplay.DataSources;
using Microsoft.Research.DynamicDataDisplay.PointMarkers;

using System.Data;
using System.Data.OleDb;

using System.Collections;
using Microsoft.Win32;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization.Formatters.Binary;
namespace sportsCare
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        //private int readBufferFlag = 0;
        //static private int dataAmount = 6; //每次传进来的数据量*4(每次Hex后拆成个十百千四位)
        static private int dataAmountRSO = 4; 
        private int readVal = 0;
        private int readVal2 = 0;

        private int beginFlagPul = 9999;  //设定好的起始标志位
        private int endFlagPul = 8888;    //设定好的终止标志位

        private int beginFlagRSO = 9977;  //设定好的起始标志位
        private int endFlagRSO = 8877;    //设定好的终止标志位

        private int countRSO = 0;

        private int readValPul;
        private int[] readValListRSO = new int[dataAmountRSO];
        private byte[] readBuffer = new byte[4];
        private byte[] readBuffer2 = new byte[4];

        private int age = 20;                 //用户年龄
        private int stablePul = 60;           //用户静息心率
        private bool asciiEnable = true;

        private int KWNmin = 144;     //(int)((220-20-60)*0.6+60)
        private int KWNmax = 179;     //(int)((220-20-60)*0.85+60)
        private int KWNmid = 161;     //<= or >
        private string pulAna;

        //private int[] rso = { 60,61,60,65,65,60,63,64,63,63,62,60,60,59,58,58,57,55,56,53,54,54,55,51,52,51,53,55,53,55,53,51,52,52,52,52,53,53,55,56,56,55,54,57,57,57,54,55,58,58,59,58,59,60,60,59,61,64,61,62,61,62,65,64,64,64,63,65,66,66,64,63,62,65,62 };

        private bool collecting = false;
        private bool collectingRSO = false;
        private bool collectingPul = false;
        //private bool readEnable = false;
        static public SerialPort comPort = new SerialPort();
        static public SerialPort comPort2 = new SerialPort();

       
        private ArrayList dataList = new ArrayList();
        private ArrayList pulList = new ArrayList();
        private ArrayList lenList = new ArrayList();
        private ArrayList fiList = new ArrayList();
        private ObservableDataSource<Point> datapoint1 = new ObservableDataSource<Point>();
        private ObservableDataSource<Point> datapoint2 = new ObservableDataSource<Point>();

        public System.Media.SoundPlayer sp = new SoundPlayer(@"D:\bp.wav");
        
        public MainWindow()
        {
            InitializeComponent();
        }

        //private int getVolt(int dataVal)
        //{
        //    return (int)((dataVal * 5 * 1000) / 1024);  
        //}

        private int max(int v1, int v2)
        {
            if (v1 > v2)
                return v1;
            else
                return v2;
        }

        private int min(int v1, int v2)
        {
            if (v1 < v2)
                return v1;
            else
                return v2;
        }

        private int getrSO2(int val11,int val12,int val21,int val22)
        {
            double x,y,w,z;
            x = (double)val11/val12;
            y = (double)val21/val22;
            w = (double)2*1/3.5;
            z = (Math.Log(x)-w)/(Math.Log(y)-w); 
            return (int)(10000*(16745-7861*z*z)/(3735*z*z+10649));
        }

        private void save_Click(object sender, RoutedEventArgs e)
        {
            SaveUnit su = new SaveUnit(dataList, pulList);
            stopWork_Click(sender, e);
            SaveFileDialog savedialog = new SaveFileDialog();
            savedialog.Filter = "spc运动监护系统|*.spc|所有文件|*.*";
            savedialog.DefaultExt = "spc";
            savedialog.AddExtension = true;

            savedialog.OverwritePrompt = true;
            savedialog.Title = "保存";
            savedialog.ValidateNames = true;


            if (savedialog.ShowDialog().Value)
            {
                FileStream fo = new FileStream(savedialog.FileName, FileMode.Create, FileAccess.Write);
                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize(fo, su);
                fo.Close();

                MessageBox.Show("文件已保存", "saved");
            }
            lenList.Clear();
        }

        private void open_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog opendialog = new OpenFileDialog();
            opendialog.Filter = "spc运动监护系统|*.spc|所有文件|*.*";
            opendialog.DefaultExt = "spc";

            opendialog.Title = "打开";
            opendialog.ValidateNames = true;

            opendialog.CheckFileExists = true;

            if (opendialog.ShowDialog().Value)
            {
                FileStream fi = new FileStream(opendialog.FileName, FileMode.Open, FileAccess.Read);
                BinaryFormatter bf = new BinaryFormatter();
                SaveUnit sc = bf.Deserialize(fi) as SaveUnit;
                dataList = sc.dataList;
                pulList = sc.pulList;
                showParamStaticPul(pulList);
                showParamStaticRSO(dataList);
                if (pulList.Count > 0)
                    dp.Position = new System.Windows.Point(0, (int)pulList[0]);
                else
                    if (dataList.Count > 0)
                        dp.Position = new System.Windows.Point(0, (((DataUnitRSO)dataList[0]).rSO2) / 100);
            }
            MessageBox.Show("文件已打开", "opened");
        }

        private void dataAnalyse_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("脉搏："+pulseAnalyse.Text+"\r\n"+"\r\n"+"肌氧："+rSO2Analyse.Text+"\r\n"+"\r\n", "DataAnalyse");
        }

        private void pulAnalyse(int i,int j)
        {
            if ((int)pulList[i] < KWNmin)
            {
               if(j == 0)
                pulAna = "运动强度未达标";
               else
                pulseAnalyse.Text = "运动强度未达标";
            }

            if (((int)pulList[i] < KWNmid)&&((int)pulList[i]>=KWNmin))
            {
                if(j == 0)
                 pulAna = "运动强度适中";
                else
                 pulseAnalyse.Text = "运动强度适中";
            }

            if (((int)pulList[i] < KWNmax) && ((int)pulList[i] >= KWNmid))
            {
                if(j == 0)
                 pulAna = "轻微疲劳";
                else
                 pulseAnalyse.Text = "轻微疲劳";
            }

            if ((int)pulList[i] >= KWNmax)
            {
                sp.Load();
                sp.Play();
                if (j == 0)
                {
                    pulAna = "过度疲劳";
                }
                else
                    pulseAnalyse.Text = "过度疲劳";
            }
        }
        

        private void beginWork_Click(object sender, RoutedEventArgs e)
        {
            collecting = true;
            MainWindow.comPort.DataReceived += new SerialDataReceivedEventHandler(com_DataReceived);
            MainWindow.comPort2.DataReceived += new SerialDataReceivedEventHandler(com2_DataReceived);
        }

        public void com2_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            try
            {
                if (collecting)
                {
                    comPort2.Read(readBuffer2, 0, 4);
                    if (asciiEnable)
                    {
                        readVal2 = (int)((readBuffer2[0] - 48) * 1000 + (readBuffer2[1] - 48) * 100 + (readBuffer2[2] - 48) * 10 + (readBuffer2[3] - 48));
                    }
                    else
                    {
                        readVal2 = (int)(readBuffer2[0] * 1000 + readBuffer2[1] * 100 + readBuffer2[2] * 10 + readBuffer2[3]);
                    }

                    if ((readVal2 == endFlagRSO) && collectingRSO)
                    {
                        collectingRSO = false;
                        DataUnitRSO du = new DataUnitRSO(readValListRSO);    // 将readValList写入预置的类dataUnit中 dataUnit.addVal(readValList);
                        du.rSO2 = getrSO2(readValListRSO[0], readValListRSO[1], readValListRSO[2], readValListRSO[3]);
                        dataList.Add(du);
                        this.rSO2Txt.Dispatcher.Invoke(new Action(() => { this.rSO2Txt.Text = (du.rSO2 / 100).ToString() + "." + (du.rSO2 - ((int)du.rSO2 / 100) * 100).ToString(); }));     //防止线程间冲突
                        showParamDynamicRSO(dataList.Count - 1);
                        Array.Clear(readValListRSO, 0, readValListRSO.Length);        //清空readValList
                    }
                
                    if (collectingRSO)
                    {
                        readValListRSO[countRSO] = readVal2;
                        countRSO++;
                    }

                    if (readVal2 == beginFlagRSO)
                    {
                        countRSO = 0;
                        collectingRSO = true;
                    }

                }
            }
            catch
            {
                return;
            }
        }

        public void com_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            try
            {
                if (collecting)
                {
                    comPort.Read(readBuffer, 0, 4);
                    if (asciiEnable)
                    {
                        readVal = (int)((readBuffer[0] - 48) * 1000 + (readBuffer[1] - 48) * 100 + (readBuffer[2] - 48) * 10 + (readBuffer[3] - 48));
                    }
                    else
                    {
                        readVal = (int)(readBuffer[0] * 1000 + readBuffer[1] * 100 + readBuffer[2] * 10 + readBuffer[3]);
                    }

                    //if ((readVal == endFlagRSO) && collectingRSO)
                    //{
                    //    collectingRSO = false;
                    //    DataUnitRSO du = new DataUnitRSO(readValListRSO);    // 将readValList写入预置的类dataUnit中 dataUnit.addVal(readValList);
                    //    du.rSO2 = getrSO2(readValListRSO[0], readValListRSO[1], readValListRSO[2], readValListRSO[3]);
                    //    dataList.Add(du);
                    //    this.rSO2Txt.Dispatcher.Invoke(new Action(() => { this.rSO2Txt.Text = (du.rSO2 / 100).ToString() + "." + (du.rSO2 - ((int)du.rSO2 / 100) * 100).ToString(); }));     //防止线程间冲突
                    //    //this.pulseTxt.Dispatcher.Invoke(new Action(() => { this.pulseTxt.Text = du.pulseVal.ToString(); }));
                    //    //this.temperatureTxt.Dispatcher.Invoke(new Action(() => { this.temperatureTxt.Text = du.value3.ToString(); }));

                    //    showParamDynamicRSO(dataList.Count - 1);
                    //    Array.Clear(readValListRSO, 0, readValListRSO.Length);        //清空readValList
                    //}

                    if ((readVal == endFlagPul) && collectingPul)
                    {
                        collectingRSO = false;
                        if ((readValPul > 200) && (readValPul < 2000))
                            readValPul = (int)(readValPul / 10);
                        if (readValPul > 2000)
                            readValPul = (int)(readValPul / 100);
                        if (pulList.Count > 5)
                        {
                            if (readVal > 1.1 * (int)pulList[pulList.Count - 1])
                                readVal = (int)(1.1 * (int)pulList[pulList.Count - 1]);
                            if (readVal < 0.9 * (int)pulList[pulList.Count - 1])
                                readVal = (int)(0.9 * (int)pulList[pulList.Count - 1]);
                        }
                        pulList.Add(readValPul);
                        pulAnalyse(pulList.Count - 1, 0);
                        this.pulseTxt.Dispatcher.Invoke(new Action(() => { this.pulseTxt.Text = readValPul.ToString(); }));
                        this.pulseAnalyse.Dispatcher.Invoke(new Action(() => { this.pulseAnalyse.Text =pulAna; }));
                        showParamDynamicPul(pulList.Count - 1);
                        readValPul = 0;
                    }

                    //if (collectingRSO)
                    //{
                    //    readValListRSO[countRSO] = readVal;
                    //    countRSO++;
                    //}

                    if (collectingPul)
                    {
                        readValPul = readVal;
                    }

                    //if (readVal == beginFlagRSO)
                    //{
                    //    countRSO = 0;
                    //    collectingRSO = true;
                    //}

                    if (readVal == beginFlagPul)
                    {
                        collectingPul = true;
                    }
                }
            }
            catch
            {
                return;
            }
        }

        private void stopWork_Click(object sender, RoutedEventArgs e)
        {
            collecting = false;
            //dataList.Clear();
            //for (int i = 0; i < 75; i++)
            //{
            //    DataUnitRSO du = new DataUnitRSO(100*rso[i]); 
            //    dataList.Add(du);
            //}
            //showParamStaticRSO(dataList);
            if (pulList.Count > 0)
                dp.Position = new System.Windows.Point(0, (int)pulList[0]);
            else
                if(dataList.Count > 0)
                    dp.Position = new System.Windows.Point(0, (((DataUnitRSO)dataList[0]).rSO2)/100);
                
        }
        private void showTable_Click(object sender, RoutedEventArgs e)
        {
            collecting = false;
            DataTable dtbl = new DataTable();
            dtbl.Columns.Add("time");
            dtbl.Columns.Add("第一波长近端");
            dtbl.Columns.Add("第一波长远端");
            dtbl.Columns.Add("第二波长近端");
            dtbl.Columns.Add("第二波长远端");
            dtbl.Columns.Add("rSO2");
            dtbl.Columns.Add("pulseVal");
            //for (int i = 0; i < 500; i++)
            //{
            //    dtbl.Columns.Add("第"+(i+1).ToString()+"个脉搏数据", typeof(double));
            //}

            if (dataList.Count < pulList.Count)
            {
                for (int j = 0; j < dataList.Count; j++)
                {
                    DataRow cRow = dtbl.NewRow();
                    object[] rowData = new object[7];
                    rowData[0] = j;
                    rowData[1] = ((DataUnitRSO)dataList[j]).value11;
                    rowData[2] = ((DataUnitRSO)dataList[j]).value12;
                    rowData[3] = ((DataUnitRSO)dataList[j]).value21;
                    rowData[4] = ((DataUnitRSO)dataList[j]).value22;
                    rowData[5] = ((DataUnitRSO)dataList[j]).rSO2;
                    rowData[6] = pulList[j];
                    cRow.ItemArray = rowData;
                    dtbl.Rows.Add(cRow);
                }
                for (int j = 0; j < (pulList.Count - dataList.Count); j++)
                {
                    DataRow cRow = dtbl.NewRow();
                    object[] rowData = new object[7];
                    rowData[0] = j + dataList.Count;
                    rowData[1] = 0;
                    rowData[2] = 0;
                    rowData[3] = 0;
                    rowData[4] = 0;
                    rowData[5] = 0;
                    rowData[6] = pulList[j];
                    cRow.ItemArray = rowData;
                    dtbl.Rows.Add(cRow);
                }
            }
            else
            {
                for (int j = 0; j < pulList.Count; j++)
                {
                    DataRow cRow = dtbl.NewRow();
                    object[] rowData = new object[7];
                    rowData[0] = j;
                    rowData[1] = ((DataUnitRSO)dataList[j]).value11;
                    rowData[2] = ((DataUnitRSO)dataList[j]).value12;
                    rowData[3] = ((DataUnitRSO)dataList[j]).value21;
                    rowData[4] = ((DataUnitRSO)dataList[j]).value22;
                    rowData[5] = ((DataUnitRSO)dataList[j]).rSO2;
                    rowData[6] = pulList[j];
                    cRow.ItemArray = rowData;
                    dtbl.Rows.Add(cRow);
                }
                for (int j = 0; j < (dataList.Count - pulList.Count); j++)
                {
                    DataRow cRow = dtbl.NewRow();
                    object[] rowData = new object[7];
                    rowData[0] = j;
                    rowData[1] = ((DataUnitRSO)dataList[j]).value11;
                    rowData[2] = ((DataUnitRSO)dataList[j]).value12;
                    rowData[3] = ((DataUnitRSO)dataList[j]).value21;
                    rowData[4] = ((DataUnitRSO)dataList[j]).value22;
                    rowData[5] = ((DataUnitRSO)dataList[j]).rSO2;
                    rowData[6] = 0;
                    cRow.ItemArray = rowData;
                    dtbl.Rows.Add(cRow);
                }
            }
            ShowTable showTbl = new ShowTable(dtbl);
            showTbl.Show();
        }

        private void exit_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult key = MessageBox.Show( "确定要退出?", "confirm",MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No);
            bool cancel;
            cancel = (key == MessageBoxResult.No);
            if (cancel)
                return;
            else
                this.Close();
        }

        private void dp_MouseMove(object sender, MouseEventArgs e)
        {
            vl.Value = dp.Position.X;
            if (collecting == false)
            {
                if (dp.Position.X >= 0 && dp.Position.X < pulList.Count - 1)
                {
                    pulseTxt.Text = pulList[(int)dp.Position.X].ToString();
                    pulAnalyse((int)dp.Position.X, 1);
                }
                else
                {
                    pulseTxt.Text = "Out of Range";
                }
                if (dp.Position.X >= 0 && dp.Position.X < dataList.Count - 1)
                {
                    DataUnitRSO dutemp = (DataUnitRSO)dataList[(int)dp.Position.X];
                    rSO2Txt.Text = (dutemp.rSO2 / 100).ToString() + "." + (dutemp.rSO2 - ((int)dutemp.rSO2 / 100) * 100).ToString();
                }
                else
                {
                    rSO2Txt.Text = "Out of Range";
                }
            }
        }

        private void dp_MouseUp(object sender, MouseButtonEventArgs e)
        {
            vl.Value = dp.Position.X;
            if (collecting == false)
            {
                if (dp.Position.X >= 0 && dp.Position.X < pulList.Count - 1)
                {
                    pulseTxt.Text = pulList[(int)dp.Position.X].ToString();
                    pulAnalyse((int)dp.Position.X, 1);
                }
                else
                {
                    pulseTxt.Text = "Out of Range";
                }

                if (dp.Position.X >= 0 && dp.Position.X < dataList.Count - 1)
                {
                    DataUnitRSO dutemp = (DataUnitRSO)dataList[(int)dp.Position.X];
                    rSO2Txt.Text = (dutemp.rSO2 / 100).ToString() + "." + (dutemp.rSO2 - ((int)dutemp.rSO2 / 100) * 100).ToString();
                }
                else
                {
                    rSO2Txt.Text = "Out of Range";
                }
            }
         }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            rSO2Txt.Text = "0";
            pulseTxt.Text = "0";
            UserAge.Text = age.ToString();
            UserStablePul.Text = stablePul.ToString();
            plotter.AddLineGraph(datapoint1, new Pen(Brushes.Red, 2), new PenDescription("肌氧变化曲线"));
            plotter.AddLineGraph(datapoint2, new Pen(Brushes.Blue, 2), new PenDescription("脉搏数变化曲线"));

            dp.Position = new System.Windows.Point(0, 0);  
        }

        private void showParamDynamicRSO(int Num)    //  对应各生理数据随时间的变化关系
        {
            double x1 = Num;    
            double y1 = (((DataUnitRSO)dataList[Num]).rSO2)/100;
            Point p1 = new Point(x1, y1);
            datapoint1.AppendAsync(base.Dispatcher, p1);    
            plotter.Viewport.FitToView();
        }

        private void showParamDynamicPul(int Num)    //  对应各生理数据随时间的变化关系
        {
            double x2 = Num;
            double y2 = (int)pulList[Num];
            Point p2 = new Point(x2, y2);
            datapoint2.AppendAsync(base.Dispatcher, p2);

            plotter.Viewport.FitToView();
        }

        private void showParamStaticPul(ArrayList dataList)    //  对应各生理数据随时间的变化关系
        {
            datapoint2.Collection.Clear();
            for (int j = 0; j < pulList.Count; j++)
            {
                int x2 = j;
                int y2 = (int)pulList[j];
                Point p2 = new Point(x2, y2);
                datapoint2.AppendAsync(base.Dispatcher, p2);
            }
            plotter.Viewport.FitToView();
         }

        private void showParamStaticRSO(ArrayList dataList)    //  对应各生理数据随时间的变化关系
        {
            datapoint1.Collection.Clear();
            for (int j = 0; j < dataList.Count; j++)
            {
                double x1 = j;
                double y1 = (((DataUnitRSO)dataList[j]).rSO2) / 100;
                Point p1 = new Point(x1, y1);
                datapoint1.AppendAsync(base.Dispatcher, p1);
            }
            plotter.Viewport.FitToView();
        }

        private void showInstruConn_Click(object sender, RoutedEventArgs e)
        {
            ShowInstruConn instruConn = new ShowInstruConn();
            instruConn.Owner = this;
            instruConn.ShowDialog();
        }

        private void new_Click(object sender, RoutedEventArgs e)
        {
            dataList.Clear();
            collecting = false;
            rSO2Txt.Text = "0";
            pulseTxt.Text = "0";
            datapoint1.Collection.Clear();
            datapoint2.Collection.Clear();
            pulseAnalyse.Text = "";
            rSO2Analyse.Text = "";
            MessageBox.Show("已新建文件", "new_file");
        }

        private void ageChanged_Click_1(object sender, RoutedEventArgs e)
        {
            age = int.Parse(UserAge.Text);
            KWNmin = (int)((220 - age - stablePul) * 0.6 + stablePul);
            KWNmax = (int)((220 - age - stablePul) * 0.85 + stablePul);
            KWNmid = (int)((KWNmax + KWNmin) / 2);
            MessageBox.Show("修改成功");
        }

        private void stablePulChanged_Click_1(object sender, RoutedEventArgs e)
        {
            stablePul = int.Parse(UserStablePul.Text);
            KWNmin = (int)((220 - age - stablePul) * 0.6 + stablePul);
            KWNmax = (int)((220 - age - stablePul) * 0.85 + stablePul);
            KWNmid = (int)((KWNmax + KWNmin) / 2);
            MessageBox.Show("修改成功");
        }

        private void asciiCode_Click_1(object sender, RoutedEventArgs e)
        {
            asciiEnable = true;
        }

        private void notAscii_Click_1(object sender, RoutedEventArgs e)
        {
            asciiEnable = false;
        }

    }
}
