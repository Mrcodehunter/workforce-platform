import dotenv from 'dotenv';
import winston from 'winston';
import cron from 'node-cron';
import { connectRabbitMQ, startConsuming } from './consumers/scheduleConsumer.js';
import { generateDashboardReport } from './reportGenerator.js';

dotenv.config();

// Configure Winston logger
const logger = winston.createLogger({
  level: 'info',
  format: winston.format.combine(
    winston.format.timestamp({ format: 'YYYY-MM-DD HH:mm:ss' }),
    winston.format.printf(({ timestamp, level, message }) => {
      return `[${timestamp}] [${level.toUpperCase()}] [ReportWorker] ${message}`;
    })
  ),
  transports: [
    new winston.transports.Console()
  ]
});

// Global logger
global.logger = logger;

async function startWorker() {
  try {
    logger.info('Starting Report Generator Worker');

    // Connect to RabbitMQ
    await connectRabbitMQ();
    logger.info('Connected to RabbitMQ');

    // Start consuming messages
    await startConsuming();
    logger.info('Started consuming messages from RabbitMQ');

    // Schedule periodic report generation (every hour)
    cron.schedule('0 * * * *', async () => {
      logger.info('Running scheduled report generation');
      try {
        await generateDashboardReport();
        logger.info('Scheduled report generation completed');
      } catch (error) {
        logger.error(`Error in scheduled report generation: ${error.message}`);
      }
    });

    logger.info('Report Generator Worker started successfully');
    logger.info('Scheduled to run every hour at minute 0');

    // Generate initial report on startup
    logger.info('Generating initial report on startup');
    await generateDashboardReport();

  } catch (error) {
    logger.error(`Failed to start worker: ${error.message}`);
    process.exit(1);
  }
}

// Handle graceful shutdown
process.on('SIGINT', () => {
  logger.info('Received SIGINT, shutting down gracefully');
  process.exit(0);
});

process.on('SIGTERM', () => {
  logger.info('Received SIGTERM, shutting down gracefully');
  process.exit(0);
});

// Start the worker
startWorker();
