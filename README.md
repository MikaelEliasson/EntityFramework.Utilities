## The goal

Performance! EF is quite fast in many cases nowdays but doing CUD over many entities is slooooow. This is a solution for that.  

EntityFramework.Utilities provides some batch operations for using EF that the EF team hasn't yet added for us. Suggestions are welcome! Pull requests are even more welcome:)

Right now it's mostly to targeted at EF on SQL server but adding providers should be simple. 

###Example

Here is a small extract from the performance section later in the document.

               Batch iteration with 25000 entities
               Insert entities: 281ms
               Update all entities with a: 163ms
               delete all entities with a: 18ms
               delete all entities: 107ms
               Standard iteration with 25000 entities
               Insert entities: 9601ms
               Update all entities with a: 457ms
               delete all entities with a: 250ms
               delete all entities: 5895ms


## Installing

Right now this only works for DbContext. If anyone want to make a failing test or provide a sample project for any of the other variants it will probably be easy to fix.

### EF 4-5

You need to manually select any of the O.1.xxx packages as the later packages are for EF V6.

Nuget package https://www.nuget.org/packages/EFUtilities/ 

### EF 6

Any package from 0.2.0 and up should work.

Nuget package https://www.nuget.org/packages/EFUtilities/ 

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

SqlBulkCopy is used under the covers if you are running against SqlServer. If you are not running against SqlServer it will default to doing the normal inserts.
 
**Warning:** Right now this will probably not work if there are many DbSets of the object type you are trying to insert.  


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
These methods are all about performance. Measuring performance should always be done in your context but some simple numbers might give you a hint.

The standard iteration is optimized in the sense that AutoDetectChangedEnabled = false; It would not be reasonable to delete/insert 25000 entities otherwise.

Here is a test run with EntitityFramework.Utilities on my laptop doing operations on a really simple object Comment(Text:string,Date:DateTime,Id:int,Reads:int)

               Batch iteration with 25 entities
               Insert entities: 23ms
               Update all entities with a: 4ms
               delete all entities with a: 2ms
               delete all entities: 1ms
               Standard iteration with 25 entities
               Insert entities: 12ms
               Update all entities with a: 6ms
               delete all entities with a: 3ms
               delete all entities: 7ms
               Batch iteration with 2500 entities
               Insert entities: 47ms
               Update all entities with a: 22ms
               delete all entities with a: 5ms
               delete all entities: 11ms
               Standard iteration with 2500 entities
               Insert entities: 905ms
               Update all entities with a: 46ms
               delete all entities with a: 22ms
               delete all entities: 552ms
               Batch iteration with 25000 entities
               Insert entities: 281ms
               Update all entities with a: 163ms
               delete all entities with a: 18ms
               delete all entities: 107ms
               Standard iteration with 25000 entities
               Insert entities: 9601ms
               Update all entities with a: 457ms
               delete all entities with a: 250ms
               delete all entities: 5895ms
               Batch iteration with 100000 entities
               Insert entities: 1048ms
               Update all entities with a: 442ms
               delete all entities with a: 60ms
               delete all entities: 292ms


This is on my ultrabook. Here I don't compare to anything so it's just to give you some overview about what to expect. Note that in the batchmode around 100k entities/sec are added when reaching larger datasets. 

