namespace ArchiX.Library.Services.Parameters
{
    /// <summary>
    /// Parametre tanımı bulunamadığında fırlatılan exception.
    /// </summary>
    public sealed class ParameterNotFoundException : Exception
    {
        public string Group { get; }
        public string Key { get; }

        public ParameterNotFoundException(string group, string key)
            : base($"Parameter definition not found: Group='{group}', Key='{key}'")
        {
            Group = group;
            Key = key;
        }
    }

    /// <summary>
    /// Parametre değeri bulunamadığında fırlatılan exception.
    /// </summary>
    public sealed class ParameterValueNotFoundException : Exception
    {
        public string Group { get; }
        public string Key { get; }
        public int ApplicationId { get; }

        public ParameterValueNotFoundException(string group, string key, int applicationId)
            : base($"Parameter value not found: Group='{group}', Key='{key}', ApplicationId={applicationId} (fallback to ApplicationId=1 also failed)")
        {
            Group = group;
            Key = key;
            ApplicationId = applicationId;
        }
    }
}
