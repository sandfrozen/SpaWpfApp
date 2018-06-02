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
            SpaWpfApp.PkbNew.PkbAPI pkb;
            if (args.Length == 0)
            {
                Console.WriteLine("No parameter (Source.txt)");
                Console.WriteLine("Program exit.");
                Console.ReadKey();
                return;
            }

            string path = args[0];
            Console.WriteLine("Path: " + path);

            try
            {
                SourceCode = File.ReadAllText(path);
                Console.WriteLine("File Readed ok");

                SourceCode = SpaWpfApp.ParserNew.ParserByTombs.Instance.Parse(SourceCode);
                Console.WriteLine("Source Parsed ok");

                pkb = SpaWpfApp.ParserNew.ParserByTombs.Instance.pkb;
                Console.WriteLine("PKB ok");

                SpaWpfApp.Ast.AstManager.GetInstance().GenerateStructures(SourceCode, pkb);
                Console.WriteLine("AST ok");

                SpaWpfApp.Cfg.CfgManager.GetInstance().GenerateStructure(SourceCode);
                Console.WriteLine("CFG ok");

            }
            catch (Exception e)
            {
                Console.WriteLine(e.GetType().Name + ": " + e.Message);
                Console.ReadKey();
                return;
            }
            Console.WriteLine("Ready");

            while (true)
            {
                string query = Console.ReadLine();
                if (query == "exit") break;
                query += Console.ReadLine();

                try
                {
                    query = SpaWpfApp.QueryProcessingSusbsytem.QueryPreProcessor.GetInstance().Parse(query);
                    SpaWpfApp.QueryProcessingSusbsytem.QueryEvaluator.GetInstance().Evaluate(SpaWpfApp.QueryProcessingSusbsytem.QueryPreProcessor.GetInstance().conditionsList);
                    
                    Console.WriteLine(SpaWpfApp.QueryProcessingSusbsytem.QueryResult.GetInstance().resultIsBoolean.ToString().ToLower());
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.GetType().Name + ": " + e.Message);
                }
            }


            Console.WriteLine("Program exit.");
            Console.ReadKey();
            return;
        }
    }
}
