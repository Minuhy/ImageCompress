using System;
using System.Collections.Generic;
using System.IO;
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
using Microsoft.WindowsAPICodePack.Dialogs;

namespace ImgZipBat
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private RadioButton[] rbs;
        private bool isStart;
        private Thread thread;
        public MainWindow()
        {
            InitializeComponent();
            InitView();
        }

        private void InitView()
        {
            gdLog.Visibility = Visibility.Hidden;
            gdSetting.Visibility = Visibility.Visible;
            pbMain.Visibility = Visibility.Hidden;
            lbPbText.Visibility = Visibility.Hidden;

            rbs = new RadioButton[5];
            rbs[0] = rbOverWrite;
            rbs[1] = rbMoveSource;
            rbs[2] = rbMoveZip;
            rbs[3] = rbReNameSource;
            rbs[4] = rbReNameZip;

            rbOverWrite.IsChecked = true;
            rbJpg.IsChecked = true;
            slZipLevel.Maximum = 100;
            slZipLevel.Value = 50;

            Rb_Click(rbOverWrite,null);
        }

        private void SlZipLevel_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if(sender == slZipLevel)
            {
                int v = (int)slZipLevel.Value;
                string des = "（低）";
                if (v>40 && v < 70)
                {
                    des = "（中）";
                }
                else if(v>=70)
                {
                    des = "（高）";
                }
                if (lbZipLevel != null)
                {
                    lbZipLevel.Content = string.Format("{0:D3}{1}", v, des);
                }
            }
        }

        
        private void Rb_Click(object sender, RoutedEventArgs e)
        {
            for(int i =0;i< rbs.Length; i++)
            {
                if (rbs[i] != sender)
                {
                    rbs[i].IsChecked = false;
                }
                else
                {
                    rbs[i].IsChecked = true;
                }
            }

            if (sender == rbReNameSource || sender == rbReNameZip)
            {
                tbMoveDir.IsEnabled = false;
                btnChooseMoveDir.IsEnabled = false;
                tbFileReg.IsEnabled = true;
            }else if(sender == rbMoveSource || sender == rbMoveZip)
            {

                tbMoveDir.IsEnabled = true;
                btnChooseMoveDir.IsEnabled = true;
                tbFileReg.IsEnabled = false;
            }
            else if (sender == rbOverWrite)
            {
                tbFileReg.IsEnabled = false;
                tbMoveDir.IsEnabled = false;
                btnChooseMoveDir.IsEnabled = false;
            }
        }

        private void BtnChooseDir_Click(object sender, RoutedEventArgs e)
        {
            string tip = "请选择待压缩图片所在的文件夹";
            if(btnChooseMoveDir == sender)
            {
                if (rbMoveSource.IsChecked == true)
                {
                    tip = "请选择源图片保存的文件夹";
                }
                else
                {
                    tip = rbMoveZip.IsChecked == true ? "请选择压缩图片保存的文件夹" : "选一个文件夹吧";
                }
            }

            CommonOpenFileDialog dialog = new CommonOpenFileDialog(tip)
            {
                IsFolderPicker = true //选择文件还是文件夹（true:选择文件夹，false:选择文件）
            };
            string path = null;
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                path = dialog.FileName;
            }

            if(path != null)
            {
                if (btnChooseMoveDir == sender)
                {
                    tbMoveDir.Text = path;
                }
                else
                {
                    tbFilesDir.Text = path;
                }
            }
        }

        private void BtnStart_Click(object sender, RoutedEventArgs e)
        {
            if (isStart)
            {
                MessageBoxResult result = MessageBox.Show("确定取消吗？", "取消", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if(result == MessageBoxResult.Yes)
                {
                    Cope.isClose = true;
                    thread = null;
                }
            }
            else
            {

                if (sender == btnStart)
                {
                    if (Cope.isClose)
                    {
                        if (!isStart)
                        {
                            if  (CheckAndSetConfig())
                            {
                                isStart = true;
                                Cope.isClose = false;

                                // 开始压缩
                                LockView(true);
                                Cope cope = new Cope();
                                cope.DisposeProgressEvent += Cope_DisposeProgressEvent;

                                thread = new Thread(cope.Dispose);
                                thread.Start();
                                Tools.Log("线程启动");
                            }
                            
                        }
                    }
                    else
                    {
                        MessageBox.Show("正在处理图片", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void Cope_DisposeProgressEvent(int current, int total, string fileName, string info)
        {
            string des = string.Format("{0}/{1} {2} {3}", current, total, info,fileName);

            int pbv = 0;
            if (total != 0 && current > 0)
            {
                pbv = current / total * 100;
                if (pbv >= 97)
                {
                    pbv = 100;
                }
            }

            Dispatcher.Invoke(new Action(() => {
                if (current >= 0)
                {
                    _ = lbLog.Items.Add(des);
                    if (pbv > 0 && pbv <= 100)
                    {
                        pbMain.Value = pbv;
                    }
                    lbPbText.Content = string.Format("正在处理（{0}%）：{1}", pbv, fileName);
                }else if(current == -1) // 表示完成了
                {
                    des = "完成：已成功处理" + total + "个文件";
                    _ = lbLog.Items.Add(des);
                    LockView(false);
                    isStart = false;

                    MessageBox.Show(des, "完成", MessageBoxButton.OK, MessageBoxImage.Information);
                }else if (current == -2) // 表示被中断停止了
                {
                    des = "停止：已成功处理" + total + "个文件";
                    _ = lbLog.Items.Add(des);
                    LockView(false);
                    isStart = false;

                    MessageBox.Show(des, "停止", MessageBoxButton.OK, MessageBoxImage.Warning);
                }

                // 设置日志最大条目数
                if(lbLog.Items.Count > 2000)
                {
                    lbLog.Items.RemoveAt(0);
                }
            }));
        }

        private void LockView(bool v)
        {
            if (v)
            {
                btnStart.Content = "停止";
                gdLog.Visibility = Visibility.Visible;
                gdSetting.Visibility = Visibility.Hidden;
                pbMain.Visibility = Visibility.Visible;
                lbPbText.Visibility = Visibility.Visible;
                btnLog.Visibility = Visibility.Hidden;
            }
            else
            {
                btnStart.Content = "开始";
                gdSetting.Visibility = Visibility.Visible;
                gdLog.Visibility = Visibility.Hidden;
                pbMain.Visibility = Visibility.Hidden;
                lbPbText.Visibility = Visibility.Hidden;
                btnLog.Visibility = Visibility.Visible;
            }

            tbFilesDir.IsEnabled = !v;
            btnChooseDir.IsEnabled = !v;
            for (int i = 0; i < rbs.Length; i++)
            {
                rbs[i].IsEnabled = !v;
            }

            if (true == rbReNameSource.IsChecked || true == rbReNameZip.IsChecked)
            {
                tbFileReg.IsEnabled = !v;
            }
            else if (true == rbMoveSource.IsChecked || true == rbMoveZip.IsChecked)
            {
                tbMoveDir.IsEnabled = !v;
                btnChooseMoveDir.IsEnabled = !v;
            }

            btnStart.Visibility = Visibility.Visible;
            btnLog.Visibility = Visibility.Visible;
        }

        private bool CheckAndSetConfig()
        {
            if (!Directory.Exists(tbFilesDir.Text.ToString()))
            {
                _ = MessageBox.Show("源图片文件夹找不到！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            else
            {
                Cope.sourcePath = tbFilesDir.Text.ToString();
            }

            if (rbMoveSource.IsChecked == true || rbMoveZip.IsChecked == true)
            {
                if (!Directory.Exists(tbMoveDir.Text.ToString()))
                {
                    _ = MessageBox.Show("目标文件夹找不到！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }
                else
                {
                    Cope.arg = tbMoveDir.Text.ToString();
                    if (rbMoveSource.IsChecked == true)
                    {
                        Cope.way = 4;
                    }
                    else if (rbMoveZip.IsChecked == true)
                    {
                        Cope.way = 5;
                    }
                }
            }
            else if (rbReNameSource.IsChecked == true || rbReNameZip.IsChecked == true)
            {
                Cope.arg = tbFileReg.Text.ToString();
                if (rbReNameSource.IsChecked == true)
                {
                    Cope.way = 2;
                }
                else if (rbReNameZip.IsChecked == true)
                {
                    Cope.way = 3;
                }
            }
            else if(rbOverWrite.IsChecked == true)
            {
                Cope.way = 1;
            }

            if(rbJpg.IsChecked == true)
            {
                Cope.isJpg = true;
            }
            else
            {
                Cope.isJpg = false;
            }

            Cope.quality = (int)slZipLevel.Value;

            return true;
        }

        private void BtnLog_Click(object sender, RoutedEventArgs e)
        {
            if(sender == btnLog)
            {
                if(gdSetting.Visibility == Visibility.Visible)
                {
                    btnLog.Content = "转到设置";
                    gdSetting.Visibility = Visibility.Hidden;
                    gdLog.Visibility = Visibility.Visible;
                }
                else
                {
                    btnLog.Content = "查看日志";
                    gdSetting.Visibility = Visibility.Visible;
                    gdLog.Visibility = Visibility.Hidden;
                }
            }
        }
    }
}
