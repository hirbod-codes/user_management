using System.Text.Json;
using MongoDB.Driver;
using MongoDB.Driver.Core.Configuration;
// using user_management.Data.MongoDB.Logics;
using user_management.Data.InMemory.Logics;
using user_management.Data.Logics;
using MongoDB.Bson.Serialization.Attributes;

internal class Program
{
    private static void Main(string[] args)
    {
        Func<Document[]> getData = () => new Document[]{
            new() {
                Int = 1,
                Long = 1L,
                Double = 1d,
                Float = 1f,
                Decimal = 1m,
                String = "1",
                Bool = true,
                DateTime = new DateTime(year: 2020, month: 1, day: 1),
                IntArray = new int[] { 1,2 },
                LongArray = new long[] { 1,2 },
                DoubleArray = new double[] { 1,2 },
                FloatArray = new float[] { 1,2 },
                DecimalArray = new decimal[] { 1,2 },
                StringArray = new string[] { "1", "2" },
                BoolArray = new bool[] { true, false },
                DateTimeArray = new DateTime[] { new(year: 2020, month: 1, day: 1), new(year: 2020, month: 1, day: 2) }
            },
        };

        // MongoClient client = new(settings: new()
        // {
        //     Scheme = ConnectionStringScheme.MongoDB,
        //     Server = new MongoServerAddress("localhost", 30123),
        //     DirectConnection = true
        // });

        // var col = client.GetDatabase("my_db").GetCollection<Document>("a");
        Update<Document>[]? o;
        Func<object, object> func;
        string json;

        json = JsonSerializer.Serialize(new object[] {
            new
            {
                Operation = "Inc",
                type = Types.INT,
                field = "Int",
                Value = 2
            },
            new
            {
                Operation = "Max",
                type = Types.FLOAT,
                field = "Float",
                value = 10f
            },
            new
            {
                Operation = "Min",
                type = Types.LONG,
                field = "Long",
                value = -10
            },
            new
            {
                Operation = "Mul",
                type = "decimal",
                field = "Decimal",
                value = 10d
            }
        });

        o = JsonSerializer.Deserialize<Update<Document>[]>(json, new JsonSerializerOptions() { PropertyNameCaseInsensitive = true });
        func = Update<Document>.BuildDefinition(o!);
        Document[] mathData = getData();
        // col.DeleteManyAsync(Builders<Document>.Filter.Empty).Wait();
        // col.InsertMany(mathData);
        // col.UpdateMany(Builders<Document>.Filter.Empty, Update<Document>.BuildDefinition(o!));
        foreach (var d in mathData)
            func(d);

        json = JsonSerializer.Serialize(new object[] {
        new
        {
            Operation = "Popfirst",
            type = Types.INT,
            field = "IntArray"
        },
        new
        {
            Operation = "Popfirst",
            type = Types.LONG,
            field = "LongArray"
        },
        new
        {
            Operation = "Popfirst",
            type = Types.FLOAT,
            field = "FloatArray"
        },
        new
        {
            Operation = "Popfirst",
            type = Types.STRING,
            field = "StringArray"
        },
        new
        {
            Operation = "Popfirst",
            type = Types.DATETIME,
            field = "DateTimeArray"
        },
        new
        {
            Operation = "Popfirst",
            type = Types.BOOL,
            field = "BoolArray"
        }
    });

        o = JsonSerializer.Deserialize<Update<Document>[]>(json, new JsonSerializerOptions() { PropertyNameCaseInsensitive = true });
        func = Update<Document>.BuildDefinition(o!);
        Document[] popFirstData = getData();
        // col.DeleteManyAsync(Builders<Document>.Filter.Empty).Wait();
        // col.InsertMany(popFirstData);
        // col.UpdateMany(Builders<Document>.Filter.Empty, Update<Document>.BuildDefinition(o!));
        foreach (var d in popFirstData)
            func(d);

        json = JsonSerializer.Serialize(new object[] {
new
{
    Operation = "Poplast",
    type = Types.INT,
    field = "IntArray"
},
        new
        {
            Operation = "Poplast",
            type = Types.LONG,
            field = "LongArray"
        },
        new
        {
            Operation = "Poplast",
            type = Types.FLOAT,
            field = "FloatArray"
        },
        new
        {
            Operation = "Poplast",
            type = Types.STRING,
            field = "StringArray"
        },
        new
        {
            Operation = "Poplast",
            type = Types.DATETIME,
            field = "DateTimeArray"
        },
        new
        {
            Operation = "Poplast",
            type = Types.BOOL,
            field = "BoolArray"
        },
});

        o = JsonSerializer.Deserialize<Update<Document>[]>(json, new JsonSerializerOptions() { PropertyNameCaseInsensitive = true });
        func = Update<Document>.BuildDefinition(o!);
        Document[] popLastData = getData();
        // col.DeleteManyAsync(Builders<Document>.Filter.Empty).Wait();
        // col.InsertMany(popLastData);
        // col.UpdateMany(Builders<Document>.Filter.Empty, Update<Document>.BuildDefinition(o!));
        foreach (var d in popLastData)
            func(d);

        json = JsonSerializer.Serialize(new object[] {
        new
        {
            Operation = "Pull",
            type = Types.INT,
            field = "IntArray",
            value = 2
        },
        new
        {
            Operation = "Pull",
            type = Types.LONG,
            field = "LongArray",
            value = 2
        },
        new
        {
            Operation = "Pull",
            type = Types.FLOAT,
            field = "FloatArray",
            value = 2f
        },
        new
        {
            Operation = "Pull",
            type = Types.STRING,
            field = "StringArray",
            value = "2"
        },
        new
        {
            Operation = "Pull",
            type = Types.DATETIME,
            field = "DateTimeArray",
            value = new DateTime(year: 2020, month: 1, day: 2).ToString()
        },
        new
        {
            Operation = "Pull",
            type = Types.BOOL,
            field = "BoolArray",
            value = false
        },
});

        o = JsonSerializer.Deserialize<Update<Document>[]>(json, new JsonSerializerOptions() { PropertyNameCaseInsensitive = true });
        func = Update<Document>.BuildDefinition(o!);
        Document[] pullData = getData();
        // col.DeleteManyAsync(Builders<Document>.Filter.Empty).Wait();
        // col.InsertMany(pullData);
        // col.UpdateMany(Builders<Document>.Filter.Empty, Update<Document>.BuildDefinition(o!));
        foreach (var d in pullData)
            func(d);

        json = JsonSerializer.Serialize(new object[] {
        new
        {
            Operation = "Pullall",
            type = Types.INT_ARRAY,
            field = "IntArray",
            Value = "1,2"
        },
        new
        {
            Operation = "Pullall",
            type = Types.LONG_ARRAY,
            field = "LongArray",
            Value = "1,2"
        },
        new
        {
            Operation = "Pullall",
            type = Types.FLOAT_ARRAY,
            field = "FloatArray",
            Value = "1,2"
        },
        new
        {
            Operation = "Pullall",
            type = Types.STRING_ARRAY,
            field = "StringArray",
            Value = "1,2"
        },
        new
        {
            Operation = "Pullall",
            type = Types.DATETIME_ARRAY,
            field = "DateTimeArray",
            Value = "2020/1/1,2020/1/2"
        },
        new
        {
            Operation = "Pullall",
            type = Types.BOOL_ARRAY,
            field = "BoolArray",
            Value = "true, false"
        }
    });

        o = JsonSerializer.Deserialize<Update<Document>[]>(json, new JsonSerializerOptions() { PropertyNameCaseInsensitive = true });
        func = Update<Document>.BuildDefinition(o!);
        Document[] pullAllData = getData();
        // col.DeleteManyAsync(Builders<Document>.Filter.Empty).Wait();
        // col.InsertMany(pullAllData);
        // col.UpdateMany(Builders<Document>.Filter.Empty, Update<Document>.BuildDefinition(o!));
        foreach (var d in pullAllData)
            func(d);

        json = JsonSerializer.Serialize(new object[] {
        new
        {
            Operation = "Push",
            type = Types.INT,
            field = "IntArray",
            value = 3
        },
        new
        {
            Operation = "Push",
            type = Types.LONG,
            field = "LongArray",
            value = 3
        },
        new
        {
            Operation = "Push",
            type = Types.FLOAT,
            field = "FloatArray",
            value = 3f
        },
        new
        {
            Operation = "Push",
            type = Types.STRING,
            field = "StringArray",
            value = "3"
        },
        new
        {
            Operation = "Push",
            type = Types.DATETIME,
            field = "DateTimeArray",
            value = new DateTime(year: 2020, month: 1, day: 3).ToString()
        },
        new
        {
            Operation = "Push",
            type = Types.BOOL,
            field = "BoolArray",
            value = true
        },
});

        o = JsonSerializer.Deserialize<Update<Document>[]>(json, new JsonSerializerOptions() { PropertyNameCaseInsensitive = true });
        func = Update<Document>.BuildDefinition(o!);
        Document[] pushData = getData();
        // col.DeleteManyAsync(Builders<Document>.Filter.Empty).Wait();
        // col.InsertMany(pushData);
        // col.UpdateMany(Builders<Document>.Filter.Empty, Update<Document>.BuildDefinition(o!));
        foreach (var d in pushData)
            func(d);
        json = JsonSerializer.Serialize(new object[] {
        new
        {
            Operation = "Pusheach",
            type = Types.INT_ARRAY,
            field = "IntArray",
            Value = "1,2"
        },
        new
        {
            Operation = "Pusheach",
            type = Types.LONG_ARRAY,
            field = "LongArray",
            Value = "1,2"
        },
        new
        {
            Operation = "Pusheach",
            type = Types.FLOAT_ARRAY,
            field = "FloatArray",
            Value = "1,2"
        },
        new
        {
            Operation = "Pusheach",
            type = Types.STRING_ARRAY,
            field = "StringArray",
            Value = "1,2"
        },
        new
        {
            Operation = "Pusheach",
            type = Types.DATETIME_ARRAY,
            field = "DateTimeArray",
            Value = "2020/1/1,2020/1/2"
        },
        new
        {
            Operation = "Pusheach",
            type = Types.BOOL_ARRAY,
            field = "BoolArray",
            Value = "true, false"
        }
    });

        o = JsonSerializer.Deserialize<Update<Document>[]>(json, new JsonSerializerOptions() { PropertyNameCaseInsensitive = true });
        func = Update<Document>.BuildDefinition(o!);
        Document[] pushEachData = getData();
        // col.DeleteManyAsync(Builders<Document>.Filter.Empty).Wait();
        // col.InsertMany(pushEachData);
        // col.UpdateMany(Builders<Document>.Filter.Empty, Update<Document>.BuildDefinition(o!));
        foreach (var d in pushEachData)
            func(d);

        json = JsonSerializer.Serialize(new object[] {
        new
        {
            Operation = "Set",
            type = Types.INT,
            field = "Int",
            Value = 9
        },
        new
        {
            Operation = "Set",
            type = Types.LONG,
            field = "Long",
            Value = 8
        },
        new
        {
            Operation = "Set",
            type = Types.FLOAT,
            field = "Float",
            Value = 5f
        },
        new
        {
            Operation = "Set",
            type = Types.STRING,
            field = "String",
            Value = "1,2"
        },
        new
        {
            Operation = "Set",
            type = Types.DATETIME,
            field = "DateTime",
            Value = new DateTime(year: 2020, month: 1, day: 5).ToString()
        },
        new
        {
            Operation = "Set",
            type = Types.BOOL,
            field = "Bool",
            Value = true
        }
    });

        o = JsonSerializer.Deserialize<Update<Document>[]>(json, new JsonSerializerOptions() { PropertyNameCaseInsensitive = true });
        func = Update<Document>.BuildDefinition(o!);
        Document[] setData = getData();
        // col.DeleteManyAsync(Builders<Document>.Filter.Empty).Wait();
        // col.InsertMany(setData);
        // col.UpdateMany(Builders<Document>.Filter.Empty, Update<Document>.BuildDefinition(o!));
        foreach (var d in setData)
            func(d);

        return;
    }
}

public class Document
{
    public int Int { get; set; }
    public long Long { get; set; }
    public double Double { get; set; }
    public float Float { get; set; }
    [BsonRepresentation(MongoDB.Bson.BsonType.Decimal128)]
    public decimal Decimal { get; set; }
    public string String { get; set; } = null!;
    public bool Bool { get; set; }
    public DateTime DateTime { get; set; }
    public IEnumerable<int> IntArray { get; set; } = null!;
    public IEnumerable<long> LongArray { get; set; } = null!;
    public IEnumerable<double> DoubleArray { get; set; } = null!;
    public IEnumerable<float> FloatArray { get; set; } = null!;
    [BsonRepresentation(MongoDB.Bson.BsonType.Decimal128)]
    public IEnumerable<decimal> DecimalArray { get; set; } = null!;
    public IEnumerable<string> StringArray { get; set; } = null!;
    public IEnumerable<bool> BoolArray { get; set; } = null!;
    public IEnumerable<DateTime> DateTimeArray { get; set; } = null!;
}

