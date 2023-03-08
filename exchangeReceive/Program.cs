﻿using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
// See https://aka.ms/new-console-template for more information
Console.WriteLine("exchangeReceive:Hello, World!");

// 1. 实例化连接工厂
var factory = new ConnectionFactory() { HostName = "localhost" };
// 2.建立连接
using (var connection = factory.CreateConnection())
{
    // 3. 创建信道
    using (var channel = connection.CreateModel())
    {
        //4. 申明队列
        channel.ExchangeDeclare(exchange: "fanoutEC", type: "fanout");
        // 申明随机队列名称
        var queuename = channel.QueueDeclare().QueueName;
        // 绑定队列fanout类型exchange， 无需指定路由键
        channel.QueueBind(queue: queuename, exchange: "fanoutEC", routingKey: "");
        // 5. 构造消费者实例
        var consumer = new EventingBasicConsumer(channel);
        // 6. 绑定消息接收后的事件委托do
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
        channel.BasicConsume(queue: queuename, autoAck: false, consumer: consumer);
        Console.WriteLine(" Press [enter] to exit.");
        Console.ReadLine();
    }
}