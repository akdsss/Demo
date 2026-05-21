public class SkillValidationResult
{
    public bool IsValid { get; }
    public SkillFailReason FailReason { get; }
    public string FailTextKey { get; }
    public string Message { get; }

    private SkillValidationResult(bool isValid, SkillFailReason failReason, string failTextKey, string message)
    {
        IsValid = isValid;
        FailReason = failReason;
        FailTextKey = failTextKey;
        Message = message;
    }

    public static SkillValidationResult Success()
    {
        return new SkillValidationResult(true, SkillFailReason.None, string.Empty, string.Empty);
    }

    public static SkillValidationResult Fail(SkillFailReason reason, string failTextKey = "", string message = "")
    {
        return new SkillValidationResult(false, reason, failTextKey, message);
    }
}
