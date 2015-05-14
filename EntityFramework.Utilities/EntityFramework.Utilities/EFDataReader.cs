using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Reflection;

namespace EntityFramework.Utilities
{
    public class EFDataReader<T> : DbDataReader
    {
        public IEnumerable<T> Items { get; set; }
        public IEnumerator<T> Enumerator { get; set; }
        public IList<string> Properties { get; set; }
        public List<Func<T, object>> Accessors { get; set; }

        public EFDataReader(IEnumerable<T> items, IEnumerable<ColumnMapping> properties)
        {
            Properties = properties.Select(p => p.NameOnObject).ToList();
            Accessors = properties.Select(p =>
            {
                if (p.StaticValue != null)
                {
                    Func<T,object> func = x => p.StaticValue;
                    return func;
                }

                var parts = p.NameOnObject.Split('.');

                PropertyInfo info = typeof(T).GetProperty(parts[0]);
                var method = typeof(EFDataReader<T>).GetMethod("MakeDelegate");
                var generic = method.MakeGenericMethod(info.PropertyType);

                var getter = (Func<T, object>)generic.Invoke(this, new object[] { info.GetGetMethod(true) });

                var temp = info;
                foreach (var part in parts.Skip(1))
                {
                    var i = temp.PropertyType.GetProperty(part);
                    var g =  i.GetGetMethod();

                    var old = getter;
                    getter = x => g.Invoke(old(x), null);

                    temp = i;
                }

                
                return getter;
            }).ToList();
            Items = items;
            Enumerator = items.GetEnumerator();
        }

        public static Func<T, object> MakeDelegate<U>(MethodInfo @get)
        {
            var f = (Func<T, U>)Delegate.CreateDelegate(typeof(Func<T, U>), @get);
            return t => f(t);
        }

        public override void Close()
        {
            this.Enumerator = null;
            this.Items = null;
        }

        public override int FieldCount
        {
            get
            {
                return Properties.Count;
            }
        }

        public override bool HasRows
        {
            get { return this.Items != null && this.Items.Any(); }
        }

        public override bool IsClosed
        {
            get { return Enumerator == null; }
        }

        public override bool Read()
        {
            return this.Enumerator.MoveNext();
        }

        public override int RecordsAffected
        {
            get { return this.Items.Count(); }
        }


        public override object GetValue(int ordinal)
        {
            return this.Accessors[ordinal](this.Enumerator.Current);
        }

        public override int GetOrdinal(string name)
        {
            return Properties.IndexOf(name);
        }

        #region Not implemented

        public override object this[string name]
        {
            get { throw new NotImplementedException(); }
        }

        public override object this[int ordinal]
        {
            get { throw new NotImplementedException(); }
        }

        public override bool IsDBNull(int ordinal)
        {
            throw new NotImplementedException();
        }

        public override bool NextResult()
        {
            throw new NotImplementedException();
        }

        public override int Depth
        {
            get { throw new NotImplementedException(); }
        }

        public override bool GetBoolean(int ordinal)
        {
            throw new NotImplementedException();
        }

        public override byte GetByte(int ordinal)
        {
            throw new NotImplementedException();
        }

        public override long GetBytes(int ordinal, long dataOffset, byte[] buffer, int bufferOffset, int length)
        {
            throw new NotImplementedException();
        }

        public override char GetChar(int ordinal)
        {
            throw new NotImplementedException();
        }

        public override long GetChars(int ordinal, long dataOffset, char[] buffer, int bufferOffset, int length)
        {
            throw new NotImplementedException();
        }

        public override string GetDataTypeName(int ordinal)
        {
            throw new NotImplementedException();
        }

        public override DateTime GetDateTime(int ordinal)
        {
            throw new NotImplementedException();
        }

        public override decimal GetDecimal(int ordinal)
        {
            throw new NotImplementedException();
        }

        public override double GetDouble(int ordinal)
        {
            throw new NotImplementedException();
        }

        public override System.Collections.IEnumerator GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public override Type GetFieldType(int ordinal)
        {
            throw new NotImplementedException();
        }

        public override float GetFloat(int ordinal)
        {
            throw new NotImplementedException();
        }

        public override Guid GetGuid(int ordinal)
        {
            throw new NotImplementedException();
        }

        public override short GetInt16(int ordinal)
        {
            throw new NotImplementedException();
        }

        public override int GetInt32(int ordinal)
        {
            throw new NotImplementedException();
        }

        public override long GetInt64(int ordinal)
        {
            throw new NotImplementedException();
        }

        public override string GetName(int ordinal)
        {
            throw new NotImplementedException();
        }

        public override DataTable GetSchemaTable()
        {
            throw new NotImplementedException();
        }

        public override string GetString(int ordinal)
        {
            throw new NotImplementedException();
        }

        public override int GetValues(object[] values)
        {
            throw new NotImplementedException();
        }

        #endregion

    }
}
