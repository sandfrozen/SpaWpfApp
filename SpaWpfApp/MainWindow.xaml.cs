using SpaWpfApp.PkbFolder;
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
        private PkbAPI Pkb;

        public MainWindow()
        {
            InitializeComponent();
        }
        private void parseButton_Click(object sender, RoutedEventArgs e)
        {
            logsRichTextBox.Document.Blocks.Clear();
            try
            {
                parsed = ParserByTombs.Instance.Parse(StringFromRichTextBox(procedureRichTextBox));
                addLog("Source Code Parser: Ok");
                return;
            }
            catch (WrongCodeException wce)
            {
                addLog("Error while parsing Source Code:\n" + wce.Message);
                return;
            }

            try
            {
                //pkb = ParserMain.Instance.pkb;
                //pkb.PrintProcTable();
                //pkb.PrintVarTable();
                //pkb.PrintCallsTable();
                //pkb.PrintModifiesTable();
                //pkb.PrintUsesTable();
                //Trace.WriteLine(pkb.GetNumberOfLines());

                //addLog("PKB Created: Ok");

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
                AstManager.GetInstance().GenerateStructures(parsed, Pkb);
                //List<int> result = astManager.GetChildren(6);
                //result = astManager.GetChildrenS(6);
                //result = astManager.GetParentS(6);

                //bool r = astManager.IsFollows(9, 11);
                //r = astManager.IsFollowsS(5, 14);
                //r = astManager.IsParent(8, 10);
                //r = astManager.IsParentS(8, 10);

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
            Result.GetInstance().Init();
            QueryEvaluator.GetInstance().Init();

            try
            {
                foreach (var relation in QueryPreProcessor.GetInstance().relationsList)
                {
                    switch (relation.type)
                    {
                        case Relation.Parent:
                            QueryEvaluator.GetInstance().Parent(relation.arg1, relation.arg2);
                            Result r = Result.GetInstance(); // do testów, potem do usunięcia ta linia
                            break;
                    }
                }
            }
            catch(NoResultsException ex) { addLog("Q Evaluator: NoResultsException:\n" + ex.Message); }
            finally
            {
                //tutaj QueryProjector wkracza do gry - interpretuje instancję klasy Result
            }
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
