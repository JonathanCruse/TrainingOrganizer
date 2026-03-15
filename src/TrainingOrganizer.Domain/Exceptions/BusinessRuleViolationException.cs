namespace TrainingOrganizer.Domain.Exceptions;

public class BusinessRuleViolationException : DomainException
{
    public string Rule { get; }

    public BusinessRuleViolationException(string rule, string message)
        : base(message)
    {
        Rule = rule;
    }
}
