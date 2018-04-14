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

        private void parseButton_Click(object sender, RoutedEventArgs e)
        {
            Trace.WriteLine("Parse clicked");
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
