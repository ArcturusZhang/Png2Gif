using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Gif.Components;

namespace png2gif
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private delegate void UpdateProgressBarDelegate(System.Windows.DependencyProperty dp, Object value);

        public MainWindow()
        {
            InitializeComponent();
            Binding bindingClearAndGenrate = new Binding { Source = clear_Button, Path = new PropertyPath("IsEnabled") };
            BindingOperations.SetBinding(generate_Button, IsEnabledProperty, bindingClearAndGenrate);
            //Binding bindingClearAndFileList = new Binding { Source = fileList.Items, Path = new PropertyPath("IsEmepty") };
            //BindingOperations.SetBinding(clear_Button, IsEnabledProperty, bindingClearAndFileList);
        }
        
        private void choose_Button_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog openDialog = new Microsoft.Win32.OpenFileDialog();
            openDialog.Filter = "图像文件(*.png)|*.png";
            openDialog.Multiselect = true;
            if ((bool)openDialog.ShowDialog())
            {
                foreach (string filename in openDialog.FileNames)
                {
                    fileList.Items.Add(filename);
                }
            }
            if (!clear_Button.IsEnabled)
            {
                clear_Button.IsEnabled = true;
            }
        }

        private void clear_Button_Click(object sender, RoutedEventArgs e)
        {
            fileList.Items.Clear();
            clear_Button.IsEnabled = false;
        }

        private void fileList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // 以后可以考虑利用Converter简化这里的逻辑
            if (fileList.SelectedIndex == -1)
            {
                moveup_Button.IsEnabled = false;
                movedown_Button.IsEnabled = false;
                delete_Button.IsEnabled = false;
            }
            else if (fileList.SelectedIndex == 0)
            {
                moveup_Button.IsEnabled = false;
                movedown_Button.IsEnabled = true;
                delete_Button.IsEnabled = true;
            }
            else if (fileList.SelectedIndex == fileList.Items.Count - 1)
            {
                moveup_Button.IsEnabled = true;
                movedown_Button.IsEnabled = false;
                delete_Button.IsEnabled = true;
            }
            else
            {
                moveup_Button.IsEnabled = true;
                movedown_Button.IsEnabled = true;
                delete_Button.IsEnabled = true;
            }
        }

        private void delete_Button_Click(object sender, RoutedEventArgs e)
        {
            fileList.Items.Remove(fileList.SelectedItem);
            if (fileList.Items.IsEmpty)
            {
                clear_Button.IsEnabled = false;
            }
        }

        private void moveup_Button_Click(object sender, RoutedEventArgs e)
        {
            var selectedIndex = fileList.SelectedIndex;
            var tmp = fileList.Items[selectedIndex];
            fileList.Items[selectedIndex] = fileList.Items[selectedIndex - 1];
            fileList.Items[selectedIndex - 1] = tmp;
            fileList.SelectedIndex = selectedIndex - 1;
            fileList.Focus();
        }

        private void movedown_Button_Click(object sender, RoutedEventArgs e)
        {
            var selectedIndex = fileList.SelectedIndex;
            var tmp = fileList.Items[selectedIndex];
            fileList.Items[selectedIndex] = fileList.Items[selectedIndex + 1];
            fileList.Items[selectedIndex + 1] = tmp;
            fileList.SelectedIndex = selectedIndex + 1;
            fileList.Focus();
        }

        private void generate_Button_Click(object sender, RoutedEventArgs e)
        {
            if (!targetTextBox.Text.EndsWith(".gif"))
            {
                targetTextBox.Text += ".gif";
            }
            string outputFilePath = targetTextBox.Text;
            AnimatedGifEncoder encoder = new AnimatedGifEncoder();
            encoder.Start(outputFilePath);
            // 时间间隔设定
            int frameStepTime;
            if (!int.TryParse(frameTextBox.Text, out frameStepTime))
            {
                frameStepTime = 500;
            }
            frameTextBox.Text = frameStepTime.ToString();
            encoder.SetDelay(frameStepTime);
            // 还需要增加一个是否循环设定
            encoder.SetRepeat(0); // -1:no repeat,0:always repeat
            progressBar.Maximum = fileList.Items.Count;
            UpdateProgressBarDelegate updatePbDelegate = new UpdateProgressBarDelegate(progressBar.SetValue);
            for (int i = 0, count = fileList.Items.Count; i < count; i++)
            {
                Dispatcher.Invoke(updatePbDelegate, System.Windows.Threading.DispatcherPriority.Background,
                    new object[] { ProgressBar.ValueProperty, (double)i });
                encoder.AddFrame(System.Drawing.Image.FromFile((string)fileList.Items[i]));
            }
            encoder.Finish();
            Dispatcher.Invoke(updatePbDelegate, System.Windows.Threading.DispatcherPriority.Background,
                    new object[] { ProgressBar.ValueProperty, progressBar.Maximum });
            MessageBox.Show("文件已生成为：" + outputFilePath);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.SaveFileDialog saveDialog = new Microsoft.Win32.SaveFileDialog();
            saveDialog.Filter = "图像文件(*.gif)|*.gif";
            if ((bool)saveDialog.ShowDialog())
            {
                targetTextBox.Text = saveDialog.FileName;
            }
        }
    }
}
