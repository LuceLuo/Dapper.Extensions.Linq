using System;
using System.Linq;
using System.Reflection;
using Dapper.Extensions.Linq.Core.Attributes;
using Dapper.Extensions.Linq.Core.Enums;

namespace Dapper.Extensions.Linq.Mapper
{
    /// <summary>
    /// Automatically maps an entity to a table using a combination of reflection and naming conventions for keys.
    /// Allows for the addition of attributes.
    /// 
    /// <see cref="IgnoreAttribute"/>, <see cref="MapToAttribute"/>, <see cref="PrefixForColumnsAttribute"/>, <see cref="TableNameAttribute"/>.
    /// </summary>
    public class AutoClassMapper<T> : ClassMapper<T> where T : class
    {
        public AutoClassMapper()
        {
            AutoMap();
        }

        public override void Table(string tableName)
        {
            if (IsDefined(EntityType, typeof(TableNameAttribute)))
                tableName = ((TableNameAttribute)EntityType.GetTypeInfo().GetCustomAttribute(typeof(TableNameAttribute))).Name;
            base.Table(tableName);
        }

        protected override void Schema(string schemaName)
        {
            if (IsDefined(EntityType, typeof(SchemaAttribute)))
                schemaName = ((SchemaAttribute)EntityType.GetTypeInfo().GetCustomAttribute(typeof(SchemaAttribute))).Name;
            base.Table(schemaName);
        }

        private void AutoMap()
        {
            Type type = typeof(T);

            string columnPrefix = string.Empty;

            if (IsDefined(type, typeof(PrefixForColumnsAttribute)))
                columnPrefix = ((PrefixForColumnsAttribute)type.GetTypeInfo().GetCustomAttribute(typeof(PrefixForColumnsAttribute))).Prefix;

            foreach (PropertyInfo propertyInfo in EntityType.GetProperties())
            {
                PropertyMap propertyMap;
                if (IsDefined(propertyInfo, typeof(IgnoreAttribute)))
                    propertyMap = Map(propertyInfo, false).Ignore();
                else if (IsDefined(propertyInfo, typeof(MapToAttribute)))
                    propertyMap = Map(propertyInfo, false).Column(((MapToAttribute)propertyInfo.GetCustomAttribute(typeof(MapToAttribute))).DatabaseColumn);
                else if (string.IsNullOrEmpty(columnPrefix) == false)
                    propertyMap = Map(propertyInfo, false).Column(string.Format("{0}{1}", columnPrefix, propertyInfo.Name));
                else propertyMap = Map(propertyInfo, false);

                if (Properties.Any(e => e.KeyType != KeyType.NotAKey)) continue;

                if (string.Equals(propertyMap.PropertyInfo.Name, "id", StringComparison.OrdinalIgnoreCase))
                    propertyMap.Key(PropertyTypeKeyTypeMapping.ContainsKey(propertyMap.PropertyInfo.PropertyType) ?
                        PropertyTypeKeyTypeMapping[propertyMap.PropertyInfo.PropertyType] :
                        KeyType.Assigned);
            }
        }

        public static bool IsDefined(Type type1, Type type2)
        {
            return type1.GetProperties().Any(p => p.IsDefined(type2));
        }
        public static bool IsDefined(PropertyInfo propertyInfo, Type type2)
        {
            return propertyInfo.IsDefined(type2);
        }
    }

}