namespace creator.interfaces;

public interface IMessageSender
{
    Task SendBatch(uint batchSize);
}