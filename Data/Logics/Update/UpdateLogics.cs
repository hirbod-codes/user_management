namespace user_management.Data.Logics.Update;

using System;
using System.Collections.Generic;
using MongoDB.Driver;
using user_management.Utilities;

public class UpdateLogics<TDocument>
{
    public const string UPDATE_SEPARATOR = "|||";
    public const string PARAMETER_SEPARATOR = "::";
    public const string ELEMENT_SEPARATOR = "_|_";
    public List<string> Fields { get; set; } = new List<string>();
    public List<UpdateDefinition<TDocument>> Logics { get; set; } = new List<UpdateDefinition<TDocument>>();

    public UpdateDefinition<TDocument> BuildDefinition()
    {
        return Builders<TDocument>.Update.Combine(Logics.ToArray());
    }

    // updatesString ==> field::Set::value::string||| ...
    public UpdateLogics<TDocument> BuildILogic(string updatesString)
    {
        UpdateLogics<TDocument> updateLogics = new UpdateLogics<TDocument>();

        string[] updateStrings = updatesString.Split(UPDATE_SEPARATOR);

        UpdateLogic<TDocument>? updateLogic;
        string[]? updateParameterStrings;
        foreach (string updateString in updateStrings)
        {
            // Do not remove empty entries in splitted string, due to potential need for empty strings as the value
            updateParameterStrings = updateString.Split(separator: PARAMETER_SEPARATOR, count: 4);
            Fields.Add(updateParameterStrings[0].Contains("_") ? updateParameterStrings[0] : updateParameterStrings[0].ToSnakeCase());

            updateLogic = new UpdateLogic<TDocument>()
            {
                Field = updateParameterStrings[0].Contains("_") ? updateParameterStrings[0] : updateParameterStrings[0].ToSnakeCase(),
                Operation = updateParameterStrings[1]
            };

            switch (updateParameterStrings[3])
            {
                case "null":
                    updateLogic.Value = null;
                    break;
                case "string":
                    updateLogic.Value = updateParameterStrings[2];
                    break;
                case "int":
                    updateLogic.Value = Int32.Parse(updateParameterStrings[2]);
                    break;
                case "bool":
                    updateLogic.Value = Boolean.Parse(updateParameterStrings[2]);
                    break;
                case "datetime":
                    updateLogic.Value = DateTime.Parse(updateParameterStrings[2]);
                    break;
                case "array":
                    updateLogic.Value = updateParameterStrings[2].Split(ELEMENT_SEPARATOR, StringSplitOptions.RemoveEmptyEntries);
                    break;
                case "string_array":
                    updateLogic.Value = updateParameterStrings[2].Split(ELEMENT_SEPARATOR, StringSplitOptions.RemoveEmptyEntries);
                    break;
                case "int_array":
                    updateLogic.Value = updateParameterStrings[2].Split(ELEMENT_SEPARATOR, StringSplitOptions.RemoveEmptyEntries).Cast<int>().ToArray();
                    break;
                case "object_array":
                    updateLogic.Value = updateParameterStrings[2].Split(ELEMENT_SEPARATOR, StringSplitOptions.RemoveEmptyEntries).Cast<object>().ToArray();
                    break;
                default:
                    throw new ArgumentException("Invalid value type provided");
            }

            updateLogics.Logics.Add(updateLogic.BuildDefinition());

            updateLogic = null;
            updateParameterStrings = null;
        }

        updateLogics.Fields = Fields;
        return updateLogics;
    }
}