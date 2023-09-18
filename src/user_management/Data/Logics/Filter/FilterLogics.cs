namespace user_management.Data.Logics.Filter;

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Bson;
using MongoDB.Driver;
using user_management.Utilities;

public class FilterLogics<TDocument> : IFilterLogic<TDocument>
{
    public const string AND = "&&";
    public const string OR = "||";

    public IFilterLogic<TDocument> FirstLogic { get; set; } = null!;
    public IFilterLogic<TDocument>? SecondLogic { get; set; }

    private string _operator = null!;
    public string Operator
    {
        get { return _operator; }
        set
        {
            if (value != AND && value != OR)
                throw new ArgumentException("Operator property only accepts following values: " + String.Join(", ", new string[] { AND, OR }));

            _operator = value;
        }
    }

    public List<string> GetRequiredFields()
    {
        List<string> fields = FirstLogic.GetRequiredFields().Concat(SecondLogic == null ? new List<string>() { } : SecondLogic.GetRequiredFields()).ToList();
        if (Operator == AND)
            fields = fields.Concat(FirstLogic.GetOptionalFields().Concat(SecondLogic == null ? new List<string>() { } : SecondLogic.GetOptionalFields())).ToList();
        return fields;
    }

    public List<string> GetOptionalFields() => Operator == AND ? new List<string>() { } : FirstLogic.GetOptionalFields().Concat(SecondLogic == null ? new List<string>() { } : SecondLogic.GetOptionalFields()).ToList();

    public FilterDefinition<TDocument> BuildDefinition()
    {
        if (SecondLogic == null)
            return FirstLogic.BuildDefinition();

        FilterDefinition<TDocument>[] filters = new FilterDefinition<TDocument>[] { FirstLogic.BuildDefinition(), SecondLogic.BuildDefinition() };

        if (Operator == AND)
            return Builders<TDocument>.Filter.And(filters);

        if (Operator == OR)
            return Builders<TDocument>.Filter.Or(filters);

        throw new ArgumentException("Invalid operator property.");
    }

    // logicsString: Name::Eq::name::string||(Price::Gt::100::int&&CreatedAt::Gt::2023-04-29T09:07:07.250Z::datetime)
    // logicsString: Name::Eq::name::string||(Price::Gt::100::int&&Description::Eq::null::null)
    public static IFilterLogic<TDocument> BuildILogic(string logicsString)
    {
        if (logicsString.IsNullOrEmpty())
            throw new ArgumentNullException();
        if (logicsString.Count<char>(c => c.ToString() == "(") != logicsString.Count<char>(c => c.ToString() == ")"))
            throw new ArgumentException();

        if (!logicsString.Contains("("))
            return ExtractLogic(logicsString);

        int depth = 0;
        List<IFilterLogic<TDocument>> listOfLogics = new List<IFilterLogic<TDocument>>(capacity: 2);
        bool insideParentheses = false;
        string insideParenthesesLogicString = "";
        string outsideParenthesesLogicString = "";
        string theOperator = "";
        foreach (char c in logicsString)
        {
            if (c.ToString().IsNullOrEmpty())
                continue;

            if (c.ToString() == "(")
            {
                insideParentheses = true;
                depth++;
                if (depth == 1)
                    continue;
            }

            if (c.ToString() == ")")
            {
                depth--;
                if (depth == 0)
                {
                    insideParentheses = false;
                    listOfLogics.Add(BuildILogic(insideParenthesesLogicString));
                    insideParenthesesLogicString = "";
                    continue;
                }
            }

            if (!insideParentheses && (c.ToString() == "&" || c.ToString() == "|"))
            {
                if (theOperator.IsNullOrEmpty())
                {
                    theOperator = c.ToString() == "&" ? FilterLogics<TDocument>.AND : FilterLogics<TDocument>.OR;
                }
                continue;
            }

            if (insideParentheses)
                insideParenthesesLogicString += c.ToString();
            else
                outsideParenthesesLogicString += c.ToString();
        }

        if (theOperator.IsNullOrEmpty())
        {
            if (outsideParenthesesLogicString.IsNullOrEmpty() && listOfLogics.Count() == 0)
                throw new ArgumentException();

            if (!outsideParenthesesLogicString.IsNullOrEmpty())
                return ExtractLogic(outsideParenthesesLogicString);

            if (listOfLogics.Count() != 1)
                throw new ArgumentException();
            else
                return listOfLogics.First();
        }

        FilterLogics<TDocument> logics = new FilterLogics<TDocument>();
        logics.Operator = theOperator;

        if ((!outsideParenthesesLogicString.IsNullOrEmpty() && listOfLogics.Count() > 1) || (outsideParenthesesLogicString.IsNullOrEmpty() && (listOfLogics.Count() > 2 || listOfLogics.Count() == 0)))
            throw new ArgumentException();

        if (outsideParenthesesLogicString.IsNullOrEmpty())
        {
            logics.FirstLogic = listOfLogics[0];
            logics.SecondLogic = listOfLogics[1];
        }
        else
        {
            logics.FirstLogic = ExtractLogic(outsideParenthesesLogicString);
            logics.SecondLogic = listOfLogics[0];
        }

        return logics;
    }

