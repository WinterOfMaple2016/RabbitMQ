using RabbitMQ.Client;
using System.Text;
using RabbitMQ.Client.Events;
// See https://aka.ms/new-console-template for more information
// PRC Remote Procedure Call
Console.WriteLine("RPC Client:Hello, World!");
// 1.实例化连接工厂
var factory = new ConnectionFactory() { HostName = "localhost" };
// 2.建立连接
using (var connection = factory.CreateConnection())
{
    // 3.创建信道
    using (var channel = connection.CreateModel())
    {
        // 申明唯一GUID用来标识此次发送的远程调用请求
        var correlationId = Guid.NewGuid().ToString();
        // 申明监听的回调队列
        var replyQueue = channel.QueueDeclare().QueueName;
        var properties = channel.CreateBasicProperties();
        properties.ReplyTo = replyQueue; // 指定回调队列
        properties.CorrelationId = correlationId; // 指定消息的唯一标识
        string number = args.Length > 0 ? args[0] : "30";
        var body = Encoding.UTF8.GetBytes(number);
        // 发布消息
        channel.BasicPublish(exchange: "", routingKey: "rpc_queue", basicProperties: properties, body: body);
        Console.WriteLine($"[*] Request fib({number})");
        // 创建消费者用于处理消息的回调（远程调用返回的结果）
        var callbackConsumer = new EventingBasicConsumer(channel);
        channel.BasicConsume(queue: replyQueue, autoAck: true, consumer: callbackConsumer);
        callbackConsumer.Received += (model, ea) =>
        {
            // 仅当消息回调的ID与发送的ID一致时，说明远程调用结果正确返回
            if (ea.BasicProperties.CorrelationId == correlationId)
            {
                var body = ea.Body.ToArray();
                var responseMessage = $"get response:{Encoding.UTF8.GetString(body)}";
                Console.WriteLine($"[x]:{responseMessage}");
            }
        };
    }
}

