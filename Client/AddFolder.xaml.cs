////////////////////////////////////////////////////////////////////////////
// AddFolder.xaml.cs - Has defintion for AddFolder events   //
// ver 1.0                                                                //
// Language:    C#, Visual Studio 2017                                  //
// Parag Taneja, CSE687 - Object Oriented Design, Spring 2018          //
///////////////////////////////////////////////////////////////////////////


/*
Package Operations:
* -------------------
* This package provides 1 class:
*
- AddFolder : where all the controls are drawn.

*Events Functions -
* private void btnDialogOk_Click(object sender, RoutedEventArgs e) - Closes Add Folder Window
* private void Window_ContentRendered(object sender, EventArgs e) - Content on window is rendered
* private void window_Load(object sender, RoutedEventArgs e) - When window is loaded , checks for folder names which are for Automation purpose.
* Maintenance History:
* --------------------
* ver 1.0 : 7th , Apr 2018


*/
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
using System.Windows.Shapes;

namespace Client
{
    /// <summary>
    /// Interaction logic for AddFolder.xaml
    /// </summary>
    public partial class AddFolder : Window
    {
       
        public AddFolder(string question, string defaultAnswer = "")
        {
            InitializeComponent();
            lblQuestion.Content = question;
            txtAnswer.Text = defaultAnswer;
        }
        //Closes Add Folder Window
        private void btnDialogOk_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        //Content on window is rendered
        private void Window_ContentRendered(object sender, EventArgs e)
        {
            txtAnswer.SelectAll();
            txtAnswer.Focus();
        }

        //When window is loaded , checks for folder names which are for Automation purpose.
        private void window_Load(object sender, RoutedEventArgs e)
        {
            if (txtAnswer.Text == "CR4c1" || txtAnswer.Text == "CR4c2" || txtAnswer.Text == "CR4c3" || txtAnswer.Text=="CR4c4")
            {
                RoutedEventArgs re = new RoutedEventArgs();
                btnDialogOk_Click(this, re);
                
            }
        }

        public string Answer
        {
            get { return txtAnswer.Text; }
        }
    }
}
