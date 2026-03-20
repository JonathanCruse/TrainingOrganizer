namespace TrainingOrganizer.SharedKernel.Domain.Exceptions;

public class InvalidEntityStateException : DomainException
{
    public string EntityName { get; }
    public string CurrentState { get; }
    public string AttemptedOperation { get; }

    public InvalidEntityStateException(string entityName, string currentState, string attemptedOperation)
        : base($"Cannot {attemptedOperation} {entityName} in state '{currentState}'.")
    {
        EntityName = entityName;
        CurrentState = currentState;
        AttemptedOperation = attemptedOperation;
    }
}
