## The goal

EntityFramework.Utilities will provide some enhancements to the core Entity Framework that the EF team hasn't yet added for us. Suggestions are welcome! Pull requests are even more welcome:)

## Installing

Nuget package https://www.nuget.org/packages/EFUtilities/ (Right now it doesn't work with EF6)

## Examples

### DeleteAll

```c#
public static int DeleteAll<T>(this DbContext source, Expression<Func<T, bool>> predicate) where T : class
```

This method will delete all Entities matching the predicate. But instead of the normal way to do this with EF (Load them into memory then delete them one by one) this method will create a Sql Query that deletes all items in one single call to the database.

```c#
               using (var db = new Context())
                {
                    var limit = DateTime.Today.AddDays(-5000);
                    db.DeleteAll<BlogPost>(p => p.Created > limit);
                }
```

You need to include a using EntityFramework.Utilities; for the extension method to be found.

**Limitations:** This method works by parsing the SQL generated when the predicate was used in a where clause. Aliases are removed when creating the delete clause so joins/subqueries are NOT supported/tested. Feel free to test if it works an if you have any idea of how to make it work I'm interested in supporting it if it doesn't add too much complexity. No contraints are checked by EF (though sql constraints are)

**Warning:** Because you are removing items directly from the database the context might still think they exist. If you have made any changes to a tracked entity that is then deleted by the query you will see some issues if you call SaveChanges on the context. 

### InsertAll

```c#
public static void InsertAll<T>(this DbContext source, IEnumerable<T> items) where T : class
```

This method uses the SqlBulkInserter to insert the items instead of adding them one by one as you normally would do with EF. The benefit is superior performance, the disadvantage is that EF will no longer validate any contraits for you and you will not get the ids back. 

```c#
           using (var db = new Context())
            {
                var list = new List<BlogPost>(10000);
                for (int i = 0; i < 10000; i++)
                {
                    var p = BlogPost.Create("T" + i, DateTime.Today.AddDays(i - 10000));
                    list.Add(p);
                }
                db.InsertAll(list);
            }
```

On my dev machine that runs at around 500ms instead of 10s using the 'out of the box method'(Optimised by disabling change tracking and validation) or 90s with changetracking and validation.
 
**Warning:** Right now this will not work if there are many DbSets of the object type you are trying to insert.  


### UpdateAll

```c#
public static int UpdateAll<T>(this DbContext source, Expression<Func<T, bool>> predicate, Expression<Func<T, object>> modifier) where T : class
```
This method works exactly like DeleteAll but instead of deleting the entities it created a query to update them.

The code:
```c#
using (var db = new Context())
{
   db.UpdateAll<BlogPost>(b => b.Title == "T1", b => b.Reads * 2);
}
```
Will generate the sql:

`UPDATE [dbo].[BlogPosts] SET [Reads] = [Reads] * 2 WHERE N'T1' = [Title]`

Right now only numbers and string manipulations are supported and the operators that are supported are [+, -, /, *]. Also only a single property might be updated. An improvement of this method is likely for future versions. 


## Performance
These methods are all about performance. Measuring performance should always be done in your context but some simple numbers might give you a hint:

### EF without optimization (Automatic changetracking enabled)
* Adding 10000 posts
* Iteration 0 took 84026 ms
* Traditional Delete
* Iteration 0 took 29393 ms

### EF with optimization (Automatic changetracking disabled)
* Adding 10000 posts
* Iteration 0 took 9965 ms
* Traditional Delete
* Iteration 0 took 2679 ms

### InsertAll and DeleteAll
* Adding 10000 posts batch
* Iteration 0 took 281 ms
* Batch Delete
* Iteration 0 took 35 ms
