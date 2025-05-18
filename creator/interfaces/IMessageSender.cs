namespace creator.interfaces;

public interface IMessageBatchSender
{
    Task SendBatch(uint batchSize);
}