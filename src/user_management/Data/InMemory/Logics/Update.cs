using MongoDB.Driver.Linq;
using user_management.Data.Logics;
using user_management.Utilities;

namespace user_management.Data.InMemory.Logics;

public class Update<T> : Update
{
    public static Func<object, object> BuildDefinition(IEnumerable<Update> updates) => (object record) =>
                                                                                                        {
                                                                                                            foreach (Update update in updates)
                                                                                                                record = Build(update)(record);
                                                                                                            return record;
                                                                                                        };

    public static Func<object, object> Build(Update update)
    {
        if (!AllOperations.Contains(update.Operation))
            throw new ArgumentException("Invalid Operation value");
        return update.Operation switch
        {
            INC => (object record) =>
            {
                switch (update.Type)
                {
                    case Types.INT:
                        record.GetType().GetProperty(update.Field.ToPascalCase())!.SetValue(record, int.Parse(record.GetType().GetProperty(update.Field.ToPascalCase())!.GetValue(record)!.ToString()!) + update.Value);
                        break;
                    case Types.LONG:
                        record.GetType().GetProperty(update.Field.ToPascalCase())!.SetValue(record, long.Parse(record.GetType().GetProperty(update.Field.ToPascalCase())!.GetValue(record)!.ToString()!) + update.Value);
                        break;
                    case Types.FLOAT:
                        record.GetType().GetProperty(update.Field.ToPascalCase())!.SetValue(record, float.Parse(record.GetType().GetProperty(update.Field.ToPascalCase())!.GetValue(record)!.ToString()!) + update.Value);
                        break;
                    case Types.DOUBLE:
                        record.GetType().GetProperty(update.Field.ToPascalCase())!.SetValue(record, double.Parse(record.GetType().GetProperty(update.Field.ToPascalCase())!.GetValue(record)!.ToString()!) + update.Value);
                        break;
                    case Types.DECIMAL:
                        record.GetType().GetProperty(update.Field.ToPascalCase())!.SetValue(record, decimal.Parse(record.GetType().GetProperty(update.Field.ToPascalCase())!.GetValue(record)!.ToString()!) + update.Value);
                        break;
                    default:
                        throw new ArgumentException($"Unsupported type for {update.Operation} operation");
                }
                return record;
            }
            ,
            MAX => (object record) =>
            {
                switch (update.Type)
                {
                    case Types.INT:
                        var recordIntValue = int.Parse(record.GetType().GetProperty(update.Field.ToPascalCase())!.GetValue(record)!.ToString()!);
                        if ((int)update.Value > recordIntValue)
                            record.GetType().GetProperty(update.Field.ToPascalCase())!.SetValue(record, update.Value);
                        break;
                    case Types.LONG:
                        var recordLongValue = long.Parse(record.GetType().GetProperty(update.Field.ToPascalCase())!.GetValue(record)!.ToString()!);
                        if ((long)update.Value > recordLongValue)
                            record.GetType().GetProperty(update.Field.ToPascalCase())!.SetValue(record, update.Value);
                        break;
                    case Types.FLOAT:
                        var recordFloatValue = float.Parse(record.GetType().GetProperty(update.Field.ToPascalCase())!.GetValue(record)!.ToString()!);
                        if ((float)update.Value > recordFloatValue)
                            record.GetType().GetProperty(update.Field.ToPascalCase())!.SetValue(record, update.Value);
                        break;
                    case Types.DOUBLE:
                        var recordDoubleValue = double.Parse(record.GetType().GetProperty(update.Field.ToPascalCase())!.GetValue(record)!.ToString()!);
                        if ((double)update.Value > recordDoubleValue)
                            record.GetType().GetProperty(update.Field.ToPascalCase())!.SetValue(record, update.Value);
                        break;
                    case Types.DECIMAL:
                        var recordDecimalValue = decimal.Parse(record.GetType().GetProperty(update.Field.ToPascalCase())!.GetValue(record)!.ToString()!);
                        if ((decimal)update.Value > recordDecimalValue)
                            record.GetType().GetProperty(update.Field.ToPascalCase())!.SetValue(record, update.Value);
                        break;
                    default:
                        throw new ArgumentException($"Unsupported type for {update.Operation} operation");
                }
                return record;
            }
            ,
            MIN => (object record) =>
            {
                switch (update.Type)
                {
                    case Types.INT:
                        var recordIntValue = int.Parse(record.GetType().GetProperty(update.Field.ToPascalCase())!.GetValue(record)!.ToString()!);
                        if ((int)update.Value < recordIntValue)
                            record.GetType().GetProperty(update.Field.ToPascalCase())!.SetValue(record, update.Value);
                        break;
                    case Types.LONG:
                        var recordLongValue = long.Parse(record.GetType().GetProperty(update.Field.ToPascalCase())!.GetValue(record)!.ToString()!);
                        if ((long)update.Value < recordLongValue)
                            record.GetType().GetProperty(update.Field.ToPascalCase())!.SetValue(record, update.Value);
                        break;
                    case Types.FLOAT:
                        var recordFloatValue = float.Parse(record.GetType().GetProperty(update.Field.ToPascalCase())!.GetValue(record)!.ToString()!);
                        if ((float)update.Value < recordFloatValue)
                            record.GetType().GetProperty(update.Field.ToPascalCase())!.SetValue(record, update.Value);
                        break;
                    case Types.DOUBLE:
                        var recordDoubleValue = double.Parse(record.GetType().GetProperty(update.Field.ToPascalCase())!.GetValue(record)!.ToString()!);
                        if ((double)update.Value < recordDoubleValue)
                            record.GetType().GetProperty(update.Field.ToPascalCase())!.SetValue(record, update.Value);
                        break;
                    case Types.DECIMAL:
                        var recordDecimalValue = decimal.Parse(record.GetType().GetProperty(update.Field.ToPascalCase())!.GetValue(record)!.ToString()!);
                        if ((decimal)update.Value < recordDecimalValue)
                            record.GetType().GetProperty(update.Field.ToPascalCase())!.SetValue(record, update.Value);
                        break;
                    default:
                        throw new ArgumentException($"Unsupported type for {update.Operation} operation");
                }
                return record;
            }
            ,
            MUL => (object record) =>
            {
                switch (update.Type)
                {
                    case Types.INT:
                        var recordIntValue = int.Parse(record.GetType().GetProperty(update.Field.ToPascalCase())!.GetValue(record)!.ToString()!);
                        record.GetType().GetProperty(update.Field.ToPascalCase())!.SetValue(record, recordIntValue * update.Value);
                        break;
                    case Types.LONG:
                        var recordLongValue = long.Parse(record.GetType().GetProperty(update.Field.ToPascalCase())!.GetValue(record)!.ToString()!);
                        record.GetType().GetProperty(update.Field.ToPascalCase())!.SetValue(record, recordLongValue * update.Value);
                        break;
                    case Types.FLOAT:
                        var recordFloatValue = float.Parse(record.GetType().GetProperty(update.Field.ToPascalCase())!.GetValue(record)!.ToString()!);
                        record.GetType().GetProperty(update.Field.ToPascalCase())!.SetValue(record, recordFloatValue * update.Value);
                        break;
                    case Types.DOUBLE:
                        var recordDoubleValue = double.Parse(record.GetType().GetProperty(update.Field.ToPascalCase())!.GetValue(record)!.ToString()!);
                        record.GetType().GetProperty(update.Field.ToPascalCase())!.SetValue(record, recordDoubleValue * update.Value);
                        break;
                    case Types.DECIMAL:
                        var recordDecimalValue = decimal.Parse(record.GetType().GetProperty(update.Field.ToPascalCase())!.GetValue(record)!.ToString()!);
                        record.GetType().GetProperty(update.Field.ToPascalCase())!.SetValue(record, recordDecimalValue * update.Value);
                        break;
                    default:
                        throw new ArgumentException($"Unsupported type for {update.Operation} operation");
                }
                return record;
            }
            ,
            POPFIRST => (object record) =>
            {
                switch (update.Type)
                {
                    case Types.INT:
                        var recordIntValue = (record.GetType().GetProperty(update.Field.ToPascalCase())!.GetValue(record)! as IEnumerable<int>)!;
                        record.GetType().GetProperty(update.Field.ToPascalCase())!.SetValue(record, recordIntValue.Where((o, i) => i != 0));
                        break;
                    case Types.LONG:
                        var recordLongValue = (record.GetType().GetProperty(update.Field.ToPascalCase())!.GetValue(record)! as IEnumerable<long>)!;
                        record.GetType().GetProperty(update.Field.ToPascalCase())!.SetValue(record, recordLongValue.Where((o, i) => i != 0));
                        break;
                    case Types.FLOAT:
                        var recordFloatValue = (record.GetType().GetProperty(update.Field.ToPascalCase())!.GetValue(record)! as IEnumerable<float>)!;
                        record.GetType().GetProperty(update.Field.ToPascalCase())!.SetValue(record, recordFloatValue.Where((o, i) => i != 0));
                        break;
                    case Types.DOUBLE:
                        var recordDoubleValue = (record.GetType().GetProperty(update.Field.ToPascalCase())!.GetValue(record)! as IEnumerable<double>)!;
                        record.GetType().GetProperty(update.Field.ToPascalCase())!.SetValue(record, recordDoubleValue.Where((o, i) => i != 0));
                        break;
                    case Types.DECIMAL:
                        var recordDecimalValue = (record.GetType().GetProperty(update.Field.ToPascalCase())!.GetValue(record)! as IEnumerable<decimal>)!;
                        record.GetType().GetProperty(update.Field.ToPascalCase())!.SetValue(record, recordDecimalValue.Where((o, i) => i != 0));
                        break;
                    case Types.STRING:
                        var recordStringValue = (record.GetType().GetProperty(update.Field.ToPascalCase())!.GetValue(record)! as IEnumerable<string>)!;
                        record.GetType().GetProperty(update.Field.ToPascalCase())!.SetValue(record, recordStringValue.Where((o, i) => i != 0));
                        break;
                    case Types.DATETIME:
                        var recordDateTimeValue = (record.GetType().GetProperty(update.Field.ToPascalCase())!.GetValue(record)! as IEnumerable<DateTime>)!;
                        record.GetType().GetProperty(update.Field.ToPascalCase())!.SetValue(record, recordDateTimeValue.Where((o, i) => i != 0));
                        break;
                    case Types.BOOL:
                        var recordBoolValue = (record.GetType().GetProperty(update.Field.ToPascalCase())!.GetValue(record)! as IEnumerable<bool>)!;
                        record.GetType().GetProperty(update.Field.ToPascalCase())!.SetValue(record, recordBoolValue.Where((o, i) => i != 0));
                        break;
                    default:
                        throw new ArgumentException($"Unsupported type for {update.Operation} operation");
                }
                return record;
            }
            ,
            POPLAST => (object record) =>
            {
                switch (update.Type)
                {
                    case Types.INT:
                        var recordIntValue = (record.GetType().GetProperty(update.Field.ToPascalCase())!.GetValue(record)! as IEnumerable<int>)!;
                        record.GetType().GetProperty(update.Field.ToPascalCase())!.SetValue(record, recordIntValue.Where((o, i) => i != recordIntValue.Count() - 1));
                        break;
                    case Types.LONG:
                        var recordLongValue = (record.GetType().GetProperty(update.Field.ToPascalCase())!.GetValue(record)! as IEnumerable<long>)!;
                        record.GetType().GetProperty(update.Field.ToPascalCase())!.SetValue(record, recordLongValue.Where((o, i) => i != recordLongValue.Count() - 1));
                        break;
                    case Types.FLOAT:
                        var recordFloatValue = (record.GetType().GetProperty(update.Field.ToPascalCase())!.GetValue(record)! as IEnumerable<float>)!;
                        record.GetType().GetProperty(update.Field.ToPascalCase())!.SetValue(record, recordFloatValue.Where((o, i) => i != recordFloatValue.Count() - 1));
                        break;
                    case Types.DOUBLE:
                        var recordDoubleValue = (record.GetType().GetProperty(update.Field.ToPascalCase())!.GetValue(record)! as IEnumerable<double>)!;
                        record.GetType().GetProperty(update.Field.ToPascalCase())!.SetValue(record, recordDoubleValue.Where((o, i) => i != recordDoubleValue.Count() - 1));
                        break;
                    case Types.DECIMAL:
                        var recordDecimalValue = (record.GetType().GetProperty(update.Field.ToPascalCase())!.GetValue(record)! as IEnumerable<decimal>)!;
                        record.GetType().GetProperty(update.Field.ToPascalCase())!.SetValue(record, recordDecimalValue.Where((o, i) => i != recordDecimalValue.Count() - 1));
                        break;
                    case Types.STRING:
                        var recordStringValue = (record.GetType().GetProperty(update.Field.ToPascalCase())!.GetValue(record)! as IEnumerable<string>)!;
                        record.GetType().GetProperty(update.Field.ToPascalCase())!.SetValue(record, recordStringValue.Where((o, i) => i != recordStringValue.Count() - 1));
                        break;
                    case Types.DATETIME:
                        var recordDateTimeValue = (record.GetType().GetProperty(update.Field.ToPascalCase())!.GetValue(record)! as IEnumerable<DateTime>)!;
                        record.GetType().GetProperty(update.Field.ToPascalCase())!.SetValue(record, recordDateTimeValue.Where((o, i) => i != recordDateTimeValue.Count() - 1));
                        break;
                    case Types.BOOL:
                        var recordBoolValue = (record.GetType().GetProperty(update.Field.ToPascalCase())!.GetValue(record)! as IEnumerable<bool>)!;
                        record.GetType().GetProperty(update.Field.ToPascalCase())!.SetValue(record, recordBoolValue.Where((o, i) => i != recordBoolValue.Count() - 1));
                        break;
                    default:
                        throw new ArgumentException($"Unsupported type for {update.Operation} operation");
                }
                return record;
            }
            ,
            PULL => (object record) =>
            {
                switch (update.Type)
                {
                    case Types.INT:
                        var recordIntValue = (record.GetType().GetProperty(update.Field.ToPascalCase())!.GetValue(record)! as IEnumerable<int>)!;
                        record.GetType().GetProperty(update.Field.ToPascalCase())!.SetValue(record, recordIntValue.Where(o => o != (int)update.Value));
                        break;
                    case Types.LONG:
                        var recordLongValue = (record.GetType().GetProperty(update.Field.ToPascalCase())!.GetValue(record)! as IEnumerable<long>)!;
                        record.GetType().GetProperty(update.Field.ToPascalCase())!.SetValue(record, recordLongValue.Where(o => o != (long)update.Value));
                        break;
                    case Types.FLOAT:
                        var recordFloatValue = (record.GetType().GetProperty(update.Field.ToPascalCase())!.GetValue(record)! as IEnumerable<float>)!;
                        record.GetType().GetProperty(update.Field.ToPascalCase())!.SetValue(record, recordFloatValue.Where(o => o != (float)update.Value));
                        break;
                    case Types.DOUBLE:
                        var recordDoubleValue = (record.GetType().GetProperty(update.Field.ToPascalCase())!.GetValue(record)! as IEnumerable<double>)!;
                        record.GetType().GetProperty(update.Field.ToPascalCase())!.SetValue(record, recordDoubleValue.Where(o => o != (double)update.Value));
                        break;
                    case Types.DECIMAL:
                        var recordDecimalValue = (record.GetType().GetProperty(update.Field.ToPascalCase())!.GetValue(record)! as IEnumerable<decimal>)!;
                        record.GetType().GetProperty(update.Field.ToPascalCase())!.SetValue(record, recordDecimalValue.Where(o => o != (decimal)update.Value));
                        break;
                    case Types.STRING:
                        var recordStringValue = (record.GetType().GetProperty(update.Field.ToPascalCase())!.GetValue(record)! as IEnumerable<string>)!;
                        record.GetType().GetProperty(update.Field.ToPascalCase())!.SetValue(record, recordStringValue.Where(o => o != (string)update.Value!));
                        break;
                    case Types.DATETIME:
                        var recordDateTimeValue = (record.GetType().GetProperty(update.Field.ToPascalCase())!.GetValue(record)! as IEnumerable<DateTime>)!;
                        record.GetType().GetProperty(update.Field.ToPascalCase())!.SetValue(record, recordDateTimeValue.Where(o => o != (DateTime)update.Value));
                        break;
                    case Types.BOOL:
                        var recordBoolValue = (record.GetType().GetProperty(update.Field.ToPascalCase())!.GetValue(record)! as IEnumerable<bool>)!;
                        record.GetType().GetProperty(update.Field.ToPascalCase())!.SetValue(record, recordBoolValue.Where(o => o != (bool)update.Value));
                        break;
                    default:
                        throw new ArgumentException($"Unsupported type for {update.Operation} operation");
                }
                return record;
            }
            ,
            PULLALL => (object record) =>
            {
                switch (update.Type)
                {
                    case Types.INT_ARRAY:
                        var recordIntValue = (record.GetType().GetProperty(update.Field.ToPascalCase())!.GetValue(record)! as IEnumerable<int>)!;
                        record.GetType().GetProperty(update.Field.ToPascalCase())!.SetValue(record, recordIntValue.Where(o => !(update.Value! as IEnumerable<int>)!.Contains(o)));
                        break;
                    case Types.LONG_ARRAY:
                        var recordLongValue = (record.GetType().GetProperty(update.Field.ToPascalCase())!.GetValue(record)! as IEnumerable<long>)!;
                        record.GetType().GetProperty(update.Field.ToPascalCase())!.SetValue(record, recordLongValue.Where(o => !(update.Value! as IEnumerable<long>)!.Contains(o)));
                        break;
                    case Types.FLOAT_ARRAY:
                        var recordFloatValue = (record.GetType().GetProperty(update.Field.ToPascalCase())!.GetValue(record)! as IEnumerable<float>)!;
                        record.GetType().GetProperty(update.Field.ToPascalCase())!.SetValue(record, recordFloatValue.Where(o => !(update.Value! as IEnumerable<float>)!.Contains(o)));
                        break;
                    case Types.DOUBLE_ARRAY:
                        var recordDoubleValue = (record.GetType().GetProperty(update.Field.ToPascalCase())!.GetValue(record)! as IEnumerable<double>)!;
                        record.GetType().GetProperty(update.Field.ToPascalCase())!.SetValue(record, recordDoubleValue.Where(o => !(update.Value! as IEnumerable<double>)!.Contains(o)));
                        break;
                    case Types.DECIMAL_ARRAY:
                        var recordDecimalValue = (record.GetType().GetProperty(update.Field.ToPascalCase())!.GetValue(record)! as IEnumerable<decimal>)!;
                        record.GetType().GetProperty(update.Field.ToPascalCase())!.SetValue(record, recordDecimalValue.Where(o => !(update.Value! as IEnumerable<decimal>)!.Contains(o)));
                        break;
                    case Types.STRING_ARRAY:
                        var recordStringValue = (record.GetType().GetProperty(update.Field.ToPascalCase())!.GetValue(record)! as IEnumerable<string>)!;
                        record.GetType().GetProperty(update.Field.ToPascalCase())!.SetValue(record, recordStringValue.Where(o => !(update.Value! as IEnumerable<string>)!.Contains(o)));
                        break;
                    case Types.DATETIME_ARRAY:
                        var recordDateTimeValue = (record.GetType().GetProperty(update.Field.ToPascalCase())!.GetValue(record)! as IEnumerable<DateTime>)!;
                        record.GetType().GetProperty(update.Field.ToPascalCase())!.SetValue(record, recordDateTimeValue.Where(o => !(update.Value! as IEnumerable<DateTime>)!.Contains(o)));
                        break;
                    case Types.BOOL_ARRAY:
                        var recordBoolValue = (record.GetType().GetProperty(update.Field.ToPascalCase())!.GetValue(record)! as IEnumerable<bool>)!;
                        record.GetType().GetProperty(update.Field.ToPascalCase())!.SetValue(record, recordBoolValue.Where(o => !(update.Value! as IEnumerable<bool>)!.Contains(o)));
                        break;
                    default:
                        throw new ArgumentException($"Unsupported type for {update.Operation} operation");
                }
                return record;
            }
            ,
            PUSH => (object record) =>
            {
                switch (update.Type)
                {
                    case Types.INT:
                        var recordIntValue = (record.GetType().GetProperty(update.Field.ToPascalCase())!.GetValue(record)! as IEnumerable<int>)!;
                        record.GetType().GetProperty(update.Field.ToPascalCase())!.SetValue(record, recordIntValue.Append((int)update.Value));
                        break;
                    case Types.LONG:
                        var recordLongValue = (record.GetType().GetProperty(update.Field.ToPascalCase())!.GetValue(record)! as IEnumerable<long>)!;
                        record.GetType().GetProperty(update.Field.ToPascalCase())!.SetValue(record, recordLongValue.Append((long)update.Value));
                        break;
                    case Types.FLOAT:
                        var recordFloatValue = (record.GetType().GetProperty(update.Field.ToPascalCase())!.GetValue(record)! as IEnumerable<float>)!;
                        record.GetType().GetProperty(update.Field.ToPascalCase())!.SetValue(record, recordFloatValue.Append((float)update.Value));
                        break;
                    case Types.DOUBLE:
                        var recordDoubleValue = (record.GetType().GetProperty(update.Field.ToPascalCase())!.GetValue(record)! as IEnumerable<double>)!;
                        record.GetType().GetProperty(update.Field.ToPascalCase())!.SetValue(record, recordDoubleValue.Append((double)update.Value));
                        break;
                    case Types.DECIMAL:
                        var recordDecimalValue = (record.GetType().GetProperty(update.Field.ToPascalCase())!.GetValue(record)! as IEnumerable<decimal>)!;
                        record.GetType().GetProperty(update.Field.ToPascalCase())!.SetValue(record, recordDecimalValue.Append((decimal)update.Value));
                        break;
                    case Types.STRING:
                        var recordStringValue = (record.GetType().GetProperty(update.Field.ToPascalCase())!.GetValue(record)! as IEnumerable<string>)!;
                        record.GetType().GetProperty(update.Field.ToPascalCase())!.SetValue(record, recordStringValue.Append((string)update.Value!));
                        break;
                    case Types.DATETIME:
                        var recordDateTimeValue = (record.GetType().GetProperty(update.Field.ToPascalCase())!.GetValue(record)! as IEnumerable<DateTime>)!;
                        record.GetType().GetProperty(update.Field.ToPascalCase())!.SetValue(record, recordDateTimeValue.Append((DateTime)update.Value));
                        break;
                    case Types.BOOL:
                        var recordBoolValue = (record.GetType().GetProperty(update.Field.ToPascalCase())!.GetValue(record)! as IEnumerable<bool>)!;
                        record.GetType().GetProperty(update.Field.ToPascalCase())!.SetValue(record, recordBoolValue.Append((bool)update.Value));
                        break;
                    default:
                        throw new ArgumentException($"Unsupported type for {update.Operation} operation");
                }
                return record;
            }
            ,
            PUSHEACH => (object record) =>
            {
                switch (update.Type)
                {
                    case Types.INT_ARRAY:
                        var recordIntValue = (record.GetType().GetProperty(update.Field.ToPascalCase())!.GetValue(record)! as IEnumerable<int>)!;
                        foreach (var v in (update.Value! as IEnumerable<int>)!)
                            recordIntValue = recordIntValue.Append(v);
                        record.GetType().GetProperty(update.Field.ToPascalCase())!.SetValue(record, recordIntValue);
                        break;
                    case Types.LONG_ARRAY:
                        var recordLongValue = (record.GetType().GetProperty(update.Field.ToPascalCase())!.GetValue(record)! as IEnumerable<long>)!;
                        foreach (var v in (update.Value! as IEnumerable<long>)!)
                            recordLongValue = recordLongValue.Append(v);
                        record.GetType().GetProperty(update.Field.ToPascalCase())!.SetValue(record, recordLongValue);
                        break;
                    case Types.FLOAT_ARRAY:
                        var recordFloatValue = (record.GetType().GetProperty(update.Field.ToPascalCase())!.GetValue(record)! as IEnumerable<float>)!;
                        foreach (var v in (update.Value! as IEnumerable<float>)!)
                            recordFloatValue = recordFloatValue.Append(v);
                        record.GetType().GetProperty(update.Field.ToPascalCase())!.SetValue(record, recordFloatValue);
                        break;
                    case Types.DOUBLE_ARRAY:
                        var recordDoubleValue = (record.GetType().GetProperty(update.Field.ToPascalCase())!.GetValue(record)! as IEnumerable<double>)!;
                        foreach (var v in (update.Value! as IEnumerable<double>)!)
                            recordDoubleValue = recordDoubleValue.Append(v);
                        record.GetType().GetProperty(update.Field.ToPascalCase())!.SetValue(record, recordDoubleValue);
                        break;
                    case Types.DECIMAL_ARRAY:
                        var recordDecimalValue = (record.GetType().GetProperty(update.Field.ToPascalCase())!.GetValue(record)! as IEnumerable<decimal>)!;
                        foreach (var v in (update.Value! as IEnumerable<decimal>)!)
                            recordDecimalValue = recordDecimalValue.Append(v);
                        record.GetType().GetProperty(update.Field.ToPascalCase())!.SetValue(record, recordDecimalValue);
                        break;
                    case Types.STRING_ARRAY:
                        var recordStringValue = (record.GetType().GetProperty(update.Field.ToPascalCase())!.GetValue(record)! as IEnumerable<string>)!;
                        foreach (var v in (update.Value! as IEnumerable<string>)!)
                            recordStringValue = recordStringValue.Append(v);
                        record.GetType().GetProperty(update.Field.ToPascalCase())!.SetValue(record, recordStringValue);
                        break;
                    case Types.DATETIME_ARRAY:
                        var recordDateTimeValue = (record.GetType().GetProperty(update.Field.ToPascalCase())!.GetValue(record)! as IEnumerable<DateTime>)!;
                        foreach (var v in (update.Value! as IEnumerable<DateTime>)!)
                            recordDateTimeValue = recordDateTimeValue.Append(v);
                        record.GetType().GetProperty(update.Field.ToPascalCase())!.SetValue(record, recordDateTimeValue);
                        break;
                    case Types.BOOL_ARRAY:
                        var recordBoolValue = (record.GetType().GetProperty(update.Field.ToPascalCase())!.GetValue(record)! as IEnumerable<bool>)!;
                        foreach (var v in (update.Value! as IEnumerable<bool>)!)
                            recordBoolValue = recordBoolValue.Append(v);
                        record.GetType().GetProperty(update.Field.ToPascalCase())!.SetValue(record, recordBoolValue);
                        break;
                    default:
                        throw new ArgumentException($"Unsupported type for {update.Operation} operation");
                }
                return record;
            }
            ,
            SET => (object record) =>
            {
                switch (update.Type)
                {
                    case Types.NULL:
                        record.GetType().GetProperty(update.Field.ToPascalCase())!.SetValue(record, null);
                        break;
                    case Types.INT:
                        record.GetType().GetProperty(update.Field.ToPascalCase())!.SetValue(record, (int)update.Value!);
                        break;
                    case Types.LONG:
                        record.GetType().GetProperty(update.Field.ToPascalCase())!.SetValue(record, (long)update.Value!);
                        break;
                    case Types.FLOAT:
                        record.GetType().GetProperty(update.Field.ToPascalCase())!.SetValue(record, (float)update.Value!);
                        break;
                    case Types.DOUBLE:
                        record.GetType().GetProperty(update.Field.ToPascalCase())!.SetValue(record, (double)update.Value!);
                        break;
                    case Types.DECIMAL:
                        record.GetType().GetProperty(update.Field.ToPascalCase())!.SetValue(record, (decimal)update.Value!);
                        break;
                    case Types.STRING:
                        record.GetType().GetProperty(update.Field.ToPascalCase())!.SetValue(record, (string)update.Value!);
                        break;
                    case Types.DATETIME:
                        record.GetType().GetProperty(update.Field.ToPascalCase())!.SetValue(record, (DateTime)update.Value!);
                        break;
                    case Types.BOOL:
                        record.GetType().GetProperty(update.Field.ToPascalCase())!.SetValue(record, (bool)update.Value!);
                        break;
                    case Types.INT_ARRAY:
                        record.GetType().GetProperty(update.Field.ToPascalCase())!.SetValue(record, (IEnumerable<int>)update.Value!);
                        break;
                    case Types.LONG_ARRAY:
                        record.GetType().GetProperty(update.Field.ToPascalCase())!.SetValue(record, (IEnumerable<long>)update.Value!);
                        break;
                    case Types.FLOAT_ARRAY:
                        record.GetType().GetProperty(update.Field.ToPascalCase())!.SetValue(record, (IEnumerable<float>)update.Value!);
                        break;
                    case Types.DOUBLE_ARRAY:
                        record.GetType().GetProperty(update.Field.ToPascalCase())!.SetValue(record, (IEnumerable<double>)update.Value!);
                        break;
                    case Types.DECIMAL_ARRAY:
                        record.GetType().GetProperty(update.Field.ToPascalCase())!.SetValue(record, (IEnumerable<decimal>)update.Value!);
                        break;
                    case Types.STRING_ARRAY:
                        record.GetType().GetProperty(update.Field.ToPascalCase())!.SetValue(record, (IEnumerable<string>)update.Value!);
                        break;
                    case Types.DATETIME_ARRAY:
                        record.GetType().GetProperty(update.Field.ToPascalCase())!.SetValue(record, (IEnumerable<DateTime>)update.Value!);
                        break;
                    case Types.BOOL_ARRAY:
                        record.GetType().GetProperty(update.Field.ToPascalCase())!.SetValue(record, (IEnumerable<bool>)update.Value!);
                        break;
                    default:
                        throw new ArgumentException($"Unsupported type for {update.Operation} operation");
                };

                return record;
            }
            ,
            _ => throw new ArgumentException("Invalid Operation value"),
        };
    }

    private static bool IsNumber(object o)
    {
        if (
            o.GetType().FullName == typeof(int).FullName
            || o.GetType().FullName == typeof(float).FullName
            || o.GetType().FullName == typeof(long).FullName
            || o.GetType().FullName == typeof(double).FullName
            )
            return true;
        return false;
    }
}
