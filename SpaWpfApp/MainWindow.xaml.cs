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
        private bool good = false;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void parseButton_Click(object sender, RoutedEventArgs e)
        {
            logsRichTextBox.Document.Blocks.Clear();
            try
            {
                ParserByTombs.SetNewInstance();

                // parsed    - FOR AST AND CFG
                parsed = ParserByTombs.Instance.Parse(StringFromRichTextBox(procedureRichTextBox));

                // formatted - ONLY FOR "MAIN WINDOW"
                var formatted = ParserByTombs.Instance.GetParsedFormattedSourceCode();

                linesRichTextBox.Document.Blocks.Clear();
                linesRichTextBox.Document.Blocks.Add(new Paragraph(new Run(formatted.lineNumbers)));

                procedureRichTextBox.Document.Blocks.Clear();
                procedureRichTextBox.Document.Blocks.Add(new Paragraph(new Run(formatted.parsedSourceCode)));
                //procedureRichTextBox.ScrollToVerticalOffset(0);
                linesRichTextBox.ScrollToVerticalOffset(procedureRichTextBox.VerticalOffset);

                addLog("Source Code Parser: Ok");
            }
            catch (Exception ex)
            {
                addLog("Source Code Parser: " + ex.GetType().Name + ": " + ex);
                good = false;
                return;
            }

            try
            {
                pkb = ParserByTombs.Instance.pkb;
                QueryEvaluator.GetInstance().pkb = pkb;
                QueryProjector.GetInstance().Pkb = pkb;
                addLog("PKB Create: Ok");
            }
            catch (Exception ex)
            {
                addLog("PKB Create: " + ex.GetType().Name + ": " + ex);
                good = false;
                return;
            }

            good = true;
        }

        private void astCfgButton_Click(object sender, RoutedEventArgs e)
        {
            if (!good) return;

            try
            {
                AstManager.GetInstance().GenerateStructures(parsed, pkb);
                addLog("AST Create: Ok");
            }
            catch (Exception ex)
            {
                addLog("AST Create: " + ex.GetType().Name + ": " + ex);
                good = false;
                return;
            }

            try
            {
                CfgManager.GetInstance().GenerateStructure(parsed, pkb);
                addLog("CFG Create: Ok");
            }
            catch (Exception ex)
            {
                addLog("CFG Create: " + ex.GetType().Name + ": " + ex);
                good = false;
                return;
            }
            good = true;
        }

        private void parseQueryButton_Click(object sender, RoutedEventArgs e)
        {
            string query = StringFromRichTextBox(queryRichTextBox);
            try
            {
                query = QueryPreProcessor.GetInstance().Parse(query);

                addLog("PQL Parser: Ok");
            }
            catch (Exception ex)
            {
                addLog("PQL Parser: " + ex.GetType().Name + ": " + ex.Message);
                good = false;
                return;
            }
            finally
            {
                queryRichTextBox.Document.Blocks.Clear();
                queryRichTextBox.Document.Blocks.Add(new Paragraph(new Run(query)));
            }
            good = true;
        }

        private void evaluateQueryButton_Click(object sender, RoutedEventArgs e)
        {
            if (!good) return;
            resultRichTextBox.Document.Blocks.Clear();

            try
            {
                try
                {
                    List<QueryProcessingSusbsytem.Condition> conditionsList = QueryPreProcessor.GetInstance().conditionsList;
                    QueryEvaluator.GetInstance().Evaluate(conditionsList);
                }
                catch (NoResultsException ex)
                {
                    addLog("Q Evaluator: " + ex.GetType().Name + ": " + ex.Message);
                    good = false;
                }
                finally
                {
                    //tutaj QueryProjector wkracza do gry - interpretuje instancję klasy Result
                    QueryResult queryResult = QueryResult.GetInstance();
                    QueryProjector queryProjector = QueryProjector.GetInstance();

                    resultRichTextBox.Document.Blocks.Add(new Paragraph(new Run(queryProjector.PrintResult())));
                    addLog("Q Evaluator: Result: ok, check Result window");
                }
            }
            catch (Exception ex)
            {
                addLog("FATAL ERROR: " + ex.GetType().Name + ": " + ex);
            }
            good = true;
        }

        private void parseAndEvaluateButton_Click(object sender, RoutedEventArgs e)
        {
            if (!good) return;
            this.parseQueryButton_Click(sender, e);
            if (good)
                this.evaluateQueryButton_Click(sender, e);
        }

        private void autoButton_Click(object sender, RoutedEventArgs e)
        {
            parseButton_Click(sender, e);
            if (good)
            {
                astCfgButton_Click(sender, e);
                if (good)
                {
                    parseQueryButton_Click(sender, e);
                    if (good)
                    {
                        evaluateQueryButton_Click(sender, e);
                    }
                }
            }
        }

        private void ProcedureRichTextBox_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            linesRichTextBox.ScrollToVerticalOffset(procedureRichTextBox.VerticalOffset);
        }

        private string StringFromRichTextBox(RichTextBox rtb)
        {
            TextRange textRange = new TextRange(
                rtb.Document.ContentStart,
                rtb.Document.ContentEnd
            );
            return textRange.Text;
        }

        private void addLog(string log)
        {
            string now = DateTime.Now.ToLongTimeString();
            logsRichTextBox.Document.Blocks.Add(new Paragraph(new Run("[" + now + "]" + " " + log)));
            logsRichTextBox.ScrollToEnd();
        }
    }
}
