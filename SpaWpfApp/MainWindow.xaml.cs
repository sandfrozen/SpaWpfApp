using SpaWpfApp.Ast;
using SpaWpfApp.Cfg;
using SpaWpfApp.Exceptions;
using SpaWpfApp.ParserNew;
using SpaWpfApp.PkbNew;
//using SpaWpfApp.ParserOld;
//using SpaWpfApp.PkbOld;
using SpaWpfApp.QueryProcessingSusbsytem;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

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
        private void parseButton_Click(object sender, RoutedEventArgs e)
        {
            logsRichTextBox.Document.Blocks.Clear();
            try
            {
                // parsed    - FOR AST AND CFG
                parsed = ParserByTombs.Instance.Parse(StringFromRichTextBox(procedureRichTextBox));

                // formatted - ONLY FOR "MAIN WINDOW"
                var formatted = ParserByTombs.Instance.GetParsedFormattedSourceCode();

                linesRichTextBox.Document.Blocks.Clear();
                linesRichTextBox.Document.Blocks.Add(new Paragraph(new Run(formatted.lineNumbers)));

                procedureRichTextBox.Document.Blocks.Clear();
                procedureRichTextBox.Document.Blocks.Add(new Paragraph(new Run(formatted.parsedSourceCode)));
                
                addLog("Source Code Parser: Ok");
                return;
            }
            catch (SourceCodeException wce)
            {
                addLog("Error while parsing Source Code:\n" + wce.Message);
                return;
            }
        }

        private void astCfgButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                pkb = ParserByTombs.Instance.pkb;
                addLog("PKB Created: Ok");
                //pkb.PrintProcTable();
                //pkb.PrintVarTable();
                //pkb.PrintCallsTable();
                //pkb.PrintModifiesTable();
                //pkb.PrintUsesTable();
                //Trace.WriteLine(pkb.GetNumberOfLines());

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
                AstManager.GetInstance().GenerateStructures(parsed, pkb);
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
                CfgManager.GetInstance().GenerateStructure(parsed);
                //bool r = cfgManager.GetInstance().IsNext(11, 13);
                //r = cfgManager.IsNext(9, 12);
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
            catch (Exception ex)
            {
                addLog("PQL Parser: Error:\n" + ex.Message);
                //MessageBox.Show("Unknown Praser Error in query: " + ex.Message, "Alert", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void evaluateQueryButton_Click(object sender, RoutedEventArgs e)
        {
            resultRichTextBox.Document.Blocks.Clear();

            try
            {
                List<Relation> relationList = QueryPreProcessor.GetInstance().relationsList;
                QueryEvaluator.GetInstance().Evaluate(relationList);
            }
            catch (NoResultsException ex) { addLog("Q Evaluator: NoResultsException:\n" + ex.Message); }
            finally
            {
                //tutaj QueryProjector wkracza do gry - interpretuje instancję klasy Result
                QueryResult queryResult = QueryResult.GetInstance();
                if (queryResult.resultIsBoolean)
                {
                    resultRichTextBox.Document.Blocks.Add(new Paragraph(new Run(queryResult.resultBoolean.ToString().ToLower())));
                    addLog("Result: " + queryResult.resultBoolean.ToString().ToLower());
                }
                else
                {
                    string now = DateTime.Now.ToLongTimeString();
                    resultRichTextBox.Document.Blocks.Add(new Paragraph(new Run("[" + now + "]" + " result")));
                    addLog("Result: NotSupported");
                }

            }



        }

        private void addLog(string log)
        {
            string now = DateTime.Now.ToLongTimeString();
            logsRichTextBox.Document.Blocks.Add(new Paragraph(new Run("[" + now + "]" + " " + log)));
            logsRichTextBox.ScrollToEnd();
        }
    }
}
