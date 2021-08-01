using System;
using System.Collections.Generic;
using System.Text;
using Antlr4.Runtime.Misc;

namespace QueryLanguage
{


    public record Expr(string type, string value)
    {
        StringBuilder builder = new StringBuilder();
        public override string ToString()
        {
            builder.Append("{");
            builder.Append(type);
            builder.Append(":");
            builder.Append(value);
            builder.Append("}");
            return builder.ToString();
        }
    }
    public class QueryLanguageVisitor : QueryBaseVisitor<string>
    {

        private BuildMongoQuery buildMongoQuery;
        private static Stack<object> elements = new Stack<object>();
        private static Stack<object> open = new Stack<object>();
        public string ToJson(string f, string text)
        {
            var builder = new StringBuilder();
            builder.Append("{");

            builder.Append(f + ":");
            builder.Append(text);
            builder.Append("}");
            return builder.ToString();

        }
        public QueryLanguageVisitor()
        {
            buildMongoQuery=new BuildMongoQuery(elements);
        }
        public string ToJson(string op, string f, string text)
        {
            var builder = new StringBuilder();
            builder.Append("{");
            builder.Append(op + ":{");
            builder.Append(f + ":");
            builder.Append(text);
            builder.Append("}}");
            return builder.ToString();

        }
        private void Log(string message, ConsoleColor color, string text)
        {
            Console.ForegroundColor = color;
            Console.Write("\t" + message + "\t" + text);
            System.Console.WriteLine();
            Console.ResetColor();


        }

        private string query = "";
        private Stack<string> stack = new Stack<string>();

        public override string VisitQuery(QueryParser.QueryContext context)
        {
            Log("VisitQuery", ConsoleColor.Red, context.GetText());

            Visit(context.select_stmt());
            Visit(context.from_stmt());
            Visit(context.where_stmt());

            return query;


        }


        public override string VisitField([NotNull] QueryParser.FieldContext context)
        {
           

            return (context.GetText());
        }
        public override string VisitWhere_stmt([NotNull] QueryParser.Where_stmtContext context)
        {
           
            foreach (var x in context.search_condition())
            {
              
                query += Visit(x);

            }
          
            return query;


        }
        public override string VisitFrom_stmt(QueryParser.From_stmtContext context)
        {


            Log("VisitFrom_stmt", ConsoleColor.Gray, context.GetText());
            elements.Push(context.GetText());
            return null;
        }

        public override string VisitSearch_condition([NotNull] QueryParser.Search_conditionContext context)
        {

           VisitChildren(context);
            return query; ;
        }


        public override string VisitFunction_predicate([NotNull] QueryParser.Function_predicateContext context)

        {
            return VisitChildren(context);
        }
        public override string VisitPredicate([NotNull] QueryParser.PredicateContext context)
        {


            Log("VisitPredicate", ConsoleColor.Gray, context.GetText());
            if (context.ChildCount == 1)
            {
                var expression = this.Visit(context.children[0]);

            }
            else
            {
                var op = this.Visit(context.children[0]);
                var expression = this.Visit(context.children[1]); ;
                buildMongoQuery.Parse(op);
            }
            return query;



        }
        public override string VisitComparison_predicate([NotNull] QueryParser.Comparison_predicateContext context)
        {

            Log("VisitComparison_predicate", ConsoleColor.Gray, context.GetText());
            var e = new Expression();

            var field = Visit(context.children[0]);
            var op = Visit(context.children[1]);
            var value = Visit(context.children[2]);
            var l1 = new Expr(field, value);
            elements.Push(l1);
            return query;

        }
        public override string VisitRange_op([NotNull] QueryParser.Range_opContext context)
        {
            return this.Visit(context);
        }
        public override string VisitAnd([NotNull] QueryParser.AndContext context)
        {
            Log("VisitAnd", ConsoleColor.Gray, context.GetText());
           
            return "$and";

        }
       

        public override string VisitOr([NotNull] QueryParser.OrContext context)
        {
            Log("VisitOr", ConsoleColor.Gray, context.GetText());

            return "$or";
        }
        public override string VisitEquals([NotNull] QueryParser.EqualsContext context)
        {
            Log("VisitEquals", ConsoleColor.Gray, context.GetText());
          
            return "$eq";

        }
        public override string VisitTerm([NotNull] QueryParser.TermContext context)
        {
            Log("VisitTerm", ConsoleColor.White, context.GetText());
           
            return context.GetText();
        }
        public override string VisitNumber([NotNull] QueryParser.NumberContext context)
        {
          
            return context.GetText();
        }

       

    }
}