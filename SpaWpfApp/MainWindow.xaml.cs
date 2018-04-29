
using SpaWpfApp.ASTFolder;
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
            } catch
            {
                pkbCreatedLabel.Content = "Error";
                return;
            }

            try
            {
                ASTAPI ast = new AST(parsed, pkb);
                astCreatedLabel.Content = "Yes";
                //Trace.WriteLine(ast.GetParent(8).programLine);
            } catch
            {
                astCreatedLabel.Content = "Error";
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



        private void evaluateQueryButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                String parsedQuery = QueryPreProcessor.GetInstance().Parse(StringFromRichTextBox(queryRichTextBox));
                queryRichTextBox.Document.Blocks.Clear();
                queryRichTextBox.Document.Blocks.Add(new Paragraph(new Run(parsedQuery)));
            }
            catch (QueryException ex)
            {
                MessageBox.Show(ex.Message, "Alert", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (WrongQueryFromatException ex)
            {
                MessageBox.Show(ex.Message, "Alert", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unknown Praser Error in query", "Alert", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}
