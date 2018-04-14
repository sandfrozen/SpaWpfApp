using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SpaWpfApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void parseButton_Click(object sender, RoutedEventArgs e)
        {
            Trace.WriteLine("Parse clicked");
            //string procedure = StringFromRichTextBox(procedureRichTextBox);
            //foreach(char c in procedure)
            //{
            //    Trace.WriteLine(c + " : " + (int)c);
            //}

            Boolean[,] table = new Boolean[4, 10];
            for(int i=0; i<table.GetLength(0); i++)
            {
                for (int j = 0; j < table.GetLength(1); j++)
                {
                    Trace.Write(table[i,j] + " ");
                }
                Trace.WriteLine("");
            }
            Trace.WriteLine(table.GetLength(0));
            Trace.WriteLine(table.GetLength(1));
        }

        private string StringFromRichTextBox(RichTextBox rtb)
        {
            TextRange textRange = new TextRange(
                // TextPointer to the start of content in the RichTextBox.
                rtb.Document.ContentStart,
                // TextPointer to the end of content in the RichTextBox.
                rtb.Document.ContentEnd
            );

            // The Text property on a TextRange object returns a string
            // representing the plain text content of the TextRange.
            return textRange.Text;
        }

    }
}
