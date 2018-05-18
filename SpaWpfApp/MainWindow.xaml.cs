
using SpaWpfApp.Ast;
using SpaWpfApp.Cfg;
using SpaWpfApp.Exceptions;
using SpaWpfApp.Parser;
using SpaWpfApp.QueryProcessingSusbsytem;
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
            logsRichTextBox.Document.Blocks.Clear();
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

                addLog("Source Code Parser: Ok");
                //MessageBox.Show("Code is ok", "Alert", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (WrongCodeException ex)
            {
                addLog("Source Code Parser: Error:\n" + ex);
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

                addLog("PKB Created: Ok");

                //parsed = System.IO.File.ReadAllText(@"C:\Users\Slightom\OneDrive\semestr 2.1\1 ATS\sparsowanySourceCodeDlaAst4.txt");

                //pkb = new Pkb(20, 1, 3);
                //pkb.InsertProc("p500");

                //pkb.InsertVar("x");
                //pkb.InsertVar("i");
                //pkb.InsertVar("y");
            }
            catch (Exception ex)
            {
                addLog("PKB Created: Error:\n" + ex);
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

                addLog("AST Created: Ok");
            }
            catch (Exception ex)
            {
                addLog("AST Created: Error:\n" + ex);
                return;
            }

            try
            {
                CfgAPI cfgManager = new CfgManager(parsed);
                bool r = cfgManager.IsNext(11, 13);
                r = cfgManager.IsNext(9, 12);
                addLog("CFG Created: Ok");
            }
            catch (Exception ex)
            {
                addLog("CFG Created: Error:\n" + ex);
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

        private void parseQueryButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                String parsedQuery = QueryPreProcessor.GetInstance().Parse(StringFromRichTextBox(queryRichTextBox));
                queryRichTextBox.Document.Blocks.Clear();
                queryRichTextBox.Document.Blocks.Add(new Paragraph(new Run(parsedQuery)));
                addLog("PQL Parser: Ok");
            }
            catch (QueryException ex)
            {
                addLog("PQL Parser: QueryException:\n" + ex.Message);
                //MessageBox.Show(ex.Message, "Alert", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (WrongQueryFromatException ex)
            {
                addLog("PQL Parser: WrongQueryFromatException:\n" + ex.Message);
                //MessageBox.Show(ex.Message, "Alert", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                addLog("PQL Parser: Error:\n" + ex.Message);
                //MessageBox.Show("Unknown Praser Error in query: " + ex.Message, "Alert", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void evaluateQueryButton_Click(object sender, RoutedEventArgs e)
        {
            resultRichTextBox.Document.Blocks.Clear();
            string now = DateTime.Now.ToLongTimeString();
            resultRichTextBox.Document.Blocks.Add(new Paragraph(new Run("[" + now + "]" + " result")));
        }

        private void addLog(string log)
        {
            string now = DateTime.Now.ToLongTimeString();
            logsRichTextBox.Document.Blocks.Add(new Paragraph(new Run("[" + now + "]" + " " + log)));
            logsRichTextBox.ScrollToEnd();
        }
    }
}
