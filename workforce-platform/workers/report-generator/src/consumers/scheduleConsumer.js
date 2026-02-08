import amqp from 'amqplib';

let connection = null;
let channel = null;

/**
 * Connect to RabbitMQ
 */
export async function connectRabbitMQ() {
  try {
    const rabbitmqHost = process.env.RABBITMQ_HOST || 'rabbitmq';
    const rabbitmqPort = process.env.RABBITMQ_PORT || 5672;
    const rabbitmqUser = process.env.RABBITMQ_USER || 'guest';
    const rabbitmqPassword = process.env.RABBITMQ_PASSWORD || 'guest';

    const connectionString = `amqp://${rabbitmqUser}:${rabbitmqPassword}@${rabbitmqHost}:${rabbitmqPort}`;

    connection = await amqp.connect(connectionString);
    channel = await connection.createChannel();

    global.logger.info('Connected to RabbitMQ successfully');
    return { connection, channel };
  } catch (error) {
    global.logger.error(`Failed to connect to RabbitMQ: ${error.message}`);
    throw error;
  }
}

/**
 * Start consuming messages from RabbitMQ
 */
export async function startConsuming() {
  if (!channel) {
    throw new Error('RabbitMQ channel not initialized. Call connectRabbitMQ() first.');
  }

  try {
    const exchangeName = process.env.RABBITMQ_EXCHANGE || 'workforce.events';
    const queueName = process.env.RABBITMQ_QUEUE || 'reports.queue';

    // Declare exchange
    await channel.assertExchange(exchangeName, 'topic', { durable: true });

    // Declare queue
    await channel.assertQueue(queueName, { durable: true });

    // Bind queue to exchange (listen to all events for now)
    await channel.bindQueue(queueName, exchangeName, '#');

    global.logger.info(`Queue ${queueName} bound to exchange ${exchangeName}`);

    // Consume messages
    await channel.consume(queueName, async (msg) => {
      if (msg) {
        try {
          const content = msg.content.toString();
          const routingKey = msg.fields.routingKey;

          global.logger.info(`Received message: ${routingKey}`);

          // Process message here if needed
          // For now, just acknowledge
          channel.ack(msg);
        } catch (error) {
          global.logger.error(`Error processing message: ${error.message}`);
          channel.nack(msg, false, true); // Requeue on error
        }
      }
    });

    global.logger.info('Started consuming messages from RabbitMQ');
  } catch (error) {
    global.logger.error(`Failed to start consuming: ${error.message}`);
    throw error;
  }
}

/**
 * Close RabbitMQ connection
 */
export async function closeConnection() {
  try {
    if (channel) {
      await channel.close();
      channel = null;
    }
    if (connection) {
      await connection.close();
      connection = null;
    }
    global.logger.info('RabbitMQ connection closed');
  } catch (error) {
    global.logger.error(`Error closing RabbitMQ connection: ${error.message}`);
  }
}
