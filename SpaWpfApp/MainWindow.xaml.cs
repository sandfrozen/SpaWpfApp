using SpaWpfApp.ASTFolder;
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

        public MainWindow()
        {
            InitializeComponent();
        }
        private void formatButton_Click(object sender, RoutedEventArgs e)
        {
           
            try
            {
                string parsed = ParserMain.Instance.Parse(StringFromRichTextBox(procedureRichTextBox));
                Trace.WriteLine("===================LAST PARSER======================");
                Trace.WriteLine(parsed);
                procedureRichTextBox.Document.Blocks.Clear();
                procedureRichTextBox.Document.Blocks.Add(new Paragraph(new Run(parsed)));
                MessageBox.Show("Code is ok", "Alert", MessageBoxButton.OK, MessageBoxImage.Information);
                Trace.WriteLine("Lines " + ParserMain.Instance.numberOfLines);
                Trace.WriteLine("procedures " + ParserMain.Instance.numberOfProcedures);
            }
            catch (Exception)
            {
                MessageBox.Show("Error in code", "Alert", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        private void parseButton_Click(object sender, RoutedEventArgs e)
        {
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
                string parsed = ParserMain.Instance.Parse(StringFromRichTextBox(procedureRichTextBox));
                Trace.WriteLine("===================LAST PARSER======================");
                Trace.WriteLine(parsed);

                MessageBox.Show("Code is ok", "Alert", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception)
            {
                MessageBox.Show("Error in code", "Alert", MessageBoxButton.OK, MessageBoxImage.Information);
            }

            // Tutaj będzie wywołanie Klasy Parsującej Adama
            // ex. Parser parser = new Parser(sourceCode);
            // parser.parse();
            // po tym: jeśli sourceCode zostanie sparsowany poprawanie to
            // obiekt parser będzie posiadał trzy kluczowe informacje o sourceCode:
            // - numberOfLines
            // - numberOfProcs
            // - numerOfVars
            // ^ te trzy wartości są potrzebne aby klasa PKB mogła się zaincjować, czyli żeby API działało:
            // Pkb pkb = new Pkb(numberOfLines, numberOfProcs, numberOfVars); // <- to te trzy kluczowe wartości w argumentach
            // Jak już to będzie to wtedy trzeba będzie napisać analize kodu, która wykorzystując API ^ wczyta te wszystkie dane do tablic,
            // Po tym, Kod Tomka Suchwałko będzie mógł zbudować drzewko AST,
            // a po tym "Query processing subsystem (Query Preprocessor, Query Evaluator and query result projector)" Zakrysia i Lucato będzie mógł działać i dawać odpowiedzi na zapytania.
            //
            //
            // To pseudo kod i pseudo rozkmina, ale może komuś to pomoże w naświetleniu tego co będzie się tu działo (i co może zaimplementować wcześniej)

            //TS: for test

            string sourceCode = System.IO.File.ReadAllText(@"C:\Users\Slightom\OneDrive\semestr 2.1\1 ATS\sparsowanySourceCodeDlaAst.txt");

            Pkb pkb= new Pkb(15, 3, 5);
            pkb.InsertProc("First");
            pkb.InsertProc("Second");
            pkb.InsertProc("Third");

            pkb.InsertVar("x");
            pkb.InsertVar("z");
            pkb.InsertVar("i");
            pkb.InsertVar("y");
            pkb.InsertVar("v");

            AST ast = new AST(sourceCode, pkb);
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
