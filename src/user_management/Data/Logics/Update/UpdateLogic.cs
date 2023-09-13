namespace user_management.Data.Logics.Update;

using System;
using System.Collections.Generic;
using MongoDB.Driver;

public class UpdateLogic<TDocument>
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


    public UpdateDefinition<TDocument> BuildDefinition()
    {
        UpdateDefinitionBuilder<TDocument> updateBuilder = Builders<TDocument>.Update;
        UpdateDefinition<TDocument> update = null!;

        switch (Operation)
        {
            case INC:
                update = updateBuilder.Inc(Field, Value);
                break;
            case MAX:
                update = updateBuilder.Max(Field, Value);
                break;
            case MIN:
                update = updateBuilder.Min(Field, Value);
                break;
            case MUL:
                update = updateBuilder.Mul(Field, Value);
                break;
            case POPFIRST:
                update = updateBuilder.PopFirst(Field);
                break;
            case POPLAST:
                update = updateBuilder.PopLast(Field);
                break;
            case PULL:
                update = updateBuilder.Pull(Field, Value);
                break;
            case PULLALL:
                update = updateBuilder.PullAll(Field, Value);
                break;
            case PULLFILTER:
                update = updateBuilder.PullFilter(Field, Value);
                break;
            case PUSH:
                update = updateBuilder.Push(Field, Value);
                break;
            case PUSHEACH:
                update = updateBuilder.PushEach(Field, Value);
                break;
            case RENAME:
                update = updateBuilder.Rename(Field, Value);
                break;
            case SET:
                update = updateBuilder.Set(Field, Value);
                break;
            case UNSET:
                update = updateBuilder.Unset(Field);
                break;
            default:
                throw new ArgumentException("Invalid Operation value");
        }

        return update;
    }

    public const string INC = "Inc";
    public const string MAX = "Max";
    public const string MIN = "Min";
    public const string MUL = "Mul";
    public const string POPFIRST = "Popfirst";
    public const string POPLAST = "Poplast";
    public const string PULL = "Pull";
    public const string PULLALL = "Pullall";
    public const string PULLFILTER = "Pullfilter";
    public const string PUSH = "Push";
    public const string PUSHEACH = "Pusheach";
    public const string RENAME = "Rename";
    public const string SET = "Set";
    public const string UNSET = "Unset";
    public readonly static List<string> OPERATIONS = new List<string> { INC, MAX, MIN, MUL, POPFIRST, POPLAST, PULL, PULLALL, PULLFILTER, PUSH, PUSHEACH, RENAME, SET, UNSET };
}
