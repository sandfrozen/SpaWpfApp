using SpaWpfApp.Exceptions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SpaWpfApp.QueryProcessingSusbsytem
{
    class QueryPreProcessor
    {
        private string parsedQuery;

        private string[] wordsInQuery;
        private int currentIndex;

        private List<string> selectClauses;
        private Dictionary<string, Action> relationshipReferences;
        public Dictionary<string, string> declarationsList { get; set; }
        public  Dictionary<string, string> returnList { get; set; }

        private Dictionary<string, string[]> entityAttributeValue;
        private Dictionary<string, Action> declarationActions;

        private static QueryPreProcessor instance;
        public static QueryPreProcessor GetInstance()
        {
            if (instance == null)
            {
                instance = new QueryPreProcessor();
            }
            return instance;
        }

        public QueryPreProcessor()
        {
            declarationsList = new Dictionary<string, string>();
            returnList = new Dictionary<string, string>();
            relationshipReferences = new Dictionary<string, Action> {
                { "Modifies", CheckModifies},
                { "Modifies*", CheckModifies},
                { "Uses", CheckModifies},
                { "Uses*", CheckModifies},
                { "Calls", CheckModifies},
                { "Calls*", CheckModifies},
                { "Parent", CheckModifies},
                { "Parent*", CheckModifies},
                { "Follows", CheckModifies},
                { "Follows*", CheckModifies},
                { "Next", CheckModifies},
                { "Next*", CheckModifies},
                { "Affects", CheckModifies},
                { "Affects*",CheckModifies },
            };
            selectClauses = new List<string>
            {
                "such", "pattern", "with", "and"
            };

            entityAttributeValue = new Dictionary<string, string[]>
            {
                {"procedure", new string[] { "procName", "string" } },
                {"stmt", new string[] { "stmt#", "int" } },
                {"assign", new string[] { "stmt#", "int" } },
                {"call", new string[] { "procName", "string" } },
                {"variable", new string[] { "varName", "string" } },
                {"constatnt", new string[] { "value", "int" } },
            };

            declarationActions = new Dictionary<string, Action>{
              {"procedure", ParseDeclaration},
              {"stmtLst", ParseDeclaration},
              {"stmt", ParseDeclaration},
              {"assign", ParseDeclaration},
              {"call", ParseDeclaration},
              {"while", ParseDeclaration},
              {"if", ParseDeclaration},
              {"variable", ParseDeclaration},
              {"constant", ParseDeclaration},
              {"prog_line", ParseDeclaration},
            };
        }

        private void CheckModifies()
        {

        }

        public string Parse(string query)
        {
            declarationsList = new Dictionary<string, string>();
            returnList = new Dictionary<string, string>();
            parsedQuery = "";
            if (!query.Contains("Select"))
            {
                throw new WrongQueryFromatException("Select is missing.");
            }
            wordsInQuery = GetWordsInCode(query);
            currentIndex = 0;
            if (wordsInQuery[0] != "Select")
            {
                for (currentIndex = 0; currentIndex < wordsInQuery.Length; currentIndex++)
                {
                    if (declarationActions.Keys.Any(k => k == wordsInQuery[currentIndex]))
                    {
                        declarationActions[wordsInQuery[currentIndex]]();

                    }
                    else if (wordsInQuery[currentIndex] == "Select")
                    {
                        break;
                    }
                    else
                    {
                        throw new WrongQueryFromatException("Invalid word or character in declaration: " + wordsInQuery[currentIndex]);
                    }
                }
            }
            if (wordsInQuery[currentIndex] != "Select")
            {
                throw new WrongQueryFromatException("Select not found: " + wordsInQuery[currentIndex]);
            }
            parsedQuery += wordsInQuery[currentIndex++];
            ParseTouple();
            while (currentIndex < wordsInQuery.Length)
            {
                if (wordsInQuery[currentIndex] == "such")
                {
                    parsedQuery += " " + wordsInQuery[currentIndex++];
                    if (wordsInQuery[currentIndex] == "that")
                    {
                        parsedQuery += " " + wordsInQuery[currentIndex];
                    }
                    else
                    {
                        throw new WrongQueryFromatException("\"that\" after \"such\" not found: " + wordsInQuery[currentIndex]);
                    }
                    do
                    {
                        ++currentIndex;
                        ParseSuchThat();
                        if (currentIndex < wordsInQuery.Length && wordsInQuery[currentIndex] == "and")
                        {
                            parsedQuery += " and";
                        }
                    } while (currentIndex < wordsInQuery.Length && wordsInQuery[currentIndex] == "and");
                }

                else if (wordsInQuery[currentIndex] == "with")
                {
                    parsedQuery += " " + wordsInQuery[currentIndex];
                    do
                    {
                        ++currentIndex;
                        ParseWith();
                        if (currentIndex < wordsInQuery.Length && wordsInQuery[currentIndex] == "and")
                        {
                            parsedQuery += " and";
                        }
                    } while (currentIndex < wordsInQuery.Length && wordsInQuery[currentIndex] == "and");
                }

                else if (wordsInQuery[currentIndex] == "pattern")
                {
                    parsedQuery += " " + wordsInQuery[currentIndex];
                    do
                    {
                        ++currentIndex;
                        ParsePattern();
                        if (currentIndex < wordsInQuery.Length && wordsInQuery[currentIndex] == "and")
                        {
                            parsedQuery += " and";
                        }
                    } while (currentIndex < wordsInQuery.Length && wordsInQuery[currentIndex] == "and");
                }

                else
                {
                    throw new WrongQueryFromatException("(such that|with|pattern) not found: " + wordsInQuery[currentIndex]);
                }
            }


            Trace.WriteLine("Declarations:");
            foreach (var v in declarationsList)
            {
                Trace.WriteLine(v.Value + " " + v.Key);
            }
            Trace.WriteLine("Return List:");
            foreach (var v in returnList)
            {
                Trace.WriteLine(v.Value + " " + v.Key);
            }

            return parsedQuery;
        }

        private string[] GetWordsInCode(string query)
        {
            for (int i = 0; i < query.Length; i++)
            {
                if (i < query.Length - 1 && IsSeparatorChar(query[i]))
                {
                    query = query.Insert(i, " ");
                    query = query.Insert(i + 2, " ");
                    i = i + 2;
                }
            }
            string[] separators = new string[] { " ", Environment.NewLine };
            return query.Split(separators, StringSplitOptions.RemoveEmptyEntries);
        }

        private bool IsSeparatorChar(char toCheck)
        {
            char[] chars = { '(', ')', ',', ';', '<', '>' };
            foreach (char c in chars)
            {
                if (c == toCheck) return true;
            }
            return false;
        }

        private void CkeckIsDeclared(string synonym)
        {
            if (!declarationsList.Keys.Contains(synonym))
            {
                throw new QueryException("Synonym used in Select is not declared: " + synonym);
            }
        }

        private void CheckTuple(string tuple)
        {
            if (tuple.First() == '<' && tuple.Last() == '>')
            {
                ExtractMultipleTuple(tuple);
            }
            else
            {
                if (tuple.Contains("."))
                {
                    string synonym = tuple.Substring(0, tuple.IndexOf('.'));
                    CkeckIsDeclared(synonym);
                    string attrName = tuple.Substring(tuple.IndexOf('.') + 1);
                    string designEntity = declarationsList[synonym];

                    switch (attrName)
                    {
                        case "stmt#":
                            if (designEntity != "assign" && designEntity != "stmt")
                                throw new QueryException("Synonym: " + synonym + " can't be used with: " + attrName);
                            break;
                        case "procName":
                            if (designEntity != "procedure" && designEntity != "call")
                                throw new QueryException("Synonym: " + synonym + " can't be used with: " + attrName);
                            break;
                        case "varName":
                            if (designEntity != "variable")
                                throw new QueryException("Synonym: " + synonym + " can't be used with: " + attrName);
                            break;
                        case "value":
                            if (designEntity != "constant")
                                throw new QueryException("Synonym: " + synonym + " can't be used with: " + attrName);
                            break;
                        default:
                            throw new QueryException("Attrybute name: " + attrName + " is unknown");
                    }
                    returnList.Add(tuple, designEntity);
                }
                else
                {
                    CkeckIsDeclared(tuple);
                    returnList.Add(tuple, declarationsList[tuple]);
                }
            }
        }

        private void ExtractMultipleTuple(string tuple)
        {
            for (int elemIndex = 1; elemIndex < tuple.Length - 1; elemIndex++)
            {
                string elem = "";
                do
                {
                    elem += tuple[elemIndex++];
                } while (elemIndex < tuple.Length && tuple[elemIndex] != ',' && tuple[elemIndex] != '>');
                CheckTuple(elem);
            }
        }

        private void IsSynonym(string synonym)
        {
            if (!Regex.IsMatch(synonym, @"^([a-zA-Z]){1}([a-zA-Z]|[0-9]|[#])*$"))
            {
                throw new WrongQueryFromatException("Invalid synonym: " + synonym);
            }
        }

        private void CheckSynonyms(string declaration)
        {
            try
            {
                IsSynonym(declaration);
            }
            catch
            {
                for (int declarationIndex = 0; declarationIndex < declaration.Length - 1; declarationIndex++)
                {
                    string synonym = "";
                    do
                    {
                        synonym += declaration[declarationIndex++];
                    } while (declarationIndex < declaration.Length && declaration[declarationIndex] != ',' && declaration[declarationIndex] != ';');

                    IsSynonym(synonym);
                }
            }
        }

        private void ParseDeclaration()
        {
            List<String> synonymsList = new List<string>();
            string declaration = wordsInQuery[currentIndex] + " ";
            while (wordsInQuery[currentIndex] != ";")
            {
                declaration += wordsInQuery[++currentIndex];
                if (wordsInQuery[currentIndex] != "," && wordsInQuery[currentIndex] != ";")
                {
                    synonymsList.Add(wordsInQuery[currentIndex]);
                }
                if (wordsInQuery[currentIndex] != "," && wordsInQuery[currentIndex] != ";" && wordsInQuery[currentIndex + 1] != "," && wordsInQuery[currentIndex + 1] != ";")
                {
                    declaration += " ";
                }
            }
            string designEntity = declaration.Substring(0, declaration.IndexOf(' '));
            string synonyms = declaration.Substring(declaration.IndexOf(' ') + 1);
            CheckSynonyms(synonyms);

            try
            {
                foreach (string synonym in synonymsList)
                {
                    declarationsList.Add(synonym, designEntity);
                }
            }
            catch
            {
                throw new QueryException("Synonyms can't have same names.\nCheck declaration:  " + declaration);
            }


            parsedQuery += declaration;
            if (wordsInQuery[currentIndex + 1] == "Select")
            {
                parsedQuery += Environment.NewLine;
            }
            else
            {
                parsedQuery += " ";
            }
        }

        private void ParseTouple()
        {
            string tuple = "";
            do
            {
                tuple += wordsInQuery[currentIndex++];
            } while (currentIndex < wordsInQuery.Length && wordsInQuery[currentIndex] != "such" && wordsInQuery[currentIndex] != "with" && wordsInQuery[currentIndex] != "pattern");
            if (tuple == "BOOLEAN")
            {
                returnList.Add("BOOLEAN", "BOOLEAN");
            }
            else
            {
                CheckTuple(tuple);
            }
            parsedQuery += " " + tuple;
        }

        private void ParseSuchThat()
        {
            string relationship = "";
            relationship += wordsInQuery[currentIndex++];
            if (!relationshipReferences.Keys.Contains(relationship))
            {
                throw new QueryException("Unknown Relationship Reference: " + relationship);
            }
            while (currentIndex < wordsInQuery.Length && !selectClauses.Contains(wordsInQuery[currentIndex]))
            {
                relationship += wordsInQuery[currentIndex++];
            }

            CheckRelationship(relationship);



            Trace.WriteLine("Relationship: " + relationship);
            parsedQuery += " " + relationship;
        }

        private void CheckRelationship(string relationship)
        {
            if (Regex.IsMatch(relationship, @"^[^(,) ]+[(][^(,) ]+[,][^(,) ]+[)]$"))
            {
                string relRef = relationship.Substring(0, relationship.IndexOf('('));
                relationship = relationship.Substring(relationship.IndexOf('(') + 1);
                string firstArg = relationship.Substring(0, relationship.IndexOf(','));
                relationship = relationship.Substring(relationship.IndexOf(',') + 1);
                string secArg = relationship.Substring(0, relationship.IndexOf(')'));

                switch (relRef)
                {
                    case "ModifiesP":
                    case "ModifiesS":
                        break;
                    case "UsesP":
                    case "UsesS":
                        break;
                    case "Calls":
                    case "CallsT":
                        break;
                    case "Parent":
                    case "ParentT":
                        break;
                    case "Follows":
                    case "FollowsT":
                        break;
                    case "Next":
                    case "NextT":
                        break;
                    case "Affects":
                    case "AffectsT":
                        break;
                }
            }
            else
            {
                throw new QueryException("Relationship wrong format: " + relationship);
            }
        }

        private void ParseWith()
        {
            string with = "";
            do
            {
                with += wordsInQuery[currentIndex++];
            } while (currentIndex < wordsInQuery.Length && !selectClauses.Contains(wordsInQuery[currentIndex]));
            Trace.WriteLine("With: " + with);
            CheckWith(with);

            parsedQuery += " " + with;
        }

        private void CheckWith(string with)
        {
            if (Regex.IsMatch(with, @"^[^= ]+[=][^= ]+$"))
            {
                string leftRef = with.Substring(0, with.IndexOf('='));
                CheckTuple(leftRef);

                with = with.Substring(with.IndexOf('=') + 1);
                string rightRef = with.Substring(0);
            }
            else
            {
                throw new QueryException("With wrong format: " + with);
            }
        }

        private void ParsePattern()
        {
            string pattern = "";
            do
            {
                pattern += wordsInQuery[currentIndex++];
            } while (currentIndex < wordsInQuery.Length && !selectClauses.Contains(wordsInQuery[currentIndex]));
            Trace.WriteLine("Pattern: " + pattern);

            CheckPattern(pattern);
            parsedQuery += " " + pattern;
        }

        private void CheckPattern(string pattern)
        {
            if (Regex.IsMatch(pattern, @"^[^(,) ]+[(][^(,) ]+(,[^, ]+){0,2}[)]$"))
            {
                string synonym = pattern.Substring(0, pattern.IndexOf('('));
                if (!declarationsList.Keys.Contains(synonym))
                {
                    throw new QueryException("Synonym used in pattern is not declared: " + synonym + "\nCheck pattern: " + pattern);
                }
                string assignWhileIf = declarationsList[synonym];
                string args = pattern.Substring(pattern.IndexOf('(') + 1);
                switch (assignWhileIf)
                {
                    case "assign":
                        if (Regex.IsMatch(args, @"^[^(,) ]+[,][^(,) ]+[)]$"))
                            CheckAssignPattern(args);
                        else
                            throw new QueryException("Wrong pattern for assign: " + pattern);
                        break;
                    case "while":
                        if (Regex.IsMatch(args, @"^[^(,) ]+[,][_][)]$"))
                            CheckWhilePattern(args);
                        else
                            throw new QueryException("Wrong pattern for while: " + pattern);
                        break;
                    case "if":
                        if (Regex.IsMatch(args, @"^[^(,) ]+[,][_][,][_][)]$"))
                            CheckIfPattern(args);
                        else
                            throw new QueryException("Wrong pattern for if: " + pattern);
                        break;
                }

            }
            else
            {
                throw new QueryException("With wrong format: " + pattern);
            }
        }

        private void CheckAssignPattern(string pattern)
        {
            string firstArg = pattern.Substring(0, pattern.IndexOf(','));
            pattern = pattern.Substring(pattern.IndexOf(',')+1);
            string secArg = pattern.Substring(0, pattern.IndexOf(')'));
            //check arguments

        }
        private void CheckWhilePattern(string pattern)
        {
            string firstArg = pattern.Substring(0, pattern.IndexOf(','));
            pattern = pattern.Substring(pattern.IndexOf(',') + 1);
            string secArg = pattern.Substring(0, pattern.IndexOf(')'));
            //check arguments

        }
        private void CheckIfPattern(string pattern)
        {
            string firstArg = pattern.Substring(0, pattern.IndexOf(','));
            pattern = pattern.Substring(pattern.IndexOf(',') + 1);
            string secArg = pattern.Substring(0, pattern.IndexOf(')'));
            //check arguments

        }
    }
}
