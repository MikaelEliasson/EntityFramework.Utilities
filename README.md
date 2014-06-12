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

### Delete by query

This will let you delete all Entities matching the predicate. But instead of the normal way to do this with EF (Load them into memory then delete them one by one) this method will create a Sql Query that deletes all items in one single call to the database. Here is how a call looks:

```c#
var count = EFBatchOperation.For(db, db.BlogPosts).Where(b => b.Created < upper && b.Created > lower && b.Title == "T2.0").Delete();
```


**Limitations:** This method works by parsing the SQL generated when the predicate was used in a where clause. Aliases are removed when creating the delete clause so joins/subqueries are NOT supported/tested. Feel free to test if it works an if you have any idea of how to make it work I'm interested in supporting it if it doesn't add too much complexity. No constraints are checked by EF (though sql constraints are)

**Warning:** Because you are removing items directly from the database the context might still think they exist. If you have made any changes to a tracked entity that is then deleted by the query you will see some issues if you call SaveChanges on the context. 

### Batch insert entities


Allows you to insert many entities in a very performant way instead of adding them one by one as you normally would do with EF. The benefit is superior performance, the disadvantage is that EF will no longer validate any contraits for you and you will not get the ids back if they are store generated. 

```c#
            using (var db = Context.Sql())
            {
                EFBatchOperation.For(db, db.BlogPosts).InsertAll(list);
            }
```

On my dev machine that runs at around 500ms instead of 10s using the 'out of the box method'(Optimised by disabling change tracking and validation) or 90s with changetracking and validation. 

SqlBulkCopy is used under the covers if you are running against SqlServer. If you are not running against SqlServer it will default to doing the normal inserts.
 
**Warning:** It should be able to handle renamed columns but I'm not 100% sure if it handle all cases of removed columns  

### Update by query


Let you update many entities in one sql query instead of loading them into memory and, modifing them and saving back to db.

```c#
            using (var db = new Context())
            {
                EFBatchOperation.For(db, db.Comments).Where(x => x.Text == "a").Update(x => x.Reads, x => x.Reads + 1);
            }
```
The modifications you can do should be what EF can support in it's queries. For example it's possible to do:


```c#count = EFBatchOperation.For(db, db.BlogPosts).Where(b => b.Title == "T2").Update(b => b.Created, b => DbFunctions.AddDays(b.Created, 1));```

To incrememt the day one step. This method should be able to handle any renamed columns but the pitfall here is that this works internally by running the modifier through a where clause to get the SQL and than this where clause is transformed to a set clause. The rules for set and where are different so this might not always be valid. This is the most fragile of the methods but you can always test and if it doesn't work open an issue on github and it might get fixed. 

## Caveats and overall design decisions
There are some special things to keep in mind when using EFUtilities. Here is a list.

- The bulk insert should be stable but remember if you use database assigned id's it will NOT return these like normal EF inserts do.
- Update and Delete is quite "hacky". They work by pretending it was a regular where and take the generated sql (not hitting db) then altering this sql for update or delete. If you use joins that might not work. It will work for simple things but if you are doing complex stuff it might not be powerful enough. 
- All 3 methods works in a way that doesn't really align with the DbContext, things are saved before SaveChanges are called, new ids are not returned and changes aren't synced to entities loaded into the context. This is the reason the methods are placed on ``EFBatchOperation``, to make sure it's clear this is working outside the normal conventions.
- Because particulary Update/Delete are implemented using hacks that depend on the generated sql from EF I would encourage you to add integrations tests whenever you use these methods. Actually I would encourage you to always do that but that is another story. With integrations tests you will be warned if an EF update break EFUtilities and avoid any unpleasant suprises in production. 

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

