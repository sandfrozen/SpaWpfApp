
using SpaWpfApp.Ast;
using SpaWpfApp.Cfg;
using SpaWpfApp.Exceptions;
using SpaWpfApp.Parser;
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

        private int numberOfLines;
        private int numberOfProcs;
        private int numerOfVars;
        private string parsed;
        private PkbAPI pkb;

        public MainWindow()
        {
            InitializeComponent();
        }
        private void formatButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                parsed = ParserMain.Instance.Parse(StringFromRichTextBox(procedureRichTextBox));
                Trace.WriteLine("===================LAST PARSER======================");
                Trace.WriteLine(parsed);
                procedureRichTextBox.Document.Blocks.Clear();
                procedureRichTextBox.Document.Blocks.Add(new Paragraph(new Run(parsed)));
                MessageBox.Show("Code is ok", "Alert", MessageBoxButton.OK, MessageBoxImage.Information);
                Trace.WriteLine("Lines " + ParserMain.Instance.numberOfLines);
                Trace.WriteLine("procedures " + ParserMain.Instance.numberOfProcedures);
            }
            catch (WrongCodeException)
            {
                MessageBox.Show("Error in code", "Alert", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        private void parseButton_Click(object sender, RoutedEventArgs e)
        {
            SetLogLabels("no");
            Trace.WriteLine("Parse clicked");
            //string code =
            //    "procedure First {" + Environment.NewLine +
            //    "x = 2 ;" + Environment.NewLine +
            //    "z = 3 ;" + Environment.NewLine +
            //    "call Second ; }" + Environment.NewLine +
            //    "procedure Second {" + Environment.NewLine +
            //    "x = 0 ;" + Environment.NewLine +
            //    "i = 5 ;" + Environment.NewLine +
            //    "while i {" + Environment.NewLine +
            //    "x = x + 2 * y ;" + Environment.NewLine +
            //    "call Third ;" + Environment.NewLine +
            //    "i = i - 1 ; }" + Environment.NewLine +
            //    "if x then {" + Environment.NewLine +
            //    "x = x + 1 ; }" + Environment.NewLine +
            //    "else {" + Environment.NewLine +
            //    "z = 1 ; }" + Environment.NewLine +
            //    "z = z + x + i ;" + Environment.NewLine +
            //    "y = z + 2 ;" + Environment.NewLine +
            //    "x = x * y + z ; }" + Environment.NewLine +
            //    "procedure Third {" + Environment.NewLine +
            //    "z = 5 ;" + Environment.NewLine +
            //    "v = z ; }";

            //Trace.Write(code);
            try
            {
                parsed = ParserMain.Instance.Parse(StringFromRichTextBox(procedureRichTextBox));
                Trace.WriteLine("===================LAST PARSER======================");
                Trace.WriteLine(parsed);
                parsedLabel.Content = "Yes";
                //MessageBox.Show("Code is ok", "Alert", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (WrongCodeException)
            {
                parsedLabel.Content = "Error: " + MessageBoxImage.Information;
                //MessageBox.Show("Error in code", "Alert", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                pkb = ParserMain.Instance.pkb;
                pkb.PrintProcTable();
                pkb.PrintVarTable();
                pkb.PrintCallsTable();
                pkb.PrintModifiesTable();
                pkb.PrintUsesTable();
                Trace.WriteLine(pkb.GetNumberOfLines());
                pkbCreatedLabel.Content = "Yes";

                //parsed = System.IO.File.ReadAllText(@"C:\Users\Slightom\OneDrive\semestr 2.1\1 ATS\sparsowanySourceCodeDlaAst4.txt");

                //pkb = new Pkb(20, 1, 3);
                //pkb.InsertProc("p500");

                //pkb.InsertVar("x");
                //pkb.InsertVar("i");
                //pkb.InsertVar("y");
            }
            catch
            {
                pkbCreatedLabel.Content = "Error";
                return;
            }

            try
            {
                AstAPI astManager = new AstManager(parsed, pkb);
                List<int> result = astManager.GetChildren(6);
                result = astManager.GetChildrenS(6);
                result = astManager.GetParentS(6);

                bool r = astManager.IsFollows(9, 11);
                r = astManager.IsFollowsS(5, 14);
                r = astManager.IsParent(8, 10);
                r = astManager.IsParentS(8, 10);
                astCreatedLabel.Content = "Yes";
            }
            catch
            {
                astCreatedLabel.Content = "Error";
                return;
            }

            try
            {
                CfgAPI cfgManager = new CfgManager(parsed);
                bool r = cfgManager.IsNext(11, 13);
                r = cfgManager.IsNext(9, 12);
                cfgCreatedLabel.Content = "Yes";
            }
            catch
            {
                cfgCreatedLabel.Content = "Error";
                return;
            }
        }

        private string StringFromRichTextBox(RichTextBox rtb)
        {
            TextRange textRange = new TextRange(
                rtb.Document.ContentStart,
                rtb.Document.ContentEnd
            );
            return textRange.Text;
        }

        private void SetLogLabels(String value)
        {
            parsedLabel.Content = value;
            pkbCreatedLabel.Content = value;
            astCreatedLabel.Content = value;
        }

    }
}
