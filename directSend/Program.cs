// See https://aka.ms/new-console-template for more information
using System.Text;
using RabbitMQ.Client;

Console.WriteLine("Direct Send:Hello, World!");

// 1.实例化连接工厂
var factory = new ConnectionFactory() { HostName = "localhost" };
// 2.建立连接
using (var connection = factory.CreateConnection())
{
    // 3.创建信道
    using (var channel = connection.CreateModel())
    {
        // 4.申明队列
        //生成随机队列名称
        var queueName = channel.QueueDeclare().QueueName;
        // 使用direct exchange type,指定exchange名称
        channel.ExchangeDeclare(exchange: "directEC", type: "direct");
        // 将消息标记为持久化 - 将IBasicProperties.SetPersistent设置为true
        var properties = channel.CreateBasicProperties();
        // 设置prefetchCount : 1 来告知RabbitMQ，在未收到消费端的消息确认时，不再分发消息，也就确保了当消费端处于忙碌状态时
        channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);
        properties.Persistent = true;
        // 5. 构建byte消息数据包
        string message = args.Length > 0 ? args[0] : "Hello RabbitMQ!";
        var body = Encoding.UTF8.GetBytes(message);
        // 6. 发送数据包 必须指定routingKey
        channel.BasicPublish(exchange: "directEC", routingKey: "green", basicProperties: null, body: body);
        Console.WriteLine(" [x] Sent {0}", message);
    }
}
