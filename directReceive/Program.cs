﻿// See https://aka.ms/new-console-template for more information
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

Console.WriteLine("Direct Receive: Hello, World!");
// 1. 实例化连接工厂
var factory = new ConnectionFactory() { HostName = "localhost" };
// 2.建立连接
using (var connection = factory.CreateConnection())
{
    // 3. 创建信道
    using (var channel = connection.CreateModel())
    {
        //4. 申明队列 类型为exchange
        // 申明随机队列名称
        var green = channel.QueueDeclare().QueueName;
        channel.ExchangeDeclare(exchange: "directEC", type: "direct");
        // 绑定队列到direct类型echange， 需指定路由键routingKey
        var routingKey = args.Length > 0 ? args[0] : "green";
        channel.QueueBind(queue: green, exchange: "directEC", routingKey: routingKey);
        // 5. 构造消费者实例
        var consumer = new EventingBasicConsumer(channel);
        // 6. 绑定消息接收后的事件委托
        consumer.Received += (model, ea) =>
        {
            var message = Encoding.UTF8.GetString(ea.Body.ToArray());
            Console.WriteLine(" [x] Received {0}", message);
            Thread.Sleep(6000); // 模拟耗时
            Console.WriteLine(" [x] Done");
            // 6.1. 发送消息确认信号（手动消息确认）
            channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
        };
        // 7. 启动消费
        // autoAck: true; 自动进行消息确认，当消费端接受到消息后，就自动发送ack信号，不管消息是否处理完毕
        // autoAck: false; 关闭自动消息确认，通过调用BasicAck方法手动进行消息确认
        channel.BasicConsume(queue: green, autoAck: false, consumer: consumer);
        Console.WriteLine(" Press [enter] to exit.");
        Console.ReadLine();
    }
}