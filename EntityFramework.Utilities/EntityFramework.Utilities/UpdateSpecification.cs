using System;
using System.Linq.Expressions;

namespace EntityFramework.Utilities
{
    public class UpdateSpecification<T>
    {
        /// <summary>
        /// Set each column you want to update, Columns that belong to the primary key cannot be updated.
        /// </summary>
        /// <param name="properties"></param>
        /// <returns></returns>
        public UpdateSpecification<T> ColumnsToUpdate(params Expression<Func<T, object>>[] properties)
        {
            Properties = properties;
            return this;
        }

        public Expression<Func<T, object>>[] Properties { get; set; }
    }
}
