using MongoDB.Driver;
using WorkforceAPI.Models.MongoDB;

namespace WorkforceAPI.Repositories;

public class LeaveRequestRepository : ILeaveRequestRepository
{
    private readonly IMongoDatabase _mongoDatabase;
    private IMongoCollection<LeaveRequest>? _collection;

    public LeaveRequestRepository(IMongoDatabase mongoDatabase)
    {
        _mongoDatabase = mongoDatabase;
    }

    private IMongoCollection<LeaveRequest> Collection
    {
        get
        {
            if (_collection == null)
            {
                _collection = _mongoDatabase.GetCollection<LeaveRequest>("LeaveRequests");
            }
            return _collection;
        }
    }

    public async Task<LeaveRequest?> GetByIdAsync(string id)
    {
        var filter = Builders<LeaveRequest>.Filter.Eq(x => x.Id, id);
        return await Collection.Find(filter).FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<LeaveRequest>> GetAllAsync()
    {
        return await Collection.Find(_ => true).SortByDescending(x => x.CreatedAt).ToListAsync();
    }

    public async Task<IEnumerable<LeaveRequest>> GetByEmployeeIdAsync(Guid employeeId)
    {
        var filter = Builders<LeaveRequest>.Filter.Eq(x => x.EmployeeId, employeeId);
        return await Collection.Find(filter).SortByDescending(x => x.CreatedAt).ToListAsync();
    }

    public async Task<IEnumerable<LeaveRequest>> GetByStatusAsync(string status)
    {
        var filter = Builders<LeaveRequest>.Filter.Eq(x => x.Status, status);
        return await Collection.Find(filter).SortByDescending(x => x.CreatedAt).ToListAsync();
    }

    public async Task<IEnumerable<LeaveRequest>> GetByLeaveTypeAsync(string leaveType)
    {
        var filter = Builders<LeaveRequest>.Filter.Eq(x => x.LeaveType, leaveType);
        return await Collection.Find(filter).SortByDescending(x => x.CreatedAt).ToListAsync();
    }

    public async Task<LeaveRequest> CreateAsync(LeaveRequest leaveRequest)
    {
        leaveRequest.CreatedAt = DateTime.UtcNow;
        leaveRequest.UpdatedAt = DateTime.UtcNow;
        
        // Initialize approval history with initial pending status
        if (leaveRequest.ApprovalHistory == null || leaveRequest.ApprovalHistory.Count == 0)
        {
            leaveRequest.ApprovalHistory = new List<ApprovalHistoryEntry>
            {
                new ApprovalHistoryEntry
                {
                    Status = "Pending",
                    ChangedBy = leaveRequest.EmployeeName,
                    ChangedAt = DateTime.UtcNow,
                    Comments = "Leave request submitted"
                }
            };
        }

        await Collection.InsertOneAsync(leaveRequest);
        return leaveRequest;
    }

    public async Task<LeaveRequest> UpdateStatusAsync(string id, string status, string changedBy, string? comments = null)
    {
        var filter = Builders<LeaveRequest>.Filter.Eq(x => x.Id, id);
        var leaveRequest = await Collection.Find(filter).FirstOrDefaultAsync();
        
        if (leaveRequest == null)
        {
            throw new InvalidOperationException($"Leave request with ID {id} not found");
        }

        // Update status
        leaveRequest.Status = status;
        leaveRequest.UpdatedAt = DateTime.UtcNow;

        // Add to approval history
        if (leaveRequest.ApprovalHistory == null)
        {
            leaveRequest.ApprovalHistory = new List<ApprovalHistoryEntry>();
        }

        leaveRequest.ApprovalHistory.Add(new ApprovalHistoryEntry
        {
            Status = status,
            ChangedBy = changedBy,
            ChangedAt = DateTime.UtcNow,
            Comments = comments
        });

        var update = Builders<LeaveRequest>.Update
            .Set(x => x.Status, status)
            .Set(x => x.UpdatedAt, DateTime.UtcNow)
            .Set(x => x.ApprovalHistory, leaveRequest.ApprovalHistory);

        await Collection.UpdateOneAsync(filter, update);
        return leaveRequest;
    }
}
