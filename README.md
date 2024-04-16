Using SchedulePublish with direct exchanges and routing by key causes delayed message to disappear.

Steps to reproduce (using my example project):
1. Run RabbitMQ with "rabbitmq-delayed-message-exchange" plugin installed, start Consumer
2. Start Publisher
3. Observe output, it publishes 3 messages, last message is published with a delay of 30 seconds
4. Wait for 30 seconds and check consumer output
5. Observe that only first two messages were consumed
6. In RabbitMQ management UI you will see "TextMessage_delay" exchange created and that it has 1 message delayed, but it doesn't publish it to the right queue