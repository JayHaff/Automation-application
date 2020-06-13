using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
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
using System.Windows.Shapes;

namespace diode_Test_Beta
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class Settings : Window
    {
        
        List<PrBoard> results = new List<PrBoard> { };
       
       
        
        public Settings()
        {
            InitializeComponent();

            results = DatabaseHelper.AllBoards();

            


            BackgroundWorker bg = new BackgroundWorker();
            bg.DoWork += Bg_DoWork;
            bg.RunWorkerCompleted += Bg_RunWorkerCompleted;

            bg.RunWorkerAsync();


          
        }


        private void Bg_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            int count = 0;
            
            foreach (var x in results)
            {
                BoardsComboBox.Items.Add(x.pr_board);
                if (x.pr_board.Trim() == Properties.Settings.Default.board.ToString().Trim())
                {
                    BoardsComboBox.SelectedIndex = count;
                }
               
                count++;
            }
            COMComboBox.SelectedIndex = Properties.Settings.Default.com;

        }

        private void Bg_DoWork(object sender, DoWorkEventArgs e)
        {
            results = DatabaseHelper.AllBoards();
            
        }



        public void SaveBtn_Click(object sender, RoutedEventArgs e)
        {


            
            Properties.Settings.Default.com = COMComboBox.SelectedIndex;
            
            Properties.Settings.Default.Save();
            Properties.Settings.Default.board = BoardsComboBox.SelectedValue.ToString();
            Properties.Settings.Default.Save();

            saveBtn.IsEnabled = false;
            

            Close();
            

                       

      
     
        }
        
        
    }
}
