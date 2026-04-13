namespace client.Services;

public enum ToastType
{
    Success,
    Error,
    Info
}

public class ToastMessage
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Message { get; set; } = string.Empty;
    public ToastType Type { get; set; }
}

public class ToastService
{
    public event Action<ToastMessage>? OnShow;

    public void Show(string message, ToastType type = ToastType.Success)
    {
        OnShow?.Invoke(new ToastMessage { Message = message, Type = type });
    }
}
