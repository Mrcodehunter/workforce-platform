import { MongoClient } from 'mongodb';
import pg from 'pg';

const { Client: PgClient } = pg;

/**
 * Generate dashboard report by aggregating data from PostgreSQL and MongoDB
 */
export async function generateDashboardReport() {
  let mongoClient = null;
  let pgClient = null;

  try {
    global.logger.info('Starting dashboard report generation...');

    // Connect to MongoDB
    const mongoUri = process.env.MONGO_URI || 'mongodb://admin:changeme@mongodb:27017';
    const mongoDbName = process.env.MONGO_DB || 'workforce_db';
    mongoClient = new MongoClient(mongoUri);
    await mongoClient.connect();
    const mongoDb = mongoClient.db(mongoDbName);

    // Connect to PostgreSQL
    pgClient = new PgClient({
      host: process.env.POSTGRES_HOST || 'postgres',
      port: parseInt(process.env.POSTGRES_PORT || '5432'),
      database: process.env.POSTGRES_DB || 'workforce_db',
      user: process.env.POSTGRES_USER || 'admin',
      password: process.env.POSTGRES_PASSWORD || 'changeme',
    });
    await pgClient.connect();

    // Aggregate data from PostgreSQL
    const employeeStats = await pgClient.query(`
      SELECT 
        COUNT(*) as total_employees,
        COUNT(*) FILTER (WHERE "IsActive" = true) as active_employees,
        COUNT(*) FILTER (WHERE "IsActive" = false) as inactive_employees,
        AVG("Salary") as avg_salary
      FROM "Employees"
      WHERE "IsDeleted" = false
    `);

    const projectStats = await pgClient.query(`
      SELECT 
        COUNT(*) as total_projects,
        COUNT(*) FILTER (WHERE "Status" = 'Active') as active_projects,
        COUNT(*) FILTER (WHERE "Status" = 'Completed') as completed_projects
      FROM "Projects"
      WHERE "IsDeleted" = false
    `);

    const taskStats = await pgClient.query(`
      SELECT 
        COUNT(*) as total_tasks,
        COUNT(*) FILTER (WHERE "Status" = 'ToDo') as todo_tasks,
        COUNT(*) FILTER (WHERE "Status" = 'InProgress') as in_progress_tasks,
        COUNT(*) FILTER (WHERE "Status" = 'Done') as done_tasks
      FROM "Tasks"
      WHERE "IsDeleted" = false
    `);

    // Aggregate data from MongoDB
    const leaveRequestStats = await mongoDb.collection('LeaveRequests').aggregate([
      {
        $group: {
          _id: '$status',
          count: { $sum: 1 },
        },
      },
    ]).toArray();

    const auditLogStats = await mongoDb.collection('AuditLogs').aggregate([
      {
        $group: {
          _id: '$entityType',
          count: { $sum: 1 },
        },
      },
    ]).toArray();

    // Build report
    const report = {
      reportType: 'DashboardSummary',
      generatedAt: new Date(),
      data: {
        employees: {
          total: parseInt(employeeStats.rows[0].total_employees || 0),
          active: parseInt(employeeStats.rows[0].active_employees || 0),
          inactive: parseInt(employeeStats.rows[0].inactive_employees || 0),
          averageSalary: parseFloat(employeeStats.rows[0].avg_salary || 0),
        },
        projects: {
          total: parseInt(projectStats.rows[0].total_projects || 0),
          active: parseInt(projectStats.rows[0].active_projects || 0),
          completed: parseInt(projectStats.rows[0].completed_projects || 0),
        },
        tasks: {
          total: parseInt(taskStats.rows[0].total_tasks || 0),
          todo: parseInt(taskStats.rows[0].todo_tasks || 0),
          inProgress: parseInt(taskStats.rows[0].in_progress_tasks || 0),
          done: parseInt(taskStats.rows[0].done_tasks || 0),
        },
        leaveRequests: leaveRequestStats.reduce((acc, stat) => {
          acc[stat._id] = stat.count;
          return acc;
        }, {}),
        auditLogs: auditLogStats.reduce((acc, stat) => {
          acc[stat._id] = stat.count;
          return acc;
        }, {}),
      },
    };

    // Save report to MongoDB
    await mongoDb.collection('Reports').insertOne(report);

    global.logger.info('Dashboard report generated and saved successfully');
    return report;
  } catch (error) {
    global.logger.error(`Error generating dashboard report: ${error.message}`);
    throw error;
  } finally {
    if (mongoClient) {
      await mongoClient.close();
    }
    if (pgClient) {
      await pgClient.end();
    }
  }
}
