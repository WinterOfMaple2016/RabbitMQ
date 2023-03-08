using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

internal class Program
{
    private static void Main(string[] args)
    {
        Console.WriteLine("RPC Server:Hello, World!");
        // 1.实例化连接工厂
        var factory = new ConnectionFactory() { HostName = "localhost" };
        // 2.建立连接
        using (var connection = factory.CreateConnection())
        {
            // 3.创建信道
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: "rpc_queue", durable: false, exclusive: false, autoDelete: false, arguments: null);
                var consumer = new EventingBasicConsumer(channel);
                Console.WriteLine("[*] Waiting for message.");
                //请求处理
                consumer.Received += (model, ea) =>
                {
                    var message = Encoding.UTF8.GetString(ea.Body.ToArray());
                    int n = int.Parse(message);
                    Console.WriteLine($"Receive request of Fib({n})");
                    int result = Fib(n);
                    Console.WriteLine($"result:{result}");
                    // 从请求的参数中获取唯一标识，在消息返回时带上
                    var properties = ea.BasicProperties;
                    var replyProperties = channel.CreateBasicProperties();
                    replyProperties.CorrelationId = properties.CorrelationId;
                    // 将远程调用的结果发送到客户端监听的队列上
                    channel.BasicPublish(exchange: "", routingKey: properties.ReplyTo, basicProperties: replyProperties, body: Encoding.UTF8.GetBytes(result.ToString()));
                    // 手动发回消息确认
                    channel.BasicAck(ea.DeliveryTag, false);
                    Console.WriteLine($"Return result: Fib(n)={result}");
                };
                channel.BasicConsume(queue: "rpc_queue", autoAck: false, consumer: consumer);
            }
        }
    }

    private static int Fib(int number)
    {
        if (number == 0 || number == 1)
        {
            return number;
        }
        return Fib(number - 1) + Fib(number - 2);
    }
}