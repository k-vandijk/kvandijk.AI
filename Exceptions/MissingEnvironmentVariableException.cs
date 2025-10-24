namespace kvandijk.AI.Exceptions;

internal class MissingEnvironmentVariableException : Exception
{
    public MissingEnvironmentVariableException(string variableName) : base($"Please add the variable '{variableName}' to your environment.")
    {
    }
}