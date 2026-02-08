# Project After Data Storage Fix

## Problem

The "after" snapshot for projects was not being stored properly in audit logs. Some project updates showed null or incomplete "after" data.

## Root Cause

The issue was related to **Entity Framework tracking**. After calling `UpdateAsync()` or `CreateAsync()`, the entity remains tracked by the DbContext. When `GetByIdAsync()` is called immediately after, Entity Framework may:

1. Return the tracked entity instead of querying the database
2. Return an entity without fully loaded navigation properties
3. Return stale data that doesn't reflect the latest changes

## Solution

### 1. Added `ReloadWithNavigationPropertiesAsync` Method

Created a new method in `ProjectRepository` that:
- Detaches any existing tracked entity with the same ID
- Reloads the entity fresh from the database using `AsNoTracking()`
- Ensures all navigation properties are loaded

```csharp
public async Task<Project?> ReloadWithNavigationPropertiesAsync(Guid id)
{
    // Detach any existing tracked entity with this ID
    var tracked = await _context.Projects.FindAsync(id);
    if (tracked != null)
    {
        _context.Entry(tracked).State = EntityState.Detached;
    }
    
    // Reload fresh from database with all navigation properties
    return await _context.Projects
        .Include(p => p.ProjectMembers)
            .ThenInclude(pm => pm.Employee)
        .Include(p => p.Tasks.Where(t => !t.IsDeleted))
            .ThenInclude(t => t.AssignedToEmployee)
        .AsSplitQuery()
        .AsNoTracking()  // Important: prevents tracking issues
        .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);
}
```

### 2. Updated `UpdateAsync` to Detach Entity

Modified `UpdateAsync` to detach the entity after saving:

```csharp
public async Task<Project> UpdateAsync(Project project)
{
    _context.Projects.Update(project);
    await _context.SaveChangesAsync();
    
    // Detach the entity to force fresh reload
    _context.Entry(project).State = EntityState.Detached;
    
    return project;
}
```

### 3. Updated `CreateAsync` to Detach Entity

Modified `CreateAsync` to detach the entity after saving:

```csharp
public async Task<Project> CreateAsync(Project project)
{
    _context.Projects.Add(project);
    await _context.SaveChangesAsync();
    
    // Detach the entity to force fresh reload with navigation properties
    _context.Entry(project).State = EntityState.Detached;
    
    return project;
}
```

### 4. Updated `ProjectService` to Use Reload Method

Changed both `CreateAsync` and `UpdateAsync` in `ProjectService` to use `ReloadWithNavigationPropertiesAsync` instead of `GetByIdAsync`:

**Before:**
```csharp
var reloaded = await _repository.GetByIdAsync(result.Id);
```

**After:**
```csharp
var reloaded = await _repository.ReloadWithNavigationPropertiesAsync(result.Id);
```

## Benefits

✅ **Fresh Data**: Always gets the latest data from the database
✅ **Complete Navigation Properties**: All related entities (ProjectMembers, Tasks) are loaded
✅ **No Tracking Issues**: `AsNoTracking()` prevents EF from returning stale tracked entities
✅ **Consistent Behavior**: Same approach for both create and update operations

## Files Modified

- `WorkforceAPI/Repositories/IProjectRepository.cs` - Added `ReloadWithNavigationPropertiesAsync` method
- `WorkforceAPI/Repositories/ProjectRepository.cs` - Implemented reload method and updated create/update to detach entities
- `WorkforceAPI/Services/ProjectService.cs` - Updated to use reload method for after snapshots

## Testing

To verify the fix:

1. **Create a project** → Check audit log has complete "after" data with all properties
2. **Update a project** → Check audit log has complete "after" data with navigation properties
3. **Check MongoDB** → Verify the "after" field contains full project data including ProjectMembers and Tasks

The "after" snapshot should now always contain complete project data.
