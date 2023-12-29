using MongoDB.Driver;
using user_management.Utilities;

namespace user_management.Data.MongoDB.Logics;

public class Update<T> : Data.Logics.Update
{
    public static UpdateDefinition<T> BuildDefinition(IEnumerable<Data.Logics.Update> updates) => Builders<T>.Update.Combine(updates.ToList().ConvertAll(Build));

    public static UpdateDefinition<T> Build(Data.Logics.Update update) => update.Operation switch
    {
        INC => Builders<T>.Update.Inc(update.Field!.ToSnakeCase(), double.Parse(update.Value!.ToString())),
        MAX => Builders<T>.Update.Max(update.Field!.ToSnakeCase(), double.Parse(update.Value!.ToString())),
        MIN => Builders<T>.Update.Min(update.Field!.ToSnakeCase(), double.Parse(update.Value!.ToString())),
        MUL => Builders<T>.Update.Mul(update.Field!.ToSnakeCase(), double.Parse(update.Value!.ToString())),
        POPFIRST => Builders<T>.Update.PopFirst(update.Field!.ToSnakeCase()),
        POPLAST => Builders<T>.Update.PopLast(update.Field!.ToSnakeCase()),
        PULL => Builders<T>.Update.Pull(update.Field!.ToSnakeCase(), update.Value),
        PULLALL => Builders<T>.Update.PullAll(update.Field!.ToSnakeCase(), update.Value),
        PUSH => Builders<T>.Update.Push(update.Field!.ToSnakeCase(), update.Value),
        PUSHEACH => Builders<T>.Update.PushEach(update.Field!.ToSnakeCase(), update.Value),
        SET => Builders<T>.Update.Set(update.Field!.ToSnakeCase(), update.Value),
        _ => throw new ArgumentException("Invalid Operation value"),
    };
}