    private static IFilterLogic<TDocument> ExtractLogic(string logicsString)
    {
        if (logicsString.IsNullOrEmpty())
            throw new ArgumentNullException();
        if (logicsString.Contains("(") || logicsString.Contains(")"))
            throw new ArgumentException();

        if (logicsString.Contains(FilterLogics<TDocument>.AND) || logicsString.Contains(FilterLogics<TDocument>.OR))
        {
            string theOperator = "";
            string[] logicStrings = null!;
            if (logicsString.Contains(FilterLogics<TDocument>.AND))
            {
                theOperator = FilterLogics<TDocument>.AND;
                logicStrings = logicsString.Split(separator: theOperator, count: 2, StringSplitOptions.RemoveEmptyEntries);
            }

            if (logicsString.Contains(FilterLogics<TDocument>.OR))
            {
                theOperator = FilterLogics<TDocument>.OR;
                logicStrings = logicsString.Split(separator: theOperator, count: 2, StringSplitOptions.RemoveEmptyEntries);
            }

            IFilterLogic<TDocument> firstLogic = ExtractLogic(logicStrings[0]);
            IFilterLogic<TDocument> secondLogic = ExtractLogic(logicStrings[1]);

            FilterLogics<TDocument> logics = new FilterLogics<TDocument>();
            logics.FirstLogic = firstLogic;
            logics.SecondLogic = secondLogic;
            logics.Operator = theOperator;

            return logics;
        }

        // Do not remove empty entries in splitted string, due to potential need for empty strings as the value
        string[] logicParts = logicsString.Split(separator: "::", count: 4);
        FilterLogic<TDocument> logic = new FilterLogic<TDocument>();
        switch (logicParts[3])
        {
            case "objectid":
                logic = new FilterLogic<TDocument>();
                logic.Value = ObjectId.Parse(logicParts[2]);
                break;
            case "null":
                logic = new FilterLogic<TDocument>();
                logic.Value = null;
                break;
            case "string":
                logic = new FilterLogic<TDocument>();
                logic.Value = logicParts[2];
                break;
            case "int":
                logic = new FilterLogic<TDocument>();
                logic.Value = Int32.Parse(logicParts[2]);
                break;
            case "bool":
                logic = new FilterLogic<TDocument>();
                logic.Value = Boolean.Parse(logicParts[2]);
                break;
            case "datetime":
                logic = new FilterLogic<TDocument>();
                logic.Value = DateTime.Parse(logicParts[2]);
                break;
            case "array":
                logic = new FilterLogic<TDocument>();
                logic.Value = logicParts[2].Split(",", StringSplitOptions.RemoveEmptyEntries);
                break;
            default:
                throw new ArgumentException("Invalid value type provided");
        }

        if (!logicParts[0].Contains("_"))
            logic.Field = logicParts[0].ToSnakeCase();
        else
            logic.Field = logicParts[0];

        logic.Operation = logicParts[1];

        return logic;
    }
}
