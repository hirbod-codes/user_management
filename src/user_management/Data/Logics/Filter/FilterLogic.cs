namespace user_management.Data.Logics.Filter;

using System;
using System.Collections.Generic;
using MongoDB.Driver;

public class FilterLogic<TDocument> : IFilterLogic<TDocument>
{
    private string _operation = null!;
    public string Operation
    {
        get { return _operation; }
        set
        {
            if (!OPERATIONS.Exists(v => v == value))
                throw new ArgumentException("Operation property only accepts following values: " + String.Join(", ", OPERATIONS));

            _operation = value;
        }
    }

    public string Field { get; set; } = null!;
    public dynamic? Value { get; set; } = null!;

    public List<string> GetRequiredFields() => new List<string>() { };
    public List<string> GetOptionalFields() => new List<string>() { Field };

    public FilterDefinition<TDocument> BuildDefinition()
    {
        FilterDefinitionBuilder<TDocument> filterBuilder = Builders<TDocument>.Filter;
        FilterDefinition<TDocument> filter = null!;
        switch (Operation)
        {
            case ALL:
                filter = filterBuilder.All(Field, Value);
                break;
            case IN:
                filter = filterBuilder.In<dynamic>(Field, Value);
                break;
            case REGEX:
                filter = filterBuilder.Regex(Field, Value);
                break;
            case EXISTS:
                filter = filterBuilder.Exists(Field, Value);
                break;
            case EQ:
                filter = filterBuilder.Eq<dynamic>(Field, Value);
                break;
            case NE:
                filter = filterBuilder.Ne<dynamic>(Field, Value);
                break;
            case GT:
                filter = filterBuilder.Gt<dynamic>(Field, Value);
                break;
            case LT:
                filter = filterBuilder.Lt<dynamic>(Field, Value);
                break;
            case GTE:
                filter = filterBuilder.Gte<dynamic>(Field, Value);
                break;
            case LTE:
                filter = filterBuilder.Lte<dynamic>(Field, Value);
                break;
            case ANYEQ:
                filter = filterBuilder.AnyEq<dynamic>(Field, Value);
                break;
            case ANYNE:
                filter = filterBuilder.AnyNe<dynamic>(Field, Value);
                break;
            case ANYGT:
                filter = filterBuilder.AnyGt<dynamic>(Field, Value);
                break;
            case ANYLT:
                filter = filterBuilder.AnyLt<dynamic>(Field, Value);
                break;
            case ANYGTE:
                filter = filterBuilder.AnyGte<dynamic>(Field, Value);
                break;
            case ANYLTE:
                filter = filterBuilder.AnyLte<dynamic>(Field, Value);
                break;
            case SIZEEQ:
                filter = filterBuilder.Size(Field, Value);
                break;
            case SIZEGT:
                filter = filterBuilder.SizeGt(Field, Value);
                break;
            case SIZELT:
                filter = filterBuilder.SizeLt(Field, Value);
                break;
            case SIZEGTE:
                filter = filterBuilder.SizeGte(Field, Value);
                break;
            case SIZELTE:
                filter = filterBuilder.SizeLte(Field, Value);
                break;
            default:
                throw new ArgumentException("Invalid Operation value");
        }

        return filter;
    }

    public const string ALL = "All";
    public const string IN = "In";
    public const string REGEX = "Regex";
    public const string EXISTS = "Exists";
    public const string EQ = "Eq";
    public const string NE = "Ne";
    public const string GT = "Gt";
    public const string LT = "Lt";
    public const string GTE = "Gte";
    public const string LTE = "Lte";
    public const string ANYEQ = "AnyEq";
    public const string ANYNE = "AnyNe";
    public const string ANYGT = "AnyGt";
    public const string ANYLT = "AnyLt";
    public const string ANYGTE = "AnyGte";
    public const string ANYLTE = "AnyLte";
    public const string SIZEEQ = "SizeEq";
    public const string SIZEGT = "SizeGt";
    public const string SIZELT = "SizeLt";
    public const string SIZEGTE = "SizeGte";
    public const string SIZELTE = "SizeLte";
    public readonly static List<string> OPERATIONS = new List<string> { ALL, IN, REGEX, EXISTS, EQ, NE, GT, LT, GTE, LTE, ANYEQ, ANYNE, ANYGT, ANYLT, ANYGTE, ANYLTE, SIZEEQ, SIZEGT, SIZELT, SIZEGTE, SIZELTE };
}
