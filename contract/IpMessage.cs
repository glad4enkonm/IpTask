namespace contract;

public class IpMessage
{
    public ulong UserId {get; set;} // считаем что у нас не может быть отрицательных id (по условию задания long)
    public required string Ip {get; set;}
}
