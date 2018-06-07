using SpaWpfApp.Ast;
using SpaWpfApp.Cfg;
using SpaWpfApp.Exceptions;
using SpaWpfApp.ParserNew;
using SpaWpfApp.PkbNew;
using SpaWpfApp.QueryProcessingSusbsytem;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ConsoleSpa
{
    class Program
    {

        static void Main(string[] args)
        {
            string SourceCode = "";
            PkbAPI pkb;
            if (args.Length == 0)
            {
                Console.WriteLine("No parameter with path to file");
                Console.WriteLine("Program exit.");
                return;
            }

            string path = args[0];
            //Console.WriteLine("Path: " + path);

            try
            {
                SourceCode = File.ReadAllText(path);
                Console.WriteLine("File ok: " + path);

                SourceCode = ParserByTombs.Instance.Parse(SourceCode);
                Console.WriteLine("Source Parsed ok");

                pkb = ParserByTombs.Instance.pkb;
                QueryEvaluator.GetInstance().pkb = pkb;
                QueryProjector.GetInstance().Pkb = pkb;
                Console.WriteLine("PKB ok");

                AstManager.GetInstance().GenerateStructures(SourceCode, pkb);
                Console.WriteLine("AST ok");

                CfgManager.GetInstance().GenerateStructure(SourceCode, pkb);
                Console.WriteLine("CFG ok");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.GetType().Name + ": " + ex);
                return;
            }
            Console.WriteLine("Ready");

            while (true)
            {
                string query = Console.ReadLine();
                query += " " + Console.ReadLine();

                try
                {
                    query = QueryPreProcessor.GetInstance().Parse(query);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.GetType().Name + ": " + ex.Message);
                    continue;
                }

                try
                {
                    List<Condition> conditionsList = QueryPreProcessor.GetInstance().conditionsList;
                    QueryEvaluator.GetInstance().Evaluate(conditionsList);
                }
                catch (Exception ex)
                {

                }
                finally
                {
                    try
                    {
                        QueryResult queryResult = QueryResult.GetInstance();
                        QueryProjector queryProjector = QueryProjector.GetInstance();
                        Console.WriteLine(queryProjector.PrintResult());
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("none");
                    }
                }
            }

            Console.WriteLine("Program exit.");
            return;
        }
    }
}
