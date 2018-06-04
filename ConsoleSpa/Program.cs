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

                SpaWpfApp.Cfg.CfgManager.GetInstance().GenerateStructure(SourceCode, pkb);
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
                query += Console.ReadLine();

                try
                {
                    query = SpaWpfApp.QueryProcessingSusbsytem.QueryPreProcessor.GetInstance().Parse(query);
                    //SpaWpfApp.QueryProcessingSusbsytem.QueryEvaluator.GetInstance().Evaluate(SpaWpfApp.QueryProcessingSusbsytem.QueryPreProcessor.GetInstance().conditionsList);
                    List<SpaWpfApp.QueryProcessingSusbsytem.Condition> conditionsList = SpaWpfApp.QueryProcessingSusbsytem.QueryPreProcessor.GetInstance().conditionsList;
                    SpaWpfApp.QueryProcessingSusbsytem.QueryEvaluator.GetInstance().Evaluate(conditionsList);
                    

                    //Console.WriteLine(SpaWpfApp.QueryProcessingSusbsytem.QueryResult.GetInstance().resultIsBoolean.ToString().ToLower());
                }
                catch (Exception e)
                {
                    //Console.WriteLine(e.GetType().Name + ": " + e.Message);
                }
                finally
                {
                    //tutaj QueryProjector wkracza do gry - interpretuje instancję klasy Result
                    try
                    {
                        SpaWpfApp.QueryProcessingSusbsytem.QueryResult queryResult = SpaWpfApp.QueryProcessingSusbsytem.QueryResult.GetInstance();
                        SpaWpfApp.QueryProcessingSusbsytem.QueryProjector queryProjector = SpaWpfApp.QueryProcessingSusbsytem.QueryProjector.GetInstance();

                        var vfvd = SpaWpfApp.QueryProcessingSusbsytem.QueryPreProcessor.GetInstance();
                        Console.WriteLine(queryProjector.PrintResult());
                    }
                    catch
                    {
                        Console.WriteLine("none");
                    }

                }
            }


            Console.WriteLine("Program exit.");
            Console.ReadKey();
            return;
        }
    }
}
